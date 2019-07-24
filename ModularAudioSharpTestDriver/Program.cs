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
			//			VarSample();
			//SeqSample();
			//			ParserSample();
			//MmlSample();
			//WavetableSample();

			//NoteSample();
			//StereoSample();
			//WaveSample();
			PolySample();
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

			var waveform = new Waveform<float>(
					// 100 smp/s = 441 Hz の矩形波
					Enumerable.Repeat(1f, 50).Concat(Enumerable.Repeat(-1f, 50)).ToList(),
					44100);
			var osc = new WaveformPlayer<float>(waveform, 441, freq, loopOffset:0);

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

			var waveform = new Waveform<float>(
					// 100 smp/s = 441 Hz の矩形波
					Enumerable.Repeat(1f, 50).Concat(Enumerable.Repeat(-1f, 50)).ToList(),
					44100);
			//var waveform = (Waveform<float>) WavFileReader.Read(@"H:\dropbox\sounds\beam2002\pad04.wav");

			var oscL = new WaveformPlayer<float>(waveform, 441, freqL, loopOffset: 0);
			var oscR = new WaveformPlayer<float>(waveform, 441, freqR, loopOffset: 0);
			//var oscL = SquareOsc(freqL);
			//var oscR = SquareOsc(freqR);

			var parser = new SimpleMmlParser();
			var mmlL = @"o3L4
								[0
									[
										[4 ce>][4 c<e]<[4 a>c][4 ac<]>
									]
									<[4 a>f][4af<][4 b>g][4 bg<]
									[4a->e-][4a-e-<][4b->f][4b-f<]>
								]
								c1
			";
			var mmlR = @"o3L4r8
								[0
									dg>dg>dg>dggd<gd<gd<gd<b>eb>eb>eb>ee<be<be<be<b
									>dg>dg>dg>dggd<gd<gd<gd<b>eb>eb>eb>ee<be<be<be<b
									>cg>cg>cg>cggc<gc<gc<gcda>da>da>daad<ad<ad<ad
									cg>cg>cg>cggc<gc<gc<gcda>da>da>daad<ad<ad<ad
								]
								e1
			";
			//var mmlL = "o3L4r4c16";
			//			var mmlR = "";

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

			var cutoff = Var(1760f);

//			var lfo = SinOsc(0.0625f) + 0.02f * SinOsc(40f + 20 * SinOsc(3f));
			var lfo = 0.5f + 0.4f * SinOsc(5f + 2.5f * SinOsc(5f)); //.Delay(400 0, 0.9f, 0.9f, 4000);

			var master = ZipToStereo(
					//((Lpf(oscL, ((Node) cutoff).AsFloat(), Const(9f)) * envL).Limit(-1f/32, 1f/32)*4).AsFloat(),
					(oscL * envL * 0.5f * lfo)/*.Delay(300 + 280 * lfo, 0.9f, 0.25f, 21499)*/.AsFloat(),
					(oscR * envR * 0.5f * lfo)/*.Delay(300 + 280 * lfo, 0.9f, 0.25f, 21499)*/.AsFloat());

			//new Thread(() => {
			//	var rand = new Random();
			//	while (true) {
			//		Thread.Sleep(500);
			//		Console.Write("* ");
			//		cutoff.Set(rand.Next(4900) + 100);
			//	}
			//}).Start();
			using (ModuleSpace.Play(master.AsStereoFloat())) {
				//Thread.Sleep(10000);
				Console.ReadKey();
			}
			Console.WriteLine(Node.TimesUpdated);
			Console.ReadKey();
		}

		private static void WaveSample() {
			var ticksPerBeat = 96;

			var tone = Var<Tone>();
			var freq = Temperament.Equal(tone);

			var waveform = (Waveform<float>) WavFileReader.Read(@"H:\dropbox\sounds\beam2002\pad03.wav");

			var osc = WaveformPlayer.Create(waveform, 261.6256f /* C4 */, freq, endOffset: 44100+11025, loopOffset:44100);
			//var oscL = SquareOsc(freqL);
			//var oscR = SquareOsc(freqR);

			var parser = new SimpleMmlParser();
			var mml = @"o5L12 arba2 arba2 arbar>d<baf gbae2 ^2r4 dred4.g8c+egb-agec+d f<a->ce2 d4c2 ^2<b-4 a8a8a2";


			var instrs = new SimpleMmlInstructionGenerator()
					.AddToneUsers(tone)
					.AddNoteUsers(osc/*, envL*/)
					.GenerateInstructions(parser.Parse(mml), ticksPerBeat).ToList();


			var tick = new Tick(110, ticksPerBeat);

			var seq = new Sequencer(tick, new SequenceThread(instrs));

			var master = (Node) osc * 0.125f;

			using (ModuleSpace.Play(master.AsStereoFloat())) {
				//Thread.Sleep(10000);
				Console.ReadKey();
			}
			Console.WriteLine(Node.TimesUpdated);
			Console.ReadKey();
		}

		private static void PolySample() {
			var ticksPerBeat = 96;

			Func<string, Node> mmlOsc = mml => {
				var tone = Var<Tone>();

				var freq = Temperament.Equal(tone, 440);

				var waveform = new Waveform<float>(
						// 100 smp/s = 441 Hz の矩形波
						Enumerable.Repeat(1f, 13).Concat(Enumerable.Repeat(-1f, 87)).ToList(),
						44100);
				var osc = new WaveformPlayer<float>(waveform, 441, freq, loopOffset:0);

				var env = ExpEnv(1 / 8f);

				var parser = new SimpleMmlParser();
				var ast = parser.Parse(mml);
				var instrs = new SimpleMmlInstructionGenerator()
						.AddToneUsers(tone)
						.AddNoteUsers(osc, env)
						.GenerateInstructions(ast, ticksPerBeat).ToList();

				var tick = new Tick(144, ticksPerBeat);

				var seq = new Sequencer(tick, new SequenceThread(instrs));

				return osc * env; //  + (osc * env).Delay(44100 * 0.5f * 120 / 144 * 0.75f, 0.5f, 1f, (int)(44100 * 0.5f * 120 / 144 * 0.75f) + 1);

//				return Portamento(osc, 0.1f);
			};

			var oscA = mmlOsc(@"o5L4
g+d+ef+ g2f+e e1 d+2...r16
c2e2 d+2^8.r16<b> e2g2 f+2...r16
e2g2 f+2^8.r16d+4 a2>c2< bag+f+
o4L4
b>ef+<b> a2g+f+ ed+8e8f+ee2d+2 c+f+g+c+ b2ag+ f+f8g+8f+c+ g+2f+2<
b>ef+<b> a2g+f+ ed+8e8f+ee2d+2 c+f+g+c+ b2ag+ f+f8g+8f+c+ g+2f+f+16g+16a16b16>
c+2^8r8c+< b2^8r8g+ aa8g+8f+f f+g+ab>
d2^8r8d c+2^8r8<a bb8>c8<bb8>c8< bag+f+<");
			var oscB = mmlOsc(@"o4L8
b2g4ab> c4<b4a4g4 f+2ef+ga b4f+ga4d+4
e4ede4g4 f+4f+ef+2 >c4<cdef+gab4f+4d+4<b4>
g2e4g4 <b4b>c+d+2 e4f+4g4e4 d+4c+4<b4a4
o3L4
b2.>c+8d+8ec+d+f+ c+d+8e8f+g+8a+8 bf+d+<b> f+2ff+8g+8 af+fb a2a+2 b2a2<
b2.>c+8d+8ec+d+f+ c+d+8e8f+g+8a+8 bf+d+<b> f+2ff+8g+8 af+fb a2a+2 b2a2
L8 e4ag+a4e4 e4bag+f+eg+> d<af+4g+c+d+f f+4<b>c+def+4
g2^gab a2^agf+ e4edcde4 d+2e4f+4<
L4
");
			var oscC = mmlOsc(@"o3L2
ec <a>d< b>f+ b<b>>
L8 c4<cdef+ga brf+rd+r<br> erede4c4 f+rf+ef+2
cdef+g4f+e d+ef+ga4b4> L2 c<a f+<b
o3L2
ed+ c+<b aa+ b1> ag+ f+f f+e d+<b>
ed+ c+c c+<a+ b1> ag+ f+f f+e d+<b>
L8 ar<ab>c+d+ef+ g+rg+f+g+rer f+4d4c+4bg+ f+4e4d4c+4<
br>babrgr f+r>c+r<ar<ab> cr>cr<ef+g4 f+4d+4<b4a4>
L2");
			var master = oscA * 0.25f * 15 / 15
					+ oscB * 0.25f * 11 / 15
					+ oscC * 0.25f * 13 / 15;
			;
			using (ModuleSpace.Play(master.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);
		}
	}
}
