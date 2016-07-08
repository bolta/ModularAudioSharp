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
			//var sin = SquareOsc((Node<float>)(440 + 40 * SinOsc(1)));
			//var delayedSin = Delay(sin, 12345);

			//var master = sin + sin * delayedSin;

			var seq = SequencerExper1.New(120, 1);
			var osc = SinOsc(seq.GetMember(s => s.Freq));

			var master = osc;

			using (var player = ModuleSpace.Play((Node<float>)master)) {
				//				Thread.Sleep(100 * 1000);
				for (var i = 0 ; ; ++i) {
					Console.WriteLine(i);
					Thread.Sleep(1 * 1000)
;				}
			}

		}
	}
}
