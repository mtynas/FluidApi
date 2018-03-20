using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace FluidAutomationService.Security
{
    public class Security
    {
        // A 16 byte random magic code
        static byte[] Key = { 0x3a, 0x82, 0x41, 0x6d, 0xe0, 0xa5, 0xc9, 0x88, 0x8e, 0x06, 0x27, 0x7d, 0xba, 0xde, 0x98, 0x13 };
        
        // A 12 byte random magic code
        static byte[] IV = { 0x9d, 0x7f, 0xa9, 0x58, 0xb9, 0x3f, 0x40, 0xaa, 0x7a, 0x41, 0x45, 0x1e, 0x4d, 0xdd, 0x21, 0x61 };

        static readonly char[] padding = { '=' };

        // Cryptographic Key
        static public byte[] getEncryptionKey()
        {
            return Key;
        }

        // Cryptographic initialization vector
        static public byte[] getEncryptionIV()
        {
            return IV;
        }

        static public bool ValidRequest(string appkey)
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(appkey))
                {
                    string incoming = appkey.Replace('_', '/').Replace('-', '+');
                    switch(appkey.Length % 4) 
                    {
                        case 2: 
                            incoming += "==";
                            break;

                        case 3: 
                            incoming += "=";
                            break;
                    }

                    byte[] CreateDecryptor = Convert.FromBase64String(incoming);

                    using (Aes myAes = Aes.Create())
                    {
                        myAes.Key = getEncryptionKey();
                        myAes.IV = getEncryptionIV();

                        byte[] toDecryptBytes = new byte[1024];

                        using (var decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV))
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                                {
                                    cs.Write(CreateDecryptor, 0, CreateDecryptor.Length);
                                    cs.FlushFinalBlock();
                                }

                                byte[] decrypted = ms.ToArray();
                                
                                string key_result = Encoding.UTF8.GetString(decrypted);

                                if (key_result == "mike")
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

				Console.WriteLine("ValidRequest : {0}", ex.Data);

			}

            return result;
        }
    }
}