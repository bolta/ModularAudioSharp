using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Mml;
using ModularAudioSharp.Sequencer;
using ModularAudioSharp.Waveform;
using static ModularAudioSharp.Nodes;

namespace ModularAudioTestDriver {
	class Program {
		static void Main(string[] args) {
#if true
			//			VarSample();
			//SeqSample();
			//			ParserSample();
			//MmlSample();
			//WavetableSample();

			//NoteSample();
			StereoSample();
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
				var tick = new Tick(tempo, 48);
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
				var tick = new Tick(tempo, 48);
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
			var tick = new Tick(134, 4);
			var freq = Var(0f);
			var seq = new Sequencer(tick, new SequenceThread(new List<Instruction>(){
#region sequence
				new ValueInstruction<float>(freq, semi(3)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(14)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(19)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(14)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(3)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(13)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(19)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(13)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(3)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(12)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(19)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(12)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(3)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(11)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(19)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(11)),
				new WaitInstruction(1),
				new ValueInstruction<float>(freq, semi(15)),
				new WaitInstruction(1),
				new JumpInstruction(0),
#endregion
			}));


			var sin = SinOsc(freq) * 0.125f;

			using (ModuleSpace.Play(sin.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);

		}

		private static void ParserSample() {
			var parser = new SimpleMmlParser();

			var ast = parser.Parse("o4L8c4^2.^ ^de>>f+<g---");
			Console.WriteLine(ast);
//			var dump = new StringBuilder();
			Console.ReadKey();
		}

		private static void MmlSample() {
			var ticksPerBeat = 96;

			var env = ExpEnv(1 / 32f);
			var tone = Var<Tone>();

			var parser = new SimpleMmlParser();
//			var ast = parser.Parse("o4L4b>ef+<b>a2g+f+ed+8e8f+ee2d+2");
			var ast = parser.Parse("o5L16c>c<b>cec<b>c<c>c<b->cec<b->c<c>c<a>cec<a>c<c>c<a->cec<a->c<");
			var instrs = new SimpleMmlInstructionGenerator()
					.AddNoteUsers(env)
					.AddToneUsers(tone)
					.GenerateInstructions(ast, ticksPerBeat).ToList();

			var tick = new Tick(60, ticksPerBeat);
			var seq = new Sequencer(tick, new SequenceThread(instrs));

//			var env = ExpEnv(1 / 32f, seq.Select(v => { /*Console.WriteLine(v.NoteOperation);*/ return v.NoteOperation; }));

			var master = SinOsc(Temperament.Equal(tone)) * env * 0.125f;


			using (ModuleSpace.Play(master.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);
		}

		private static void NoteSample() {
			var env = ExpEnv(0.06125f);

			var master = SinOsc(440) * env * 0.25f;

			using (ModuleSpace.Play(master.AsFloat())) {
				var note = false;
				while (true) {
					Console.ReadKey();
					note = ! note;
					if (note) env.NoteOn(); else env.NoteOff();
				}
			}
		}

		private static void WavetableSample() {
			var ticksPerBeat = 96;

			var tone = Var<Tone>();
			var freq = Temperament.Equal(tone);

			var waveform = new Waveform(
					// 100 smp/s = 441 Hz の矩形波
					Enumerable.Repeat(1f, 50).Concat(Enumerable.Repeat(-1f, 50)).ToList(),
					44100);
			var osc = new WaveformPlayer(waveform, 441, freq, loopOffset:0);

			var parser = new SimpleMmlParser();
			var ast = parser.Parse(
				"o5L16c>c<b>cec<b>c<c>c<b->cec<b->c<c>c<a>cec<a>c<c>c<a->cec<a->c<"
			//	"o5c1^1^1^1^1"
				);
			var instrs = new SimpleMmlInstructionGenerator()
					.AddToneUsers(tone)
					.AddNoteUsers(osc)
					.GenerateInstructions(ast, ticksPerBeat).ToList();

			var tick = new Tick(134, ticksPerBeat);

			var seq = new Sequencer(tick, new SequenceThread(instrs));

			var master = (Node) osc * 0.125f;

			using (ModuleSpace.Play(master.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);

		}

		private static void StereoSample() {
			var ticksPerBeat = 96;

			var toneL = Var<Tone>();
			var freqL = Temperament.Equal(toneL);
			var envL = ExpEnv(1 / 16f);

			var toneR = Var<Tone>();
			var freqR = Temperament.Equal(toneR);
			var envR = ExpEnv(1 / 16f);

			var waveform = new Waveform(
					// 100 smp/s = 441 Hz の矩形波
					Enumerable.Repeat(1f, 50).Concat(Enumerable.Repeat(-1f, 50)).ToList(),
					44100);
			var oscL = new WaveformPlayer(waveform, 441, freqL, loopOffset: 0);
			var oscR = new WaveformPlayer(waveform, 441, freqR, loopOffset: 0);
			//var oscL = SquareOsc(freqL);
			//var oscR = SquareOsc(freqR);

			var parser = new SimpleMmlParser();
			var mmlL = @"o3L4
					ce>ce>ce>ce>c<ec<ec<ec<e<a>ca>ca>ca>cac<ac<ac<ac
					ce>ce>ce>ce>c<ec<ec<ec<e<a>ca>ca>ca>cac<ac<ac<ac
					<a>fa>fa>fa>faf<af<af<af<b>gb>gb>gb>gbg<bg<bg<bg
					<a->e-a->e-a->e-a->e-a-e-<a-e-<a-e-<a-e-<b->fb->fb->fb->fb-f<b-f<b-f<b-f
					c1
";
			var mmlR = @"o3L4c8
					dg>dg>dg>dggd<gd<gd<gd<b>eb>eb>eb>ee<be<be<be<b
					>dg>dg>dg>dggd<gd<gd<gd<b>eb>eb>eb>ee<be<be<be<b
					>cg>cg>cg>cggc<gc<gc<gcda>da>da>daad<ad<ad<ad
					cg>cg>cg>cggc<gc<gc<gcda>da>da>daad<ad<ad<ad8
					e1
";

			var instrsL = new SimpleMmlInstructionGenerator()
					.AddToneUsers(toneL)
					.AddNoteUsers(oscL, envL)
					.GenerateInstructions(parser.Parse(mmlL), ticksPerBeat).ToList();

			var instrsR = new SimpleMmlInstructionGenerator()
					.AddToneUsers(toneR)
					.AddNoteUsers(oscR, envR)
					.GenerateInstructions(parser.Parse(mmlR), ticksPerBeat).ToList();

			var tick = new Tick(156, ticksPerBeat);

			var seqL = new Sequencer(tick, new SequenceThread(instrsL));
			var seqR = new Sequencer(tick, new SequenceThread(instrsR));

			var master = ZipToStereo(
					(oscL * envL * 0.125f).AsFloat(),
					(oscR * envR * 0.125f).AsFloat());

			using (ModuleSpace.Play(master.AsStereoFloat())) Console.ReadKey();

		}
	}
}
