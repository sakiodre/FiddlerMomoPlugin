# FiddlerMomoPlugin
Một plugin cơ bản dùng cho việc giải mã traffic của app **Momo**.

## Cài đặt
- Tải về plugin này [tại đây](https://github.com/thedemons/FiddlerMomoPlugin/releases/tag/Release) và giải nén
- Xác định vị trí thư mục chứa **Fiddler**, đối với Fiddler 5 thì mặc định nằm ở `%localappdata%\Programs\Fiddler` hoặc `C:\Program Files\Fiddler`.
- Tiến hành copy 3 file (`FiddlerMomoPlugin.dll, Newtonsoft.Json.dll, và BouncyCastle.Crypto.dll`) vào thư mục `Scripts` và `Inspectors` nằm trong thư mục chứa **Fiddler**, xin hãy lưu ý là phải copy vào cả 2 thư mục.

## Sử dụng
- Để dùng được Fiddler với thiết bị Android, đầu tiên bạn phải làm theo [hướng dẫn này](/docs/fiddler-on-android/).
- Sau khi cài đặt thành công, nếu bạn đã đăng nhập vào **Momo**, hãy đăng xuất ra trước.
- Sau đó tiến hành đăng nhập vào **Momo**, bây giờ plugin đã load thành công và sẵn sàng để decrypt.
- Việc này là để thay *Momo public key* trong kết quả trả về của request login, hãy xem [Cách thức hoạt động của **FiddlerMomoPlugin**](/docs/technical-details/) để biết thêm chi tiết.
- Bây giờ plugin đã có thể bắt đầu giải mã, khi bật tab Inspector, bạn sẽ thấy như sau: <br><img src="/img/decrypted_request.jpg" width=300>
- Ở phần response, do giới hạn của Fiddler nên không thể hiện dữ liệu đã được giải mã trực tiếp như phần request, thay vào đó có 2 tab được tạo ra là `TextView Decrypted` và `JSON Decrypted` dùng để xem dữ liệu đã được giải mã.

## Nâng cao
- Bạn có thể dùng hai lệnh này trong `QuickExec` của **Fiddler**  để debug.
    - `momo_debug`: mở console của plugin để xem output.
    - `momo_auth`: để lấy Authorization Token của tài khoản **Momo** hiện tại.
- Hãy nhập vào ô ở góc dưới bên trái của **Fiddler** (phím tắt ALT+Q để focus).
- Ảnh xem trước: <br><img src="/img/console.jpg" width=500>

## Giấy phép & Điều khoản
- Plugin này được xuất bản dưới [Giấy Phép Công Cộng GNU GPLv3](https://vi.wikipedia.org/wiki/Gi%E1%BA%A5y_ph%C3%A9p_C%C3%B4ng_c%E1%BB%99ng_GNU)
- Tuy nhiên, xin vui lòng không sử dụng plugin này vào các mục đích sau đây:
    1) Botting, spamming, tấn công DDOS, hay chủ ý phá hoại máy chủ **Momo** bằng những phương thức tương tự.
    2) Tạo tài khoản với danh tính ảo, bao gồm có hoặc không nhằm mục đích xấu, như ẩn danh, cờ bạc, giao dịch trái pháp luật.
    3) Khai thác lỗ hỏng của **Momo** khi chưa được sự cho phép của **Momo** và các bên có liên quan.
    4) Chỉnh sửa lại plugin nhằm lây lan mã độc, bao gồm việc không công khai mã nguồn.
    5) Các mục đích xấu khác nhằm trục lợi, chiếm đoạt, tấn công, hay phá hoại bất cứ tổ chức hoặc cá nhân nào.

## Từ chối trách nhiệm *(Disclaimer)*
- Tác giả của plugin này không chịu trách nhiệm cho mọi hoạt động bất hợp pháp và vi phạm điều khoản như đã nêu ở trên của mọi người dùng.
- Mục đích tác giả tạo ra plugin này là cho việc nghiên cứu & tìm hiểu. Tác giả khuyến khích người dùng hãy khám phá **Momo API** một cách chính đáng với tinh thần học hỏi.

## **FiddlerMomoPlugin** có giúp ích cho bạn?
Hãy cho repository này một ⭐ nếu nó đã giúp bạn nhé! Điều này sẽ làm cho tác giả có động lực duy trì cập nhật và sửa lỗi!
