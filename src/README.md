# Những gì cần làm trước khi build
- Sửa lại reference tới `Fiddler.exe` và `Standard.dll` theo [hướng dẫn này](https://docs.telerik.com/fiddler/extend-fiddler/extendwithdotnet#sample-extension-step-by-step). File `Fiddler.exe` nằm trong thư mục chính, còn file `Standard.dll` nằm trong thư mục `\Inspectors`.
- Kiểm tra lại build events: Trong Solution Explorer của Visual Studio, right-click vào project `FiddlerMomoPlugin` chọn `Properties`, mở tab `Build Events`, bạn sẽ thấy những event này:
    ```bash
    copy "$(TargetPath)" "%localappdata%\Programs\Fiddler\Scripts\$(TargetFilename)"
    copy "$(TargetPath)" "%localappdata%\Programs\Fiddler\Inspectors\$(TargetFilename)"
    
    copy "BouncyCastle.Crypto.dll" "%localappdata%\Programs\Fiddler\Scripts\BouncyCastle.Crypto.dll"
    copy "BouncyCastle.Crypto.dll" "%localappdata%\Programs\Fiddler\Inspectors\BouncyCastle.Crypto.dll"
    
    copy "Newtonsoft.Json.dll" "%localappdata%\Programs\Fiddler\Scripts\Newtonsoft.Json.dll"
    copy "Newtonsoft.Json.dll" "%localappdata%\Programs\Fiddler\Inspectors\Newtonsoft.Json.dll"
    ```
- Nếu `%localappdata%\Programs\Fiddler` không phải là folder Fiddler trên máy của bạn, hãy sửa lại.
- Kế tiếp mở tab `Debug`, ở input `Start external program`, hãy chọn đường dẫn tới Fiddler.exe