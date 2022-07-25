## Cách sử dụng Fiddler với thiết bị Android
Đã có khá nhiều hướng dẫn trên mạng cho việc này rồi. [Đây](https://youtu.be/mXN0dUDz0qk) là một video khá dễ hiểu và trực quan.

Hiện tại mình chưa có thời gian hoàn thiện hướng dẫn này, mình sẽ cập nhật trong tương lai, nhưng hãy tóm tắt nhé:
- Đầu tiên mở `Tools->Options` trong Fiddler, ở tab `Connections`, nhập `8888` vào ô `Fiddler listens on port`. Sau đó bật `Allow remote computers to connect`. Chuyển sang tab `HTTPS`, bật `Capture HTTPS CONNECTs`, `Decrypt HTTPS traffic` và `Ignore server certificate errors (unsafe)`.
- Cũng trong Fiddler, bạn nhìn phía trên bên phải sẽ có một icon máy tính và chữ Online, di chuột vào đó sẽ hiện ra thông tin IP. Có thể sẽ có nhiều IP hiện ra, bạn hãy ghi lại IP nào có dạng `192.168.x.x` xuất hiện đầu tiên (cái nằm ở trên cùng).
- Bây giờ hãy mở thiết bị Android lên, **lưu ý là thiết bị phải được ROOT mới có thể cài**, cài app [Root Certificate Manager(ROOT)](https://play.google.com/store/apps/details?id=net.jolivier.cert.Importer&hl=en&gl=US).
- Sau đó vào `Settings (Cài đặt)` của thiết bị, đi tới phần `Wifi (hay Kết nối)`, nhấn giữ vào tên wifi đang kết nối, sẽ có option hiện lên, click vào `Modify network (Chỉnh sửa kết nối)`. Ở phần Proxy, hãy chọn `Manual (Thủ công)`, sau đó nhập vào IP mà bạn lấy được ở bước ở trên, và Port là `8888`.
- Bây giờ mọi kết nối trên thiết bị đều được proxy qua fiddler, hãy thử mở trình duyệt trên thiết bị và truy cập `ipv4.fiddler:8888`, nếu ra một trang web thì bạn đã thành công. Còn nếu không được, hãy kiếm tra lại IP bạn nhập vào Proxy đã đúng chưa. Như trường hợp của mình proxy không hoạt động trên giả lập LDPlayer, chuyển sang Nox thì lại được, nếu gặp lỗi này bạn hãy thử dùng thiết bị khác nhé.
- Nếu đã vào được trang `ipv4.fiddler:8888`, bạn hãy nhấn vào link `FiddlerRoot certificate` ở cuối trang để download certificate. Sau khi download xong thì nhấn vào file đó để tiến hành cài certificate, ở ô `Certificate name` bạn có thể nhập tên bất kỳ.
- Bây giờ hãy mở app `Root Certificate Manager(ROOT)`, nhấn vào icon Folder ở trên bên phải (kế icon kính lúp) và chọn file certificate fiddler bạn vừa download. Nhấn Import.

Vậy là bạn đã kết nối thành công thiết bị với Fiddler.