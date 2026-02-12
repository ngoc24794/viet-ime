using VietIME.Core.Models;

namespace VietIME.Core.Engines;

/// <summary>
/// Engine xử lý kiểu gõ Telex
/// Quy tắc Telex:
/// - Dấu: s=sắc, f=huyền, r=hỏi, x=ngã, j=nặng
/// - Mũ: aa=â, ee=ê, oo=ô, aw=ă, ow=ơ, uw=ư
/// - Đặc biệt: dd=đ, w sau u/o = ư/ơ
/// </summary>
public class TelexEngine : IInputEngine
{
    public string Name => "Telex";
    
    // Buffer lưu từ đang gõ
    private readonly List<char> _buffer = [];
    
    // Map phím dấu thanh Telex
    private static readonly Dictionary<char, VietnameseChar.ToneIndex> ToneKeys = new()
    {
        ['s'] = VietnameseChar.ToneIndex.Acute,  // Sắc
        ['f'] = VietnameseChar.ToneIndex.Grave,  // Huyền
        ['r'] = VietnameseChar.ToneIndex.Hook,   // Hỏi
        ['x'] = VietnameseChar.ToneIndex.Tilde,  // Ngã
        ['j'] = VietnameseChar.ToneIndex.Dot,    // Nặng
        ['z'] = VietnameseChar.ToneIndex.None,   // Xóa dấu
    };
    
    public ProcessKeyResult ProcessKey(char key, bool isShiftPressed)
    {
        var result = new ProcessKeyResult();
        char lowerKey = char.ToLower(key);

        // Xử lý phím tắt '[' = ư, ']' = ơ (toggle: lần 2 trả lại bracket)
        if (key == '[' || key == ']')
        {
            var bracketResult = TryProcessBracket(key);
            if (bracketResult.HasValue)
            {
                result.Handled = true;
                result.BackspaceCount = bracketResult.Value.backspaceCount;
                result.OutputText = bracketResult.Value.output;
                result.CurrentBuffer = GetBuffer();
                return result;
            }
            // Bracket không xử lý được → reset buffer, để qua
            Reset();
            result.Handled = false;
            return result;
        }

        // Nếu là ký tự không phải chữ cái -> reset buffer
        if (!char.IsLetter(key))
        {
            Reset();
            result.Handled = false;
            return result;
        }

        // Auto-reset: Nếu buffer đã có dấu thanh và ký tự mới là phụ âm
        // thì có thể là bắt đầu từ mới → undo dấu + reset buffer
        // Ví dụ: "lỗi" xong gõ "n" → reset, bắt đầu từ "n..."
        // Ví dụ: "pú" + "s" + "h" → undo dấu → "push"
        // Ngoại trừ các phụ âm cuối hợp lệ: c, m, n, p, t
        // (g, h chỉ hợp lệ khi ghép: ng, nh, ch)
        if (_buffer.Count > 0 && !IsVowel(key) && lowerKey != 'w' && HasToneInBuffer())
        {
            char lastChar = _buffer[^1];
            if (IsVowel(lastChar))
            {
                // Sau nguyên âm: chỉ c, m, n, p, t là phụ âm cuối đơn hợp lệ
                // g, h KHÔNG hợp lệ đứng một mình sau nguyên âm
                if (!(lowerKey is 'c' or 'm' or 'n' or 'p' or 't'))
                {
                    UndoAndReset(result, key);
                    return result;
                }
            }
            else if (_buffer.Count >= 2)
            {
                // Ký tự cuối là phụ âm cuối, thêm phụ âm nữa chỉ hợp lệ nếu ghép được
                // ng, nh, ch là các phụ âm cuối ghép hợp lệ
                char lastLower = char.ToLower(lastChar);
                bool canCombine = (lastLower == 'n' && lowerKey is 'g' or 'h') ||
                                  (lastLower == 'c' && lowerKey == 'h');
                if (!canCombine)
                {
                    UndoAndReset(result, key);
                    return result;
                }
            }
        }

        // Xử lý phím dấu thanh (s, f, r, x, j, z)
        if (ToneKeys.TryGetValue(lowerKey, out var toneIndex))
        {
            var toneResult = TryApplyTone(toneIndex, key);
            if (toneResult.HasValue)
            {
                result.Handled = true;
                result.BackspaceCount = toneResult.Value.backspaceCount;
                result.OutputText = toneResult.Value.output;
                result.CurrentBuffer = GetBuffer();
                return result;
            }
        }
        
        // Xử lý 'd' -> 'đ'
        if (lowerKey == 'd')
        {
            var dResult = TryProcessD(key, isShiftPressed);
            if (dResult.HasValue)
            {
                result.Handled = true;
                result.BackspaceCount = dResult.Value.backspaceCount;
                result.OutputText = dResult.Value.output;
                result.CurrentBuffer = GetBuffer();
                return result;
            }
        }
        
        // Xử lý 'w' -> ă, ơ, ư
        if (lowerKey == 'w')
        {
            var wResult = TryProcessW(key);
            if (wResult.HasValue)
            {
                result.Handled = true;
                result.BackspaceCount = wResult.Value.backspaceCount;
                result.OutputText = wResult.Value.output;
                result.CurrentBuffer = GetBuffer();
                return result;
            }
        }
        
        // Xử lý nguyên âm đôi (aa, ee, oo)
        if (lowerKey is 'a' or 'e' or 'o')
        {
            var doubleResult = TryProcessDoubleVowel(key);
            if (doubleResult.HasValue)
            {
                result.Handled = true;
                result.BackspaceCount = doubleResult.Value.backspaceCount;
                result.OutputText = doubleResult.Value.output;
                result.CurrentBuffer = GetBuffer();
                return result;
            }
        }
        
        // Không xử lý đặc biệt -> thêm vào buffer
        _buffer.Add(key);
        result.Handled = false;
        result.CurrentBuffer = GetBuffer();
        return result;
    }
    
    /// <summary>
    /// Tìm vị trí nguyên âm để đặt dấu (theo quy tắc tiếng Việt)
    /// </summary>
    private int FindVowelPositionForTone()
    {
        // Quy tắc đặt dấu tiếng Việt:
        // 1. Nếu có nguyên âm mũ/móc (ê, ô, ơ, â, ă, ư) -> đặt dấu vào đó
        // 2. Với nguyên âm đôi bắt đầu bằng i/u (ie, ia, ua, uo, ưa, ươ) -> đặt dấu vào nguyên âm sau
        // 3. Với oa, oe, uy -> đặt dấu vào nguyên âm sau
        // 4. Trường hợp khác: nếu kết thúc bằng phụ âm -> nguyên âm đầu, ngược lại -> nguyên âm sau
        
        var vowelPositions = new List<int>();
        
        for (int i = 0; i < _buffer.Count; i++)
        {
            if (IsVowel(_buffer[i]))
            {
                vowelPositions.Add(i);
            }
        }
        
        if (vowelPositions.Count == 0)
            return -1;
        
        if (vowelPositions.Count == 1)
            return vowelPositions[0];
        
        // Tìm nhóm nguyên âm liền nhau cuối cùng
        var lastGroup = new List<int>();
        for (int i = vowelPositions.Count - 1; i >= 0; i--)
        {
            if (lastGroup.Count == 0 || vowelPositions[i] == lastGroup[0] - 1)
            {
                lastGroup.Insert(0, vowelPositions[i]);
            }
            else
            {
                break;
            }
        }
        
        if (lastGroup.Count == 1)
            return lastGroup[0];
        
        if (lastGroup.Count >= 2)
        {
            char firstVowel = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[lastGroup[0]]));
            char secondVowel = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[lastGroup[1]]));
            
            // ƯƠ -> dấu vào Ơ (nguyên âm SAU) khi có phụ âm sau
            // Ví dụ: được, mướn, dường
            // PHẢI kiểm tra TRƯỚC khi tìm nguyên âm có mũ/móc
            if (firstVowel == 'ư' && secondVowel == 'ơ')
            {
                int lastVowelPos = lastGroup[^1];
                bool hasConsonantAfter = lastVowelPos < _buffer.Count - 1 && 
                                         !IsVowel(_buffer[lastVowelPos + 1]);
                // Nếu có phụ âm sau -> dấu vào ơ
                if (hasConsonantAfter)
                {
                    return lastGroup[1];
                }
            }
            
            // oa, oe, oă -> dấu vào nguyên âm SAU (a, e, ă)
            // Ví dụ: hoà, toé, loạn
            if (firstVowel == 'o' && secondVowel is 'a' or 'e' or 'ă')
            {
                return lastGroup[1];
            }

            // qu + nguyên âm -> dấu vào nguyên âm SAU (không phải u)
            // Ví dụ: quá, quẻ, quỳ, quốc (q không phải nguyên âm, u đi với q)
            if (firstVowel == 'u' && lastGroup[0] > 0 &&
                char.ToLower(_buffer[lastGroup[0] - 1]) == 'q')
            {
                return lastGroup[1];
            }

            // gi + nguyên âm -> dấu vào nguyên âm SAU (không phải i)
            // Ví dụ: giúp, giải, giờ (g + i là phụ âm ghép, i không mang dấu)
            if (firstVowel == 'i' && lastGroup[0] > 0 &&
                char.ToLower(_buffer[lastGroup[0] - 1]) == 'g')
            {
                return lastGroup[1];
            }
        }
        
        // Ưu tiên nguyên âm có mũ/móc (ê, ô, ơ, â, ă, ư)
        foreach (int pos in lastGroup)
        {
            char c = _buffer[pos];
            // Lấy nguyên âm không dấu thanh (giữ mũ/móc)
            char vowelWithoutTone = char.ToLower(VietnameseChar.GetVowelWithoutTone(c));
            if (vowelWithoutTone is 'ê' or 'ô' or 'ơ' or 'â' or 'ă' or 'ư')
            {
                return pos;
            }
        }
        
        if (lastGroup.Count >= 2)
        {
            char firstV = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[lastGroup[0]]));
            char secondV = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[lastGroup[1]]));
            int lastVowelPos = lastGroup[^1];
            bool hasConsonantAfter = lastVowelPos < _buffer.Count - 1 &&
                                     !IsVowel(_buffer[lastVowelPos + 1]);

            // ua, uo + phụ âm cuối → dấu vào nguyên âm SAU
            // Ví dụ: muốn (uo+n), thuận (ua+n), chuộc (uo+c)
            if (firstV == 'u' && secondV is 'a' or 'o' && hasConsonantAfter)
            {
                return lastGroup[1];
            }

            // ia, ie + phụ âm cuối → dấu vào nguyên âm SAU
            // Ví dụ: tiến (ie+n), kiến (ie+n), miếng (ie+ng)
            if (firstV == 'i' && secondV is 'a' or 'e' && hasConsonantAfter)
            {
                return lastGroup[1];
            }

            // Các trường hợp còn lại → dấu vào nguyên âm ĐẦU
            // Ví dụ: lại (ai), mùa (ua không có phụ âm cuối), mía (ia), chạy (ay), hỏi (oi)
            return lastGroup[0];
        }
        
        // 3 nguyên âm -> giữa
        return lastGroup[1];
    }
    
    private bool IsVowel(char c)
    {
        char lower = char.ToLower(c);
        return lower is 'a' or 'ă' or 'â' or 'e' or 'ê' or 'i' or 'o' or 'ô' or 'ơ' or 'u' or 'ư' or 'y'
               || VietnameseChar.IsVietnameseVowel(c);
    }
    
    private (int backspaceCount, string output)? TryApplyTone(VietnameseChar.ToneIndex tone, char originalKey)
    {
        // Không đặt dấu nếu từ không thể là tiếng Việt
        if (!CouldBeVietnamese())
            return null;

        // Áp dụng các quy tắc thông minh trước khi đặt dấu
        AutoCorrectDPattern();      // d + vowel + d -> đ + vowel
        AutoConvertDWithUO();       // d + uo/ưo -> đ + uo/ưo
        AutoConvertUoToUoHorn();    // ưo + consonant -> ươ

        int vowelPos = FindVowelPositionForTone();
        
        if (vowelPos < 0)
        {
            // Không có nguyên âm -> thêm key vào buffer như bình thường
            return null;
        }
        
        char oldVowel = _buffer[vowelPos];
        var currentTone = VietnameseChar.GetToneIndex(oldVowel);
        
        // Toggle: Nếu nguyên âm đã có dấu giống dấu đang gõ -> xoá dấu và thêm ký tự gốc
        // Ví dụ: tẻ + r -> ter (xoá dấu hỏi, thêm 'r')
        // Gửi lại TOÀN BỘ buffer để đảm bảo đúng trên mọi terminal
        if (currentTone == tone && tone != VietnameseChar.ToneIndex.None)
        {
            // Xoá dấu thanh
            char vowelWithoutTone = VietnameseChar.ApplyTone(oldVowel, VietnameseChar.ToneIndex.None);
            _buffer[vowelPos] = vowelWithoutTone;
            
            // Thêm ký tự dấu gốc vào buffer
            _buffer.Add(originalKey);
            
            // Gửi toàn bộ buffer: xoá hết ký tự cũ trên màn hình, ghi lại từ đầu
            // Số ký tự trên màn hình = buffer.Count - 1 (trừ ký tự mới thêm chưa hiển)
            int toggleBackspace = _buffer.Count - 1;
            string toggleOutput = new string(_buffer.ToArray());
            
            return (toggleBackspace, toggleOutput);
        }
        
        char newVowel = VietnameseChar.ApplyTone(oldVowel, tone);

        if (newVowel == oldVowel)
        {
            // Nếu đang xoá dấu (z) mà nguyên âm không có dấu thanh,
            // thử xoá mũ/móc (ô→o, ê→e, â→a, ư→u, ơ→o, ă→a)
            if (tone == VietnameseChar.ToneIndex.None)
            {
                return TryRemoveAllDiacritics();
            }
            // Không thay đổi -> trả về key gốc
            return null;
        }
        
        // Cập nhật buffer
        _buffer[vowelPos] = newVowel;
        
        // Tính số backspace và output
        int backspaceCount = _buffer.Count - vowelPos;
        string output = new string(_buffer.Skip(vowelPos).ToArray());
        
        return (backspaceCount, output);
    }

    /// <summary>
    /// Xoá tất cả dấu thanh + mũ/móc trong buffer (gõ z khi không có dấu thanh)
    /// Ví dụ: "púsh" + z → "push", "đường" + z → "duong"
    /// </summary>
    private (int backspaceCount, string output)? TryRemoveAllDiacritics()
    {
        bool changed = false;
        int firstChangedPos = _buffer.Count;

        for (int i = 0; i < _buffer.Count; i++)
        {
            char c = _buffer[i];

            // Xoá dấu thanh
            var tone = VietnameseChar.GetToneIndex(c);
            if (tone != VietnameseChar.ToneIndex.None)
            {
                c = VietnameseChar.ApplyTone(c, VietnameseChar.ToneIndex.None);
                _buffer[i] = c;
                changed = true;
                if (i < firstChangedPos) firstChangedPos = i;
            }

            // Xoá mũ/móc: ô→o, ê→e, â→a, ư→u, ơ→o, ă→a, đ→d
            char lower = char.ToLower(c);
            bool isUpper = char.IsUpper(c);
            char? replaced = lower switch
            {
                'ô' => 'o',
                'ê' => 'e',
                'â' => 'a',
                'ư' => 'u',
                'ơ' => 'o',
                'ă' => 'a',
                'đ' => 'd',
                _ => null
            };

            if (replaced.HasValue)
            {
                _buffer[i] = isUpper ? char.ToUpper(replaced.Value) : replaced.Value;
                changed = true;
                if (i < firstChangedPos) firstChangedPos = i;
            }
        }

        if (!changed)
            return null;

        // Thêm 'z' vào buffer (ký tự gốc đi qua)
        _buffer.Add('z');

        int bs = _buffer.Count - firstChangedPos - 1; // -1 vì 'z' mới thêm không cần backspace
        string output = new string(_buffer.Skip(firstChangedPos).ToArray());

        return (bs, output);
    }

    /// <summary>
    /// Quy tắc thông minh: Tự động chuyển pattern 'd' + nguyên âm + 'd' thành 'đ' + nguyên âm
    /// Ví dụ: "dud" -> "đu", "did" -> "đi", "duod" -> "đuo"
    /// Điều này cho phép gõ linh hoạt hơn khi không gõ 'dd' liền nhau
    /// </summary>
    private void AutoCorrectDPattern()
    {
        // Tìm pattern: d + nguyên âm(s) + d
        // Chỉ xử lý khi 'd' đầu là chữ thường (chưa phải 'đ')
        if (_buffer.Count < 3)
            return;
        
        // Kiểm tra 'd' đầu tiên
        char first = _buffer[0];
        if (char.ToLower(first) != 'd')
            return;
        
        // Nếu đã là 'đ' rồi thì bỏ qua
        if (first == 'đ' || first == 'Đ')
            return;
        
        // Tìm 'd' thứ hai sau các nguyên âm
        int secondDPos = -1;
        bool hasVowelBetween = false;
        
        for (int i = 1; i < _buffer.Count; i++)
        {
            char c = _buffer[i];
            if (IsVowel(c))
            {
                hasVowelBetween = true;
            }
            else if (char.ToLower(c) == 'd' && hasVowelBetween)
            {
                secondDPos = i;
                break;
            }
        }
        
        if (secondDPos > 0 && hasVowelBetween)
        {
            // Chuyển 'd' đầu thành 'đ'
            bool isUpper = char.IsUpper(first);
            _buffer[0] = isUpper ? 'Đ' : 'đ';
            
            // Xóa 'd' thứ hai
            _buffer.RemoveAt(secondDPos);
        }
    }
    
    /// <summary>
    /// Quy tắc thông minh: Tự động chuyển 'd' đầu từ thành 'đ' khi có pattern 'ươ' hoặc 'uo'
    /// Ví dụ: "duo" + w -> "đươ" (thay vì "duơ")
    /// </summary>
    private void AutoConvertDWithUO()
    {
        if (_buffer.Count < 3)
            return;
        
        char first = _buffer[0];
        // Chỉ xử lý nếu bắt đầu bằng 'd' thường (chưa phải 'đ')
        if (char.ToLower(first) != 'd' || first == 'đ' || first == 'Đ')
            return;
        
        // Kiểm tra có pattern 'u' + 'o' hoặc 'ư' + 'o' hoặc 'ư' + 'ơ' sau 'd' không
        for (int i = 1; i < _buffer.Count - 1; i++)
        {
            char c1 = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[i]));
            char c2 = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[i + 1]));
            
            if ((c1 == 'u' || c1 == 'ư') && (c2 == 'o' || c2 == 'ơ'))
            {
                // Có pattern uo/ưo/ươ -> chuyển 'd' thành 'đ'
                bool isUpper = char.IsUpper(first);
                _buffer[0] = isUpper ? 'Đ' : 'đ';
                return;
            }
        }
    }
    
    /// <summary>
    /// Tự động chuyển 'ưo' + phụ âm thành 'ươ'
    /// Trong tiếng Việt, 'ưo' + phụ âm + dấu không tồn tại, phải là 'ươ'
    /// Ví dụ: đưoc + j -> được (tự động chuyển o -> ơ)
    /// </summary>
    private void AutoConvertUoToUoHorn()
    {
        for (int i = 0; i < _buffer.Count - 1; i++)
        {
            char current = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[i]));
            char next = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[i + 1]));
            
            // Tìm pattern: ư + o
            if (current == 'ư' && next == 'o')
            {
                // Kiểm tra có phụ âm sau không
                bool hasConsonantAfter = false;
                for (int j = i + 2; j < _buffer.Count; j++)
                {
                    if (!IsVowel(_buffer[j]))
                    {
                        hasConsonantAfter = true;
                        break;
                    }
                }
                
                if (hasConsonantAfter)
                {
                    // Chuyển 'o' thành 'ơ', giữ nguyên hoa/thường và dấu (nếu có)
                    char oChar = _buffer[i + 1];
                    bool isUpper = char.IsUpper(oChar);
                    var existingTone = VietnameseChar.GetToneIndex(oChar);
                    
                    char newO = isUpper ? 'Ơ' : 'ơ';
                    if (existingTone != VietnameseChar.ToneIndex.None)
                    {
                        newO = VietnameseChar.ApplyTone(newO, existingTone);
                    }
                    
                    _buffer[i + 1] = newO;
                }
            }
        }
    }
    
    private (int backspaceCount, string output)? TryProcessD(char key, bool isShiftPressed)
    {
        if (_buffer.Count == 0)
            return null;

        // Không chuyển dd -> đ nếu từ không thể là tiếng Việt
        if (!CouldBeVietnamese())
            return null;

        char lastChar = _buffer[^1];
        char lowerLast = char.ToLower(lastChar);

        // dd -> đ
        if (lowerLast == 'd')
        {
            // Triple-check uppercase: buffer char, key char, hoặc Shift state
            bool isUpper = char.IsUpper(lastChar) || char.IsUpper(key) || isShiftPressed;
            char newChar = isUpper ? VietnameseChar.UpperD : VietnameseChar.LowerD;
            
            _buffer[^1] = newChar;
            
            return (1, newChar.ToString());
        }
        
        return null;
    }
    /// <summary>
    /// Xử lý phím tắt '[' = ư, ']' = ơ với toggle.
    /// Lần 1: [ → ư, ] → ơ
    /// Lần 2: ư + [ → [, ơ + ] → ]
    /// </summary>
    private (int backspaceCount, string output)? TryProcessBracket(char key)
    {
        char targetVowel = key == '[' ? 'ư' : 'ơ';
        char bracket = key;

        // Toggle: nếu ký tự cuối trong buffer là ư (cho [) hoặc ơ (cho ]) → trả lại bracket
        if (_buffer.Count > 0)
        {
            char lastLower = char.ToLower(VietnameseChar.GetVowelWithoutTone(_buffer[^1]));
            if (lastLower == targetVowel)
            {
                // Xoá ư/ơ khỏi buffer, trả lại bracket gốc
                _buffer.RemoveAt(_buffer.Count - 1);
                // Xoá 1 ký tự (ư/ơ) trên màn hình, gửi bracket
                return (1, bracket.ToString());
            }
        }

        // Lần đầu: thêm ư/ơ vào buffer, output ư/ơ (chặn bracket gốc)
        _buffer.Add(targetVowel);
        return (0, targetVowel.ToString());
    }

    private (int backspaceCount, string output)? TryProcessW(char key)
    {
        // Buffer rỗng -> không chuyển w thành ư, để nguyên w (có thể là từ tiếng Anh: windows, web...)
        if (_buffer.Count == 0)
            return null;

        // Không chuyển đổi nếu từ không thể là tiếng Việt
        if (!CouldBeVietnamese())
            return null;

        // Kiểm tra ký tự cuối - nếu đã là ư/ơ/ă thì toggle ngược lại
        char lastChar = _buffer[^1];
        char lastLower = char.ToLower(VietnameseChar.GetVowelWithoutTone(lastChar));
        
        // Toggle: ư -> u, ơ -> o, ă -> a (gõ w lần 2 để hủy)
        // Thêm 'w' vào output để khôi phục đúng ký tự gốc
        // Ví dụ: "uw" -> ư, "uww" -> uw (undo)
        if (lastLower is 'ư' or 'ơ' or 'ă')
        {
            bool isUpper = char.IsUpper(lastChar);
            var existingTone = VietnameseChar.GetToneIndex(lastChar);
            
            char originalVowel = lastLower switch
            {
                'ư' => isUpper ? 'U' : 'u',
                'ơ' => isUpper ? 'O' : 'o',
                'ă' => isUpper ? 'A' : 'a',
                _ => lastChar
            };
            
            // Áp dụng lại dấu thanh nếu có
            if (existingTone != VietnameseChar.ToneIndex.None)
            {
                originalVowel = VietnameseChar.ApplyTone(originalVowel, existingTone);
            }
            
            // Đặc biệt: pattern ươ -> revert cả ư thành u
            // Ví dụ: "uow" -> ươ, "uoww" -> uow
            if (lastLower == 'ơ' && _buffer.Count >= 2)
            {
                char prevChar = _buffer[^2];
                char prevLower = char.ToLower(VietnameseChar.GetVowelWithoutTone(prevChar));
                if (prevLower == 'ư')
                {
                    // Revert ư -> u
                    bool prevUpper = char.IsUpper(prevChar);
                    var prevTone = VietnameseChar.GetToneIndex(prevChar);
                    char prevOriginal = prevUpper ? 'U' : 'u';
                    if (prevTone != VietnameseChar.ToneIndex.None)
                        prevOriginal = VietnameseChar.ApplyTone(prevOriginal, prevTone);
                    
                    _buffer[^2] = prevOriginal;
                    _buffer[^1] = originalVowel;
                    _buffer.Add(key);
                    
                    // Xoá cả ươ (2 ký tự), ghi lại uow
                    int startPos = _buffer.Count - 3;
                    int bs = 2;
                    string output = new string(_buffer.Skip(startPos).ToArray());
                    return (bs, output);
                }
            }
            
            _buffer[^1] = originalVowel;
            _buffer.Add(key); // Thêm 'w' vào buffer

            return (1, originalVowel.ToString() + key);
        }
        
        // Kiểm tra pattern đặc biệt: 'uo' hoặc 'ưo' -> chuyển thành 'ươ'
        // Chỉ tìm trong âm tiết hiện tại
        // Ví dụ: "duo" + w -> "dươ", "duoc" + w -> "dươc", "dưo" + w -> "dươ"
        {
            int uPos = -1, oPos = -1;
            bool enteredVowelCluster = false;
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                char c = _buffer[i];
                char lowerC = char.ToLower(VietnameseChar.GetVowelWithoutTone(c));

                if (IsVowel(c))
                {
                    enteredVowelCluster = true;
                    if (oPos < 0 && lowerC == 'o')
                    {
                        oPos = i;
                    }
                    else if (oPos >= 0 && (lowerC == 'u' || lowerC == 'ư'))
                    {
                        uPos = i;
                        break;
                    }
                }
                else
                {
                    // Đã vào vùng nguyên âm rồi gặp phụ âm → ra khỏi âm tiết
                    if (enteredVowelCluster)
                        break;
                    // Chưa vào vùng nguyên âm → phụ âm cuối
                    char lch = char.ToLower(c);
                    if (!(lch is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h'))
                        break;
                }
            }

            if (uPos >= 0 && oPos > uPos)
            {
                char uChar = _buffer[uPos];
                char oChar = _buffer[oPos];
                char uLower = char.ToLower(VietnameseChar.GetVowelWithoutTone(uChar));

                // Chuyển 'u' thành 'ư' (nếu chưa)
                if (uLower == 'u')
                {
                    bool isUpper = char.IsUpper(uChar);
                    var tone = VietnameseChar.GetToneIndex(uChar);
                    char newU = isUpper ? 'Ư' : 'ư';
                    if (tone != VietnameseChar.ToneIndex.None)
                        newU = VietnameseChar.ApplyTone(newU, tone);
                    _buffer[uPos] = newU;
                }

                // Chuyển 'o' thành 'ơ'
                {
                    bool isUpper = char.IsUpper(oChar);
                    var tone = VietnameseChar.GetToneIndex(oChar);
                    char newO = isUpper ? 'Ơ' : 'ơ';
                    if (tone != VietnameseChar.ToneIndex.None)
                        newO = VietnameseChar.ApplyTone(newO, tone);
                    _buffer[oPos] = newO;
                }

                int backspaceCount = _buffer.Count - uPos;
                string output = new string(_buffer.Skip(uPos).ToArray());
                return (backspaceCount, output);
            }
        }
        
        // Tìm nguyên âm gần nhất TRONG ÂM TIẾT HIỆN TẠI để chuyển đổi
        // Ví dụ: "dudw" -> "dưd" (tìm 'u' trong cùng âm tiết)
        {
            bool enteredVowelCluster = false;
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                char c = _buffer[i];
                char lowerC = char.ToLower(c);
                
                if (IsVowel(c))
                {
                    enteredVowelCluster = true;
                    // Chỉ xử lý a, o, u (ă, ô đã được transform rồi)
                    if (lowerC is 'a' or 'o' or 'u')
                    {
                        char newVowel = VietnameseChar.TransformVowel(c, 'w');
                        
                        if (newVowel != c)
                        {
                            _buffer[i] = newVowel;
                            
                            int backspaceCount = _buffer.Count - i;
                            string output = new string(_buffer.Skip(i).ToArray());
                            
                            return (backspaceCount, output);
                        }
                    }
                }
                else
                {
                    // Đã vào vùng nguyên âm rồi gặp phụ âm → ra khỏi âm tiết
                    if (enteredVowelCluster)
                        break;
                    // Chưa vào vùng nguyên âm → phụ âm cuối
                    char lowerCh = char.ToLower(c);
                    if (!(lowerCh is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h'))
                        break;
                }
            }
        }
        
        // Không tìm thấy nguyên âm để chuyển đổi
        // Nếu buffer chỉ có phụ âm (chưa có nguyên âm nào) → w = ư (viết tắt)
        // Ví dụ: nhw → như, thw → thư, chwx → chữ, lw → lư
        {
            bool hasVowel = false;
            for (int i = 0; i < _buffer.Count; i++)
            {
                if (IsVowel(_buffer[i]))
                {
                    hasVowel = true;
                    break;
                }
            }

            if (!hasVowel)
            {
                bool isUpper = char.IsUpper(key);
                char uHorn = isUpper ? 'Ư' : 'ư';
                _buffer.Add(uHorn);
                return (0, uHorn.ToString());
            }
        }

        // Có nguyên âm nhưng không chuyển được → không xử lý, để ra chữ 'w' thường
        return null;
    }
    
    private (int backspaceCount, string output)? TryProcessDoubleVowel(char key)
    {
        if (_buffer.Count == 0)
            return null;

        // Không chuyển aa->â, ee->ê, oo->ô nếu từ không thể là tiếng Việt
        if (!CouldBeVietnamese())
            return null;

        char lastChar = _buffer[^1];
        char lowerLast = char.ToLower(lastChar);
        char lowerKey = char.ToLower(key);

        // aa -> â, ee -> ê, oo -> ô (liền nhau)
        if (lowerLast == lowerKey)
        {
            char newVowel = lowerKey switch
            {
                'a' => 'â',
                'e' => 'ê',
                'o' => 'ô',
                _ => lastChar
            };

            if (char.IsUpper(lastChar))
            {
                newVowel = char.ToUpper(newVowel);
            }

            // Giữ lại dấu thanh nếu có
            var currentTone = VietnameseChar.GetToneIndex(lastChar);
            if (currentTone != VietnameseChar.ToneIndex.None)
            {
                newVowel = VietnameseChar.ApplyTone(newVowel, currentTone);
            }

            _buffer[^1] = newVowel;

            return (1, newVowel.ToString());
        }

        // Toggle: nếu ký tự cuối đã là mũ (â/ê/ô) và gõ thêm nguyên âm gốc tương ứng
        // → revert mũ thành nguyên âm thường + thêm ký tự gốc (undo transform)
        // Ví dụ: "Gô" + o → "Goo" (undo ô → o, thêm o cho phím bị nuốt khi tạo mũ)
        char lowerLastBase = char.ToLower(VietnameseChar.GetVowelWithoutTone(lastChar));
        char expectedBase = lowerLastBase switch
        {
            'â' => 'a',
            'ê' => 'e',
            'ô' => 'o',
            _ => '\0'
        };
        if (expectedBase != '\0' && lowerKey == expectedBase)
        {
            // Revert: ô → o, â → a, ê → e
            bool isUpper = char.IsUpper(lastChar);
            char reverted = isUpper ? char.ToUpper(expectedBase) : expectedBase;

            // Giữ dấu thanh nếu có
            var existingTone = VietnameseChar.GetToneIndex(lastChar);
            if (existingTone != VietnameseChar.ToneIndex.None)
            {
                reverted = VietnameseChar.ApplyTone(reverted, existingTone);
            }

            _buffer[^1] = reverted;
            // Thêm 1 ký tự gốc (phím bị nuốt khi tạo mũ)
            _buffer.Add(key);

            return (1, reverted.ToString() + key);
        }

        // Tìm ngược buffer TRONG CÙNG ÂM TIẾT: nếu có cùng nguyên âm gốc (a/e/o) ở trước,
        // chuyển nguyên âm đó thành mũ và bỏ ký tự mới
        // Ví dụ: "toi" + o -> "tôi", "muon" + o -> "muôn"
        // Giới hạn: chỉ tìm trong âm tiết hiện tại, không vượt ranh giới âm tiết
        {
            bool enteredVowelCluster = false;
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                char c = _buffer[i];
                char lowerC = char.ToLower(VietnameseChar.GetVowelWithoutTone(c));

                if (IsVowel(c))
                {
                    enteredVowelCluster = true;
                    
                    if (lowerC == lowerKey)
                    {
                        char newVowel = lowerKey switch
                        {
                            'a' => 'â',
                            'e' => 'ê',
                            'o' => 'ô',
                            _ => c
                        };

                        if (newVowel == c) break; // Không chuyển được

                        if (char.IsUpper(c))
                        {
                            newVowel = char.ToUpper(newVowel);
                        }

                        // Giữ lại dấu thanh nếu có
                        var existingTone = VietnameseChar.GetToneIndex(c);
                        if (existingTone != VietnameseChar.ToneIndex.None)
                        {
                            newVowel = VietnameseChar.ApplyTone(newVowel, existingTone);
                        }

                        _buffer[i] = newVowel;

                        // Xóa từ vị trí nguyên âm đến cuối, ghi lại
                        int backspaceCount = _buffer.Count - i;
                        string output = new string(_buffer.Skip(i).ToArray());

                        return (backspaceCount, output);
                    }
                }
                else
                {
                    // Đã vào vùng nguyên âm rồi gặp phụ âm → ra khỏi âm tiết hiện tại → dừng
                    // Ví dụ: "donamthu" + 'o': sau 'u' gặp 'h' → dừng, không match 'o' ở "do"
                    if (enteredVowelCluster)
                        break;
                    
                    // Chưa vào vùng nguyên âm → đang ở phụ âm cuối (c,m,n,p,t,ng,nh,ch)
                    char lowerCh = char.ToLower(c);
                    if (!(lowerCh is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h'))
                        break;
                }
            }
        }
        
        return null;
    }
    
    public void Reset()
    {
        _buffer.Clear();
    }

    /// <summary>
    /// Undo tất cả dấu/mũ/móc trong buffer, gửi output sửa lại,
    /// rồi reset buffer và bắt đầu từ mới với ký tự hiện tại.
    /// Ví dụ: buffer "pú" + key 'h' → xoá "pú" (2 bs), ghi "push", reset buffer thành ['h']
    /// Ví dụ: buffer "tẻ" + "mina" + key 'l' → xoá "tẻmina" (6 bs), ghi "terminal"
    /// </summary>
    private void UndoAndReset(ProcessKeyResult result, char newKey)
    {
        // Tìm phím dấu đã dùng (s/f/r/x/j) và VỊ TRÍ nguyên âm đã mang dấu
        char? toneChar = null;
        int toneVowelPos = -1;
        for (int i = 0; i < _buffer.Count; i++)
        {
            var tone = VietnameseChar.GetToneIndex(_buffer[i]);
            if (tone != VietnameseChar.ToneIndex.None)
            {
                toneChar = tone switch
                {
                    VietnameseChar.ToneIndex.Acute => 's',
                    VietnameseChar.ToneIndex.Grave => 'f',
                    VietnameseChar.ToneIndex.Hook => 'r',
                    VietnameseChar.ToneIndex.Tilde => 'x',
                    VietnameseChar.ToneIndex.Dot => 'j',
                    _ => null
                };
                toneVowelPos = i;
                break;
            }
        }

        // Số ký tự trên màn hình cần xoá = số ký tự trong buffer
        int bs = _buffer.Count;

        // Xoá tất cả dấu/mũ/móc trong buffer
        for (int i = 0; i < _buffer.Count; i++)
        {
            char c = _buffer[i];

            // Xoá dấu thanh
            var t = VietnameseChar.GetToneIndex(c);
            if (t != VietnameseChar.ToneIndex.None)
                c = VietnameseChar.ApplyTone(c, VietnameseChar.ToneIndex.None);

            // Xoá mũ/móc
            bool isUpper = char.IsUpper(c);
            char lower = char.ToLower(c);
            char? replaced = lower switch
            {
                'ô' => 'o', 'ê' => 'e', 'â' => 'a',
                'ư' => 'u', 'ơ' => 'o', 'ă' => 'a', 'đ' => 'd',
                _ => null
            };
            if (replaced.HasValue)
                c = isUpper ? char.ToUpper(replaced.Value) : replaced.Value;

            _buffer[i] = c;
        }

        // Chèn phím dấu vào ĐÚNG vị trí (ngay sau nguyên âm đã mang dấu)
        // Ví dụ: buffer ['t','e','m','i','n','a'] + toneChar='r' tại pos=1
        //       → insert 'r' tại pos 2 → ['t','e','r','m','i','n','a']
        //       → output = "termina" + newKey
        if (toneChar.HasValue && toneVowelPos >= 0)
        {
            _buffer.Insert(toneVowelPos + 1, toneChar.Value);
        }

        string undone = new string(_buffer.ToArray());
        string output = undone + newKey;

        result.Handled = true;
        result.BackspaceCount = bs;
        result.OutputText = output;

        // Reset buffer, bắt đầu từ mới với ký tự hiện tại
        _buffer.Clear();
        _buffer.Add(newKey);
        result.CurrentBuffer = GetBuffer();
    }

    public bool ProcessBackspace()
    {
        if (_buffer.Count > 0)
        {
            _buffer.RemoveAt(_buffer.Count - 1);
            return false; // Vẫn cần gửi backspace xuống
        }
        return false;
    }

    public string GetBuffer()
    {
        return new string(_buffer.ToArray());
    }

    /// <summary>
    /// Kiểm tra buffer có chứa nguyên âm đã có dấu thanh không
    /// </summary>
    private bool HasToneInBuffer()
    {
        foreach (char c in _buffer)
        {
            if (IsVowel(c) && VietnameseChar.GetToneIndex(c) != VietnameseChar.ToneIndex.None)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Kiểm tra ký tự có phải phụ âm cuối hợp lệ trong tiếng Việt không
    /// Phụ âm cuối: c, ch, m, n, ng, nh, p, t
    /// </summary>
    private static bool IsValidEndingConsonant(char lowerKey)
    {
        return lowerKey is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h';
    }

    /// <summary>
    /// Kiểm tra buffer hiện tại có bắt đầu bằng phụ âm đầu không hợp lệ trong tiếng Việt.
    /// Tiếng Việt KHÔNG có phụ âm đầu: w, f, j, z
    /// Nếu từ bắt đầu bằng các ký tự này → chắc chắn là từ tiếng Anh → bỏ qua xử lý dấu.
    /// </summary>
    private bool StartsWithNonVietnameseConsonant()
    {
        if (_buffer.Count == 0) return false;
        char firstLower = char.ToLower(_buffer[0]);
        return firstLower is 'w' or 'f' or 'j' or 'z';
    }

    /// <summary>
    /// Kiểm tra buffer có chứa cluster phụ âm không hợp lệ trong tiếng Việt.
    /// Tiếng Việt chỉ có các phụ âm đầu ghép: ch, gh, gi, kh, ng, ngh, nh, ph, qu, th, tr
    /// Nếu có cluster phụ âm liên tiếp không nằm trong danh sách → không phải tiếng Việt.
    /// Ví dụ: "nd" (windows), "ftw", "str" → không hợp lệ
    /// </summary>
    private bool HasInvalidConsonantCluster()
    {
        if (_buffer.Count < 2) return false;

        // Tìm các phụ âm đầu liên tiếp
        var consonants = new List<char>();
        for (int i = 0; i < _buffer.Count; i++)
        {
            char c = char.ToLower(_buffer[i]);
            if (IsVowel(_buffer[i])) break;
            // 'đ' được coi là phụ âm đơn hợp lệ
            if (c == 'đ') { consonants.Add('d'); break; }
            consonants.Add(c);
        }

        if (consonants.Count <= 1) return false;

        string cluster = new string(consonants.ToArray());

        // Danh sách phụ âm đầu ghép hợp lệ trong tiếng Việt
        return cluster is not ("ch" or "gh" or "gi" or "kh" or "ng" or "ngh" or "nh"
                               or "ph" or "qu" or "th" or "tr");
    }

    /// <summary>
    /// Kiểm tra xem buffer hiện tại có khả năng là từ tiếng Việt hợp lệ không.
    /// Trả về false nếu rõ ràng KHÔNG phải tiếng Việt → bỏ qua xử lý dấu/mũ/móc.
    /// </summary>
    private bool CouldBeVietnamese()
    {
        if (_buffer.Count == 0) return true;

        // Bắt đầu bằng phụ âm không có trong tiếng Việt
        if (StartsWithNonVietnameseConsonant()) return false;

        // Cluster phụ âm đầu không hợp lệ
        if (HasInvalidConsonantCluster()) return false;

        // 'w' xuất hiện trong buffer → không phải tiếng Việt
        // Trong tiếng Việt, w chỉ là modifier (chuyển u->ư, o->ơ, a->ă),
        // không bao giờ là phần của từ. Nếu 'w' nằm trong buffer nghĩa là
        // nó đã đi qua như ký tự thường (từ tiếng Anh) hoặc sau toggle undo.
        if (HasRawW()) return false;

        return true;
    }

    /// <summary>
    /// Kiểm tra buffer có chứa ký tự 'w' thường (không phải modifier đã chuyển thành ư/ơ/ă).
    /// Nếu có 'w' trong buffer → từ này là tiếng Anh (ví dụ: windows, download sau toggle).
    /// </summary>
    private bool HasRawW()
    {
        foreach (char c in _buffer)
        {
            if (char.ToLower(c) == 'w')
                return true;
        }
        return false;
    }
}
