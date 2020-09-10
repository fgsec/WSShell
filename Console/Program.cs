using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Shell {
	class Program {

		public static string defaultFolder = @"C:\Users\Public";
		public static string keyFile = null;

		
		static bool waitForKey() {

			string[] fileEntries = Directory.GetFiles(defaultFolder);
			foreach (string fileName in fileEntries) {
				if (Path.GetFileName(fileName).Split('-').Length == 6 && Path.GetFileName(fileName).Split('-')[0] == "SS") {
					keyFile = fileName;
					return false;
				}
			}
			return true;
		}

		public static string readReply(Encryption encryption) {

			string result = null;

			try {
				string filename = String.Format(@"{0}\SSO-{1}.txt", encryption.defaultFolder, encryption.comm_guid);
				result = encryption.decrypt(File.ReadAllText(filename));
				File.Delete(filename);
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
			}

			return result;

		}

		static bool waitForReply(string guid) {
			var file = String.Format(@"{0}\SSO-{1}.txt", defaultFolder, guid);
			try {
				if (File.Exists(file)) {
					try {
						File.Open(file, FileMode.Open, FileAccess.Read).Dispose();
						return false;
					}
					catch (IOException) {
						return true;
					}
				}
			} catch (Exception ex) {
				Console.WriteLine("lock!");
				return true;
			}
			
			return true;
			
		}

		static void startConsole(Encryption encryption) {

			while(true) {
				Console.Write("console > ");
				encryption.encrypt(Console.ReadLine());
				while (waitForReply(encryption.comm_guid)) { }
				var reply = readReply(encryption);
				Console.WriteLine("Received: {0}", reply);
			}
			
			
		}

		static void Main(string[] args) {

			Console.WriteLine("Starting!");
			Console.Write("Waiting for key to be created by service");

			while (waitForKey()) {
				Console.Write(".");
				Thread.Sleep(1000);
			}
			Console.WriteLine("\n");

			Encryption encryption = new Encryption(defaultFolder);

			if(encryption.readKey(keyFile)) {
				Console.WriteLine("Opening console!");
				startConsole(encryption);
			}
			
			Console.ReadKey();

		}
	}
}
