using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	class Program {
		static void Main(string[] args) {
			if (args.Length < 1) {
				Console.Error.WriteLine("Specify the moddl file to play.");
				Environment.Exit(1);
				return;
			}

			var moddlPath = args[0];
			var moddl = File.ReadAllText(moddlPath);
			using (new Player().Play(moddl)) {
				Console.WriteLine("Press any key to terminate.");
				Console.ReadKey();
			}
		}
	}
}
