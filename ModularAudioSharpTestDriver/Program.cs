using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using static ModularAudioSharp.Nodes;

namespace ModularAudioTestDriver {
	class Program {
		static void Main(string[] args) {
#if false
			{
				var tempo = Nodes.Const(120f);
				tempo.Update();
				var tick = Sequencer.Tick(tempo, 48);
				tick.Update();
				foreach (var t in tick.UseAsStream()) {
					Console.WriteLine(t);
					tick.Update();
				}
				return;
			}
#elif false
			{
				var tempo = 120 + 80 * SinOsc(0.125f);
				var tick = Tick.New(tempo, 48);
				var seq = SequencerExper1.New(tick, 48, 1);
				var osc = SquareOsc(seq.GetMember(s => s.Freq));

				var master = 0.125f * osc;

				using (var player = ModuleSpace.Play((Node<float>) master)) {
					//				Thread.Sleep(100 * 1000);
					for (var i = 0 ; ; ++i) {
						Console.WriteLine(i);
						Thread.Sleep(1 * 1000);
					}
				}
			}
#else
			{
				var tempo = 120 + 80 * SinOsc(0.125f);
				var tick = Tick.New(tempo, 48);
				tick.Name = "tick";
				var seq = Sequencer.New(tick, 48);

				var temper = Temperament.Equal(seq.GetMember(s => s.Tone));

				var osc = SquareOsc(temper);

				var master = 0.125f * osc;

				{
					for (var i = 0 ; i < 40 ; ++i) {
						seq.AddInstruction(48 * i + 0, o => new Sequencer.Output {
							Tone = new Tone { Octave = 3, ToneName = ToneName.A, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 12, o => new Sequencer.Output {
							Tone = new Tone { Octave = 4, ToneName = ToneName.E, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 24, o => new Sequencer.Output {
							Tone = new Tone { Octave = 4, ToneName = ToneName.A, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 36, o => new Sequencer.Output {
							Tone = new Tone { Octave = 5, ToneName = ToneName.C, Accidental = 1 },
						});
					}

					using (var player = ModuleSpace.Play((Node<float>) master)) {
						//				Thread.Sleep(100 * 1000);
						for (var i = 0 ; ; ++i) {
							Console.WriteLine(i);
							Thread.Sleep(1 * 1000);
						}
					}
				}
			}
#endif
		}
	}
}
