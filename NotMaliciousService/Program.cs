using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace NotMaliciousService {
	static class Program {

			static void Main(string[] args) {
				Service service = new Service();
				if (Environment.UserInteractive) {
					service.RunAsConsole(args);
				}
				else {
					ServiceBase[] ServicesToRun;
					ServicesToRun = new ServiceBase[] {
						new Service()
					};
					ServiceBase.Run(ServicesToRun);
				}
			}

		}
	}
