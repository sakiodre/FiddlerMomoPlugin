using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using Newtonsoft.Json;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Encodings;

using Fiddler;
using Standard;

using CConsoleW;

[assembly: Fiddler.RequiredVersion("5.0.0.0")]

public class MomoPlugin : IAutoTamper
{
    AsymmetricKeyParameter momoPublicKey = null;
    AsymmetricKeyParameter injectedPrivateKey = null;

    string defaultMomoPublicKey =
@"-----BEGIN RSA PUBLIC KEY-----
MEgCQQDjtTNZJnbMWXON/mhhLzENzQW8TOH/gaOZ72u6FEzfjyWSfGsP6/rMIVjY
2w44ZyqNG2p45PGmp3Y8bquPAQGnAgMBAAE=
-----END RSA PUBLIC KEY-----
";

    // our key pair
    string injectedPublicKeyPEM =
@"-----BEGIN PUBLIC KEY-----
MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgHkGaaizp9IBufI/ItthwVyY9HZG
RE2NyAsjdVlFPbYDFc5c7afiTin2a2tmTVFEpU6tveYO68Sy7PoR9fAyxILddB8c
dOTm3Dc6s+5jxn4eyvXm3J0/tt8ypOH/jJv6Gfn7myRdnELXZvPVU5TlQ/gxjO8/
IVJbyJ8D6veWTdknAgMBAAE=
-----END PUBLIC KEY-----
";
    string injectedPrivateKeyPEM =
@"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgHkGaaizp9IBufI/ItthwVyY9HZGRE2NyAsjdVlFPbYDFc5c7afi
Tin2a2tmTVFEpU6tveYO68Sy7PoR9fAyxILddB8cdOTm3Dc6s+5jxn4eyvXm3J0/
tt8ypOH/jJv6Gfn7myRdnELXZvPVU5TlQ/gxjO8/IVJbyJ8D6veWTdknAgMBAAEC
gYA5GGNkaU008AePV2XUZavZSXebDM9QXyEO0C2ebeINKitxbbnYyBTkCxLmWh8D
xgTOt3ytOkDUTS0rVKnYJbs68SfiDoTp6eVy0mZ36WjCa70n7PA7A36PkJyGy7c5
3o1X+dVFM2ZpuNTsCpRVk/GcMbDIa/ZJUFdSLtVqMkdpkQJBANG5YJLWngvcZUzD
DvxCcvvq699nAZP9wKicDRukiPfT78g82nrNoXCK9gSSeoRLcANYieEnd1d7UnZ0
FD6INcMCQQCTure6ZVLWkQc0OiXVszUlCAz+GgxyOwXwIC+ewIhWfMQb+bHtLUcb
UvvhztR14E26zpWmUgsNTifjO0wBEUTNAkBnUWnwLObDdRo6jMWvJJU84ei9sCMo
4xOsfORAP/hyDujLvp7rbK4hoXO9oOPWlMtj+pRCZYz3ffuL+3eVrsi9AkBj9RsM
DDMY69isBgxDpJZ5EBF6fbWeNUG3UI/oIu4dVx56r2Es0k4ituunxoMLn1H47H9l
om3H+vISVrT+o+ihAkEAre9rCJOoAp3d/PjwbLgurq3HMt+4IYj8cjdX+GjrDgZf
30vOspu5mO9v4yXiXz8fWiNdNkTkW9af1DNf1fnseg==
-----END RSA PRIVATE KEY-----
";

    public static string authToken = null;

    public MomoPlugin()
    {
        var stream = new StringReader(injectedPrivateKeyPEM);
        var keyPair = (AsymmetricCipherKeyPair)new PemReader(stream).ReadObject();
        injectedPrivateKey = keyPair.Private;

        stream = new StringReader(defaultMomoPublicKey);
        momoPublicKey = (AsymmetricKeyParameter)new PemReader(stream).ReadObject();

#if DEBUG
        CConsole.Initialize();
        CConsole.LogGreen("[D] Running in debug mode");
#endif

    }


    private string RSAEncryptWithMomoPublicKey(string data)
    {
        var encryptEngine = new Pkcs1Encoding(new RsaEngine());

        var bytesToEncrypt = Encoding.UTF8.GetBytes(data);

        try
        {
            encryptEngine.Init(true, momoPublicKey);
        }
        catch (Exception e)
        {
            CConsole.LogRed("RSAEncryptWithMomoPublicKey error: " + e.Message);
            return null;
        }

        return Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
    }

    private string RSADecryptWithInjectedPrivateKey(string base64_encrypted)
    {
        var bytesToDecrypt = Convert.FromBase64String(base64_encrypted);

        var decryptEngine = new Pkcs1Encoding(new RsaEngine());

        try
        {
            decryptEngine.Init(false, injectedPrivateKey);
            var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            return decrypted;
        }
        catch (Exception e)
        {
            CConsole.LogRed("RSADecryptWithInjectedPrivateKey error: " + e.Message);
            return null;
        }

    }

    public static string AESEncrypt(string data, string key)
    {
        byte[] secret = Encoding.UTF8.GetBytes(data);
        byte[] _key = Encoding.UTF8.GetBytes(key);

        using (MemoryStream ms = new MemoryStream())
        {
            using (AesManaged cryptor = new AesManaged())
            {
                cryptor.Mode = CipherMode.CBC;
                cryptor.Padding = PaddingMode.PKCS7;
                cryptor.KeySize = 256;
                cryptor.BlockSize = 128;

                byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(_key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(secret, 0, secret.Length);
                    }
                }
                catch (Exception e)
                {
                    CConsole.LogRed("AESEncrypt error: " + e.Message);
                    return null;
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string AESDecrypt(string data, string key)
    {
        byte[] secret = Convert.FromBase64String(data);
        byte[] _key = Encoding.UTF8.GetBytes(key);
        byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (AesManaged cryptor = new AesManaged())
            {
                cryptor.Mode = CipherMode.CBC;
                cryptor.Padding = PaddingMode.PKCS7;
                cryptor.KeySize = 256;
                cryptor.BlockSize = 128;

                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(_key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(secret, 0, secret.Length);

                    }
                }
                catch (Exception e)
                {
                    CConsole.LogRed("AESDecrypt error: " + e.Message);
                    return null;
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }

    public void OnLoad() { }
    public void OnBeforeUnload() { }

    public void AutoTamperRequestBefore(Session oSession)
    {
        if (!oSession.url.StartsWith("api.momo.vn/") && !oSession.url.StartsWith("owa.momo.vn/")) return;
        if ((oSession.oRequest["Authorization"] != "") && (oSession.oRequest["Authorization"].StartsWith("Bearer ")))
        {
            authToken = oSession.oRequest["Authorization"].Substring(7);
        }

        if (oSession.oRequest["requestkey"] == "") return;

        // decrypt the requestkey with our private key
        string aes_key = RSADecryptWithInjectedPrivateKey(oSession.oRequest["requestkey"]);

        if (aes_key == null)
        {
            CConsole.LogYellow("[!] No encryption key, please log out and sign in again.");
            return;
        }

        // encrypt it back using momo server public key
        oSession.oRequest["requestkey"] = RSAEncryptWithMomoPublicKey(aes_key);

        // put the decrypted key in the header for later usage in the response handling part
        oSession.oRequest["aes_key"] = aes_key;

        // decryption is expensive, check if we had the console opened else it is wasting resources for nothing.
        if (CConsole.isOpen)
        {
            string decrypted_data = AESDecrypt(Encoding.UTF8.GetString(oSession.RequestBody), aes_key);
            CConsole.LogMagenta("[>] " + oSession.url);
            CConsole.LogGray(decrypted_data);
        }
    }

    // we handle the edit/repeat request here
    public void AutoTamperRequestAfter(Session oSession) {

        if (!oSession.url.StartsWith("api.momo.vn/") && !oSession.url.StartsWith("owa.momo.vn/")) return;

        // make sure the request has gone through AutoTamperRequestBefore
        if (oSession.oRequest["aes_key"] == "") return;

        string aes_key = oSession.oRequest["aes_key"];

        // if the body is not encrypted, it is probably the user is trying to send something, we should encrypt it.
        try
        {
            string decrypted_data = AESDecrypt(Encoding.UTF8.GetString(oSession.RequestBody), aes_key);
        }
        catch (Exception e)
        {

            string request_body = Encoding.UTF8.GetString(oSession.RequestBody);
            string encrypted_request = AESEncrypt(request_body, aes_key);

            oSession.RequestBody = Encoding.UTF8.GetBytes(encrypted_request);
        }
    }

    public void AutoTamperResponseBefore(Session oSession)
    {
        // inject our public key on login
        if (oSession.url == "owa.momo.vn/public/login")
        {
            // uncompress the response;
            oSession.utilDecodeResponse();

            string body = Encoding.UTF8.GetString(oSession.ResponseBody);
            dynamic response = JsonConvert.DeserializeObject<dynamic>(body);

            if ((response == null) || (!(bool)response.result))
            {
                CConsole.LogRed("Cannot get the public key, this might be due to an unsuccessful login attempt");
                return;
            }

            // get the original public key
            string publicKeyPEM = response.extra.REQUEST_ENCRYPT_KEY;
            StringReader stream = new StringReader(publicKeyPEM);
            momoPublicKey = (AsymmetricKeyParameter)new PemReader(stream).ReadObject();

            // put our public key into response
            response.extra.REQUEST_ENCRYPT_KEY = injectedPublicKeyPEM;

            // modify the response body
            string newResponse = JsonConvert.SerializeObject(response);
            oSession.ResponseBody = Encoding.UTF8.GetBytes(newResponse);


            authToken = (string)response.extra.AUTH_TOKEN;

            CConsole.LogCyan("[+] Public key injected");
            CConsole.LogGray("[-] Original public key:\n" + publicKeyPEM);
            CConsole.LogGreen("[A] Authorization: ", false);
            CConsole.LogWhite(authToken);

        }
        // or decrypt the request data
        else if (oSession.oRequest["requestkey"] != "")
        {
            if (oSession.oRequest["aes_key"] == "") return;

            // uncompress the response;
            oSession.utilDecodeResponse();

            // decrypt the request data
            string post_data = Encoding.UTF8.GetString(oSession.RequestBody);
            string aes_key = oSession.oRequest["aes_key"];
            string decrypted_post_data = AESDecrypt(post_data, aes_key);

            oSession.RequestBody = Encoding.UTF8.GetBytes(decrypted_post_data);
            oSession.oResponse["aes_key"] = aes_key;
        }
    }
    public void AutoTamperResponseAfter(Session oSession) { }
    public void OnBeforeReturningError(Session oSession) { }
}


// http://fiddler.wikidot.com/extending-execaction-with-net
public class MomoPluginCmd : IFiddlerExtension, IHandleExecAction
{
    public void OnLoad()
    {
    }
    public void OnBeforeUnload()
    {
    }

    //Required Function - Used to handle an ExecAction command
    public bool OnExecAction(string action)
    {
        if ((action == "debug_momo") || (action == "momo_debug"))
        {
            if (CConsole.Initialize())
            {
                CConsole.LogGreen("[+] MomoPlugin loaded!");
            }

            return true;  //The command was handled successfully
        }
        else if ((action == "auth_momo") || (action == "momo_auth"))
        {
            CConsole.Initialize();
            if (MomoPlugin.authToken == null)
            {
                CConsole.LogYellow("[!] There is no token stored, please sign in and make a request to get your token");
            }
            else
            {
                CConsole.LogGreen("[A] Authorization: ", false);
                CConsole.LogWhite(MomoPlugin.authToken);
            }

            return true;
        }

        return false;  //The command was not handled and allows other ExecAction handlers to process
    }
}

    public class MomoPluginResponseJsonViewer : Inspector2, IResponseInspector2
{
    JSONResponseViewer jsonResponseViewer;
    HTTPResponseHeaders responseHeaders;

    public byte[] body
    {
        get
        {
            return jsonResponseViewer.body;
        }
        set
        {
            // we have already decrypted the key when sending the request
            if (value != null && value.Length > 0 && headers != null && headers["aes_key"] != "")
            {
                string encrypted_body = Encoding.UTF8.GetString(value);
                string decrypted_body = MomoPlugin.AESDecrypt(encrypted_body, headers["aes_key"]);
                jsonResponseViewer.body = Encoding.UTF8.GetBytes(decrypted_body);
            }
            else
            {
                jsonResponseViewer.body = value;
            }
        }
    }

    public bool bDirty
    {
        get { return jsonResponseViewer.bDirty; }
    }
    public bool bReadOnly
    {
        get { return jsonResponseViewer.bReadOnly; }
        set { jsonResponseViewer.bReadOnly = value; }
    }

    public HTTPResponseHeaders headers
    {
        get { return responseHeaders; }
        set { responseHeaders = value; }
    }
    public void Clear() { jsonResponseViewer.Clear(); }
    public override void AssignSession(Session oS) { base.AssignSession(oS); }
    public override int GetOrder() { return jsonResponseViewer.GetOrder(); }
    public override void SetFontSize(float flSizeInPoints) { jsonResponseViewer.SetFontSize(flSizeInPoints); }
    public override void AddToTab(System.Windows.Forms.TabPage o)
    {
        jsonResponseViewer = new JSONResponseViewer();
        jsonResponseViewer.AddToTab(o);
        o.Text = "JSON Decrypted";
    }
}

public class MomoPluginResponseTextViewer : Inspector2, IResponseInspector2
{
    ResponseTextViewer textResponseViewer;
    HTTPResponseHeaders responseHeaders;

    public byte[] body
    {
        get
        {
            return textResponseViewer.body;
        }
        set
        {
            // we have already decrypted the key when sending the request
            if (value != null && value.Length > 0 && headers != null && headers["aes_key"] != "")
            {
                string encrypted_body = Encoding.UTF8.GetString(value);
                string decrypted_body = MomoPlugin.AESDecrypt(encrypted_body, headers["aes_key"]);
                textResponseViewer.body = Encoding.UTF8.GetBytes(decrypted_body);
            }
            else
            {
                textResponseViewer.body = value;
            }
        }
    }

    public bool bDirty
    {
        get { return textResponseViewer.bDirty; }
    }
    public bool bReadOnly
    {
        get { return textResponseViewer.bReadOnly; }
        set { textResponseViewer.bReadOnly = value; }
    }

    public HTTPResponseHeaders headers
    {
        get { return responseHeaders; }
        set { responseHeaders = value; }
    }
    public void Clear(){ textResponseViewer.Clear(); }
    public override void AssignSession(Session oS){ base.AssignSession(oS); }
    public override int GetOrder(){ return textResponseViewer.GetOrder(); }
    public override void SetFontSize(float flSizeInPoints){ textResponseViewer.SetFontSize(flSizeInPoints); }
    public override void AddToTab(System.Windows.Forms.TabPage o)
    {
        textResponseViewer = new ResponseTextViewer();
        textResponseViewer.AddToTab(o);
        o.Text = "TextView Decrypted";
    }
}