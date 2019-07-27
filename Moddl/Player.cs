using Moddl.Language;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Mml;
using ModularAudioSharp.Sequencer;
using ModularAudioSharp.Waveform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Mml;
using ModularAudioSharp.Sequencer;
using ModularAudioSharp.Waveform;
using static ModularAudioSharp.Nodes;

namespace Moddl {
	public class Player {
		public void Play(string moddl) {
			var ast = new Parser().Parse(moddl);
			var mmls = new Dictionary<string, StringBuilder>() {
				{ "a", new StringBuilder() },
				{ "b", new StringBuilder() },
				{ "c", new StringBuilder() },
			};

			foreach (var stmt in ast.Statements) {
				var mmlStmt = stmt as MmlStatement;
				if (mmlStmt == null) continue;

				foreach (var part in mmlStmt.Parts) {
					// TODO パート名が正しいかチェック
					mmls[part].AppendLine(mmlStmt.Mml);
				}
			}

			var nodes = mmls.Values.Select(mml => MmlToNode(mml.ToString()));

			var master = nodes.Aggregate(Const(0f), (acc, node) => (Node<float>)(acc + node * 0.25f));
			using (ModuleSpace.Play(master.AsFloat())) Console.ReadKey(); // Thread.Sleep(10000);

		}

		private static Node<float> MmlToNode(string mml) {
			var ticksPerBeat = 96;
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

//			return ((Node<float>) osc) * ((Node<float>) env); //  + (osc * env).Delay(44100 * 0.5f * 120 / 144 * 0.75f, 0.5f, 1f, (int)(44100 * 0.5f * 120 / 144 * 0.75f) + 1);
			return (Node<float>) (osc * env);
//				return Portamento(osc, 0.1f);

		}
	}
}
