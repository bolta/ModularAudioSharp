using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModularAudioSharp;
using static ModularAudioSharp.Nodes;

namespace ModularAudioTestDriver {
	class Program {
		static void Main(string[] args) {
			var sin = SquareOsc(440 + 40 * SinOsc(1));
			var delayedSin = Delay(sin, 12345);

			var master = sin + sin * delayedSin;

			using (var player = ModuleSpace.Play(master)) {
				Thread.Sleep(10 * 1000);
			}

		}
	}
}
