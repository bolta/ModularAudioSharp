using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Output {
	public class ConsoleOutput<T> : Output<T> where T : struct {
		public override void Play(IEnumerable<T> signal) {
			foreach (var value in signal) {
				Console.WriteLine(value);
			}
		}
	}
}
