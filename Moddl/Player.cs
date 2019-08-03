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
			var instrm = Instruments.ExponentialDecayPulseWave();

			var parser = new SimpleMmlParser();
			var ast = parser.Parse(mml);
			var instrcGen = new SimpleMmlInstructionGenerator();
			foreach (var t in instrm.ToneUsers) instrcGen.AddToneUsers(t);
			foreach (var n in instrm.NoteUsers) instrcGen.AddNoteUsers(n);
			var instrcs = instrcGen.GenerateInstructions(ast, ticksPerBeat).ToList();

			var tick = new Tick(144, ticksPerBeat);

			var seq = new Sequencer(tick, instrm.Parameters, instrcs);

			//			return ((Node<float>) osc) * ((Node<float>) env); //  + (osc * env).Delay(44100 * 0.5f * 120 / 144 * 0.75f, 0.5f, 1f, (int)(44100 * 0.5f * 120 / 144 * 0.75f) + 1);

			// TODO ステレオに正しく対応（キャストを除去）
			return (Node<float>) instrm.Output;
		}
	}
}
