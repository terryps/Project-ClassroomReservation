using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using ClassroomReservation.Server;

namespace ClassroomReservation.Client {
    public delegate void OnLoginSuccess();
    public delegate void OnPasswordWrong();
    public delegate void OnLoginError(string msg);
    public delegate void OnChangeSuccess();
    public delegate void OnChangeFailed(string msg);

    class LoginClient {
        private static LoginClient instance;
        public static LoginClient getInstance() {
            if (instance == null)
                instance = new LoginClient();

            return instance;
        }

        private const string DEFAULT_PASSWORD = "1234";
        private const string KEY = "8V11o88lAXpBE0uNzSTuzHiczNFEdwf2usM0Zkpxh0wQ3rzjCIcSBRkvU183h2Uu";
        private const string PATH = @"Login.bin";
        private BinaryFormatter binformatter = new BinaryFormatter();

        public OnLoginSuccess onLoginSuccess { private get; set; }
        public OnPasswordWrong onPasswordWrong { private get; set; }
        public OnLoginError onLoginError { private get; set; }
        public OnChangeSuccess onChangeSuccess { private get; set; }
        public OnChangeFailed onChangeFailed { private get; set; }

        private LoginClient() { }

        public bool Login(string password) {
            try {
                if (password.Equals(DecryptString(Read(), KEY))) {
                    onLoginSuccess?.Invoke();
                    return true;
                } else {
                    onPasswordWrong?.Invoke();
                    return false;
                }
            } catch (Exception e) {
                onLoginError?.Invoke(e.Message);
                Write(EncryptString(DEFAULT_PASSWORD, KEY));
                return false;
            }
        }

        public bool ChangeAccount(string password) {
            try {
                Write(EncryptString(password, KEY));
                onChangeSuccess?.Invoke();
                Logger.log(password);
                return true;
            } catch (Exception e) {
                onChangeFailed?.Invoke(e.Message);
                return false;
            }
        }

        private string Read() {
            var fs = File.Open(PATH, FileMode.Open);
            string tmp = (string)binformatter.Deserialize(fs);
            fs.Close();
            return tmp;
        }

        private void Write(string text) {
            var fs = File.Create(PATH);
            binformatter.Serialize(fs, text);
            fs.Close();
        }

        public static string EncryptString(string plainText) {
            return EncryptString(plainText, KEY);
        }

        public static string DecryptString(string encryptedText) {
            return DecryptString(encryptedText, KEY);
        }

        public static string EncryptString(string plainText, string key) {
            // Rihndael class를 선언하고, 초기화 합니다
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(plainText);

            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);

            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(PlainText, 0, PlainText.Length);

            cryptoStream.FlushFinalBlock();

            byte[] CipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);

            Console.WriteLine(DecryptString(EncryptedData, key));

            return EncryptedData;
        }

        public static string DecryptString(string encryptedText, string key) {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(encryptedText);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);

            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream(EncryptedData);

            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            byte[] PlainText = new byte[EncryptedData.Length];

            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

            return DecryptedData;
        }
    }
}
