using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace FluidAutomationService.Controllers
{
    public class SecurityController
    {

        public static String GenerateKey( String password )
		{

            String returnKey = "";

			try
			{
				using (Aes myAes    = Aes.Create())
				{
					
                    myAes.Key       = Security.Security.getEncryptionKey();
					myAes.IV        = Security.Security.getEncryptionIV();

					byte[] toEncryptBytes = Encoding.UTF8.GetBytes(password);

					Crc32 crc = new Crc32();
					crc.AddData(toEncryptBytes);
					byte[] toEncryptBytesCRC = toEncryptBytes.Concat(crc.ToByteArray()).ToArray();

					using (var encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV))
					{
						using (var ms = new MemoryStream())
						{
							using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
							{
								cs.Write(toEncryptBytesCRC, 0, toEncryptBytesCRC.Length);
								cs.FlushFinalBlock();
							}

							
							returnKey = Convert.ToBase64String(ms.ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');


						}
					}
				}

			}


            catch (Exception ex)
			{

                Console.WriteLine("\n!!!!!!!!!!!!!! ExecuteSqlCommandForScalar \n!!!!!!!!!!!!!! {0}", ex);

			}

			return returnKey;

		}
    }
}
