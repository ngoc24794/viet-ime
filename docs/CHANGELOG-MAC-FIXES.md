# VietIME — Các thay đổi TelexEngine cần apply cho bản Windows

> Document này tổng hợp các bug fix và tính năng mới trong `TelexEngine.cs` (shared Core)
> đã được phát triển/fix trên bản macOS. Cần apply lại cho bản Windows.
>
> File: `src/VietIME.Core/Engines/TelexEngine.cs`
> Commit: `5df81a0` — có thể pull trực tiếp từ `upstream/master`

---

## 1. Fix Toggle Double Character (TryApplyTone)

**Vấn đề**: Khi gõ `tẻ` rồi nhấn `r` lần nữa (toggle xoá dấu), kết quả ra `terr` thay vì `ter`.

**Nguyên nhân**: Toggle chỉ gửi phần thay đổi (bs=1 + "er"), backspace có thể không xoá đúng ký tự unicode trên một số terminal.

**Fix**: Thay đổi toggle để gửi **toàn bộ buffer** thay vì chỉ phần thay đổi.

```csharp
// CŨ (dòng ~314):
int toggleBackspace = _buffer.Count - vowelPos - 1;
string toggleOutput = new string(_buffer.Skip(vowelPos).ToArray());

// MỚI:
int toggleBackspace = _buffer.Count - 1; // xoá hết ký tự cũ trên màn hình
string toggleOutput = new string(_buffer.ToArray()); // ghi lại toàn bộ buffer
```

**Ví dụ**: `tẻ` + `r` → bs=2 (xoá "tẻ") + output="ter" (ghi lại toàn bộ)

---

## 2. Fix Syllable Boundary Detection (3 chỗ)

**Vấn đề**: Gõ `donamthuongtin` → chữ `o` thứ 2 (trong "thuong") match nhầm `o` đầu tiên (trong "do") → biến thành `dônamthungtin`.

**Nguyên nhân**: Backward search trong `TryProcessDoubleVowel` và `TryProcessW` cho phép vượt qua phụ âm cuối hợp lệ (c,m,n,p,t,g,h) vô hạn, không nhận diện ranh giới âm tiết.

**Fix**: Thêm biến `enteredVowelCluster` — khi đã gặp nguyên âm (vào vùng vowel cluster) rồi gặp phụ âm → đã ra khỏi âm tiết → **dừng tìm**.

### 2a. TryProcessDoubleVowel — backward search (dòng ~843)

```csharp
// CŨ:
for (int i = _buffer.Count - 1; i >= 0; i--)
{
    // ... tìm match ...
    if (!IsVowel(c))
    {
        char lowerCh = char.ToLower(c);
        if (!(lowerCh is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h'))
            break;
    }
}

// MỚI:
bool enteredVowelCluster = false;
for (int i = _buffer.Count - 1; i >= 0; i--)
{
    if (IsVowel(c))
    {
        enteredVowelCluster = true;
        // ... tìm match ...
    }
    else
    {
        if (enteredVowelCluster) break; // ĐÃ RA KHỎI ÂM TIẾT → DỪNG
        char lowerCh = char.ToLower(c);
        if (!(lowerCh is 'c' or 'm' or 'n' or 'p' or 't' or 'g' or 'h'))
            break;
    }
}
```

### 2b. TryProcessW — pattern uo/ưo → ươ (dòng ~658)

Cùng pattern: thêm `enteredVowelCluster` vào vòng lặp tìm `u` + `o`.

### 2c. TryProcessW — tìm nguyên âm gần nhất (dòng ~718)

Cùng pattern: thêm `enteredVowelCluster` thay vì tìm không giới hạn.

---

## 3. Thêm w → ư shorthand (TryProcessW)

**Tính năng**: Khi buffer chỉ có phụ âm (chưa có nguyên âm), gõ `w` → `ư`.

**Ví dụ**: `nhw` → `như`, `thw` → `thư`, `chwx` → `chữ`, `lw` → `lư`

**Code** (thêm ở cuối TryProcessW, trước `return null`):

```csharp
// Nếu buffer chỉ có phụ âm → w = ư (viết tắt)
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
```

---

## 4. Fix UndoAndReset — vị trí chèn tone char

**Vấn đề**: Khi undo dấu (ví dụ `tẻmina` + `l` → `terminal`), phím dấu `r` bị chèn ở cuối thay vì sau nguyên âm.

**Fix**: Dùng `toneVowelPos` để chèn đúng vị trí.

```csharp
// CŨ:
if (toneChar.HasValue)
{
    _buffer.Add(toneChar.Value); // SAI: thêm ở cuối
}

// MỚI:
if (toneChar.HasValue && toneVowelPos >= 0)
{
    _buffer.Insert(toneVowelPos + 1, toneChar.Value); // ĐÚNG: chèn sau nguyên âm
}
```

**Ví dụ**: buffer `['t','e','m','i','n','a']` + toneChar='r' tại pos=1
→ Insert 'r' tại pos 2 → `['t','e','r','m','i','n','a']` → output "terminal"

---

## Cách apply nhanh nhất

Bản Windows dùng chung `VietIME.Core`, nên chỉ cần:

```powershell
cd <thư-mục-vietIME-windows>
git pull upstream master
```

File `src/VietIME.Core/Engines/TelexEngine.cs` sẽ được cập nhật tự động.
Không cần sửa gì ở `VietIME.Hook` hay `VietIME.App` (Windows-specific).
