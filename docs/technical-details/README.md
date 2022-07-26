
# Cách thức hoạt động của **FiddlerMomoPlugin**
Đây là chi tiết kỹ thuật về cách mà plugin này giải mã traffic.

## Momo mã hóa như thế nào?
- Đầu tiên hãy xem qua một request đã được mã hóa <br> <p align="center"><img src="/../../raw/main/img/encrypted_request.jpg" width=600></p>
- Ở phần request header ta thấy có một trường `requestkey` dưới dạng base64, đây là `aes_key` đã được mã hóa bằng **RSA** sử dụng `MOMO_PUBLIC_KEY`.
- `MOMO_PUBLIC_KEY` là RSA Public Key được **Momo** trả về khi người dùng đăng nhập.
- Phần request và response body được mã hóa bằng `AES-256-CBC` sử dụng `aes_key`, với iv là 16 null bytes
- Dưới đây là pseudo code mô tả lại quá trình encrypt:
    ```python
    aes_key = urandom(32) # aes_key là một dãy random 256 bits
    requestkey = base64(rsa_encrypt(aes_key, MOMO_PUBLIC_KEY))
    encrypted_request = aes_256_cbc_encrypt(plain_request, aes_key) # plain_request là data chưa mã hóa
    request_body = base64(encrypted_request) # request body được post lên server
    ```
- Quá trình decrypt trên server:
    ```python
    aes_key = rsa_decrypt(from_base64(requestkey), MOMO_PRIVATE_KEY) # chúng ta không thể biết được private key của momo
    encrypted_request = from_base64(request_body)
    decrypted_request = aes_256_cbc_decrypt(encrypted_request, aes_key)
    ...
    encrypted_response = aes_256_cbc_encrypt(plain_response, aes_key)
    ```
- Sau khi server response, client sẽ decrypt như thế này:
    ```python
     # aes_key được tạo ở trên, lúc gửi request đi
    decrypted_data = aes_256_cbc_decrypt(from_base64(response_body), aes_key)
    ```

## FiddlerMomoPlugin giải mã như thế nào nếu không có RSA Private Key của Momo?
- Nếu như bạn chưa biết về [RSA](https://vi.wikipedia.org/wiki/RSA_(m%C3%A3_h%C3%B3a)), thì nó là một loại [Mã hóa bất đối xứng](https://en.wikipedia.org/wiki/Public-key_cryptography) - nền tảng cho hầu hết mọi kết nối internet hiện tại (https).
- Về cơ bản, sẽ có 1 cặp key được tạo ra ngẫu nhiên, một Public và một Private. Khi dữ liệu được mã hóa bằng Public Key thì chỉ có thể được giải mã bằng Private Key và ngược lại. 
- Dưới đây là pseudo code mô tả lại cách thức mã hóa:
    ```python
    public_key, private_key = rsa_generate_key_pair() # tạo ra 1 cặp key ngẫu nhiên
    plain_text = "Hello World!"

    encrypted_text = rsa_encrypt(plain_text, public_key) # mã hóa bằng public key
    decrypted_text = rsa_decrypt(encrypted_text, private_key) # và giải mã bằng private key
    assert(decrypted_text == plain_text) # nếu không có lỗi gì xảy ra thì chúng ta đã giải mã thành công
    
    # và ngược lại!
    encrypted_text = rsa_encrypt(plain_text, private_key) # mã hóa bằng private key
    decrypted_text = rsa_decrypt(encrypted_text, public_key) # và giải mã bằng public key
    assert(decrypted_text == plain_text)
    ```
- Nếu tới đây bạn thắc mắc, nếu như `requestkey` đã được mã hóa RSA bằng `MOMO_PUBLIC_KEY`, vậy thì làm sao plugin này có thể giải mã nó? Thì đúng rồi đấy, điều đó là bất khả thi.
- Nhưng điều plugin này có thể làm là tráo `MOMO_PUBLIC_KEY` bằng một Public Key được chúng ta tạo ra, và chúng ta có Private Key tương ứng để có thể giải mã nó!
- Khi chúng ta đã có thể giải mã `requestkey` để lấy `aes_key`, thì việc còn lại chỉ là giải mã AES-256-CBC cho request và response body.

## Cách thức chi tiết việc giải mã
- Đầu tiên plugin này can thiệp vào request đăng nhập tới `htttps://owa.momo.vn/public/login`. API này sẽ trả về `MOMO_PUBLIC_KEY`, từ đó ta có thể thay thế nó bằng Public Key được chúng ta kiểm soát:
    ```csharp
    string body = Encoding.UTF8.GetString(oSession.ResponseBody);
    dynamic response = JsonConvert.DeserializeObject<dynamic>(body);

    // lưu lại MOMO_PUBLIC_KEY
    string publicKeyPEM = response.extra.REQUEST_ENCRYPT_KEY;
    StringReader stream = new StringReader(publicKeyPEM);
    momoPublicKey = (AsymmetricKeyParameter)new PemReader(stream).ReadObject();

    // thay thế Public Key của chúng ta vào response body
    response.extra.REQUEST_ENCRYPT_KEY = injectedPublicKeyPEM;
    string newResponse = JsonConvert.SerializeObject(response);
    oSession.ResponseBody = Encoding.UTF8.GetBytes(newResponse);
    ```
- Khi đã thay thế được `MOMO_PUBLIC_KEY`, chúng ta có thể tiến hành giải mã với Private Key tương ứng. Nhưng không chỉ thế, chúng ta cần phải mã hóa lại `requestkey` bằng `MOMO_PUBLIC_KEY` để server có thể giải mã được. Phải làm những thứ này trước khi request được gửi lên server:
    ```csharp
    // giải mã requestKey với Private Key của chúng ta
    string aes_key = RSADecryptWithInjectedPrivateKey(oSession.oRequest["requestkey"]);

    // mã hóa lại bằng MOMO_PUBLIC_KEY
    oSession.oRequest["requestkey"] = RSAEncryptWithMomoPublicKey(aes_key);
    
    // lưu lại aes_key vào header của request vì chúng ta
    // không có cách khác để lưu dữ liệu vào một session của Fiddler
    // đây là một giới hạn, việc này có thể khiến Momo phát hiện ra
    // chúng ta đang giải mã, nhưng nếu xảy ra việc đó, chúng ta sẽ
    // tìm cách khác, còn bây giờ thì nó vẫn đang hoạt động ổn
    oSession.oRequest["requestkey_decrypted"] = aes_key;

    // giải mã request body bằng aes_key
    string decrypted_data = AESDecrypt(Encoding.UTF8.GetString(oSession.RequestBody), aes_key);
    ```
- Sau khi server trả về response, chúng ta sẽ giải mã như sau:
    ```csharp
    // giải mã response body
    string encrypted_body = Encoding.UTF8.GetString(value);
    string decrypted_body = MomoPlugin.AESDecrypt(encrypted_body, headers["requestkey_decrypted"]);
    ```

- Vậy là xong, nếu bạn đã đọc tới đây thì xin cảm ơn 🎉 hãy cho mình một ⭐ nhé!
