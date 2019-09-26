using ModularAudioSharp.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moddl {
	class Program {
		[STAThread]
		//static async Task Main(string[] args) {
		static void Main(string[] args) {
			if (args.Length < 1) {
				Console.Error.WriteLine("Specify the moddl file to play.");
				Environment.Exit(1);
				return;
			}

			var moddlPath = args[0];
			var moddl = File.ReadAllText(moddlPath);
#if true
			using (var output = new AudioOutput<float>()) {
				//await new Player().Play(moddl, output);
				new Player().Play(moddl, output);
				//Console.WriteLine("Press any key to terminate.");
				//Console.ReadKey();
				while (true) {
					Application.DoEvents();
					Thread.Sleep(10);
				}

			}
			//new Player().Play(moddl, new ConsoleOutput<float>());

#else
			var timeSecs = 30;
			var output = new NullOutput<float>(timeSecs * 44100);
			var startTime = DateTime.Now;
			new Player().Play(moddl, output);
			var elapsedTime = DateTime.Now - startTime;
			Console.WriteLine(elapsedTime);
			Console.WriteLine(string.Format("Generation efficency: {0}", timeSecs / elapsedTime.TotalSeconds));
			Console.ReadKey();
#endif

		}
	}
}
