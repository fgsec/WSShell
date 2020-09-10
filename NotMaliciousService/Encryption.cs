using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;

namespace NotMaliciousService {
	public class Encryption {

		public string comm_key = null;
		public string comm_iv = null;
		public string comm_guid = null;
		public string defaultFolder = null;

		public Encryption(string folder) {
			defaultFolder = folder;
		}

		public static string encodeBase64(string value) {
			var plt = System.Text.Encoding.UTF8.GetBytes(value);
			return System.Convert.ToBase64String(plt);
		}

		public static string decodeBase64(string value) {
			var plt = System.Convert.FromBase64String(value);
			return System.Text.Encoding.UTF8.GetString(plt);
		}

		static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");
			byte[] encrypted;
			using (Aes aesAlg = Aes.Create()) {
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
				using (MemoryStream msEncrypt = new MemoryStream()) {
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
							swEncrypt.Write(plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}
			return encrypted;

		}

		public bool setEncryptionKeys() {

			Guid g = Guid.NewGuid();
			using (Aes myAes = Aes.Create()) {
				try {

					var IV = Convert.ToBase64String(myAes.IV);
					var key = Convert.ToBase64String(myAes.Key);
					var f_key = encodeBase64(IV + "." + key + "." + g);
					File.WriteAllText(String.Format(@"{0}\{1}-{2}.txt", defaultFolder, @"SS", g), f_key);
					comm_key = key;
					comm_iv = IV;
					comm_guid = Convert.ToString(g);
					return true;
				}
				catch (Exception ex) {
					Console.WriteLine("Error during encryption:" + ex.Message);
				}
			}
			return false;
		}


		public bool encrypt(string block) {


			try {
				byte[] key = Convert.FromBase64String(comm_key);
				byte[] IV = Convert.FromBase64String(comm_iv);
				byte[] encrypted = EncryptStringToBytes_Aes(block, key, IV);
				File.WriteAllText( String.Format(@"{0}\SSO-{1}.txt", defaultFolder, comm_guid), Convert.ToBase64String(encrypted));
				return true;
			} catch (Exception ex) {
				Console.Write("Error trying to encrypt: " + ex);
			}

			return false;
		}


		public string decrypt(string block) {

			string decoded = null;

			try {

				byte[] encryptedString = Convert.FromBase64String(block);
				byte[] key = Convert.FromBase64String(comm_key);
				byte[] IV = Convert.FromBase64String(comm_iv);

				decoded = DecryptStringFromBytes_Aes(encryptedString, key, IV);

			}
			catch (Exception ex) {
				Console.Write("Error trying to decrypt: " + ex);
			}

			return decoded;
		}


		static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");
			string plaintext = null;
			using (Aes aesAlg = Aes.Create()) {
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
				using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
						using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}
			return plaintext;
		}

	

	}
}
