using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Mml;
using ModularAudioSharp.Sequencer;
using static ModularAudioSharp.Nodes;

namespace ModularAudioTestDriver {
	class Program {
		static void Main(string[] args) {
#if true
//			VarSample();
//			SeqSample();
			ParserSample();
#elif true
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
#elif true
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
				var seq = Sequencer_old.New(tick, 48);

				var temper = Temperament.Equal(seq.GetMember(s => s.Tone));

				var osc = SquareOsc(temper);

				var master = 0.125f * osc;

				{
					for (var i = 0 ; i < 40 ; ++i) {
						seq.AddInstruction(48 * i + 0, o => new Sequencer_old.Output {
							Tone = new Tone { Octave = 3, ToneName = ToneName.A, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 12, o => new Sequencer_old.Output {
							Tone = new Tone { Octave = 4, ToneName = ToneName.E, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 24, o => new Sequencer_old.Output {
							Tone = new Tone { Octave = 4, ToneName = ToneName.A, Accidental = 0 },
						});
						seq.AddInstruction(48 * i + 36, o => new Sequencer_old.Output {
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
		private static void VarSample() {
			var freq = Var(440f);
			var sin = SinOsc(freq) * 0.125f;

			var rand = new Random();
			//using (ModuleSpace.Play(sin.AsFloat())) Thread.Sleep(2000);
			using (ModuleSpace.Play(sin.AsFloat())) {
				while (true) {
//					Console.ReadKey();
					Thread.Sleep(125);

					var f = (float) rand.NextDouble() * (1760 - 110) + 110;
					Console.WriteLine(f);
					freq.Set(f);
				}
			}

		}

		private static void SeqSample() {
//			var tempo = Const(160f);
			Func<int, float> semi = s => (float) (440 * Math.Pow(2, s / 12.0));
			var tick = Tick.New(120, 4);
			var seq = Sequencer<float>.New(tick, 0, new SequenceThread<float>(new List<Instruction<float>>(){
				new ValueInstruction<float>(semi(3)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(14)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(19)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(14)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(3)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(13)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(19)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(13)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(3)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(12)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(19)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(12)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(3)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(11)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(19)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(11)),
				new WaitInstruction<float>(1),
				new ValueInstruction<float>(semi(15)),
				new WaitInstruction<float>(1),
				new JumpInstruction<float>(0),
			}));


			var sin = SinOsc(seq) * 0.125f;

			using (ModuleSpace.Play(sin.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);

		}

		private static void ParserSample() {
			var parser = new SimpleMmlParser();

			var ast = parser.Parse("o4L8c4^2.^^de");
			Console.WriteLine(ast);
			Console.ReadKey();
		}

	}
}
