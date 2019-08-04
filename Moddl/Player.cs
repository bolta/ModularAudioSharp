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

		private float tempo = 120f;

		// TODO ModularAudioSharp.Player は実装詳細なので外に出さないようにしたいが…
		public ModularAudioSharp.Player Play(string moddl) {
			var ast = new Parser().Parse(moddl);
			var mmls = new Dictionary<string, StringBuilder>() {
				{ "a", new StringBuilder() },
				{ "b", new StringBuilder() },
				{ "c", new StringBuilder() },
			};

			foreach (var stmt in ast.Statements) {
				var dirStmt = stmt as DirectiveStatement;
				if (dirStmt != null) {
					this.ProcessDirectiveStatement(dirStmt);
					continue;
				}

				var mmlStmt = stmt as MmlStatement;
				if (mmlStmt != null) {
					foreach (var part in mmlStmt.Parts) {
						// TODO パート名が正しいかチェック
						mmls[part].AppendLine(mmlStmt.Mml);
					}
				}
			}

			var nodes = mmls.Values.Select(mml => this.MmlToNode(mml.ToString()));

			var master = nodes.Aggregate(Const(0f), (acc, node) => (Node<float>)(acc + node));
			var masterVol = 0.25f;

			return ModuleSpace.Play((master * masterVol).AsFloat());

		}

		private void ProcessDirectiveStatement(DirectiveStatement stmt) {
			if (stmt.Name == "tempo") {
				// TODO エラーチェック
				this.tempo = ((FloatValue) stmt.Arguments[0]).Value;
			}

		}


		private Node<float> MmlToNode(string mml) {
			var ticksPerBeat = 96;
			var instrm = Instruments.ExponentialDecayPulseWave();
			var temper = new EqualTemperament(440f);
			var vol = Var(1f);

			var parser = new SimpleMmlParser();
			var ast = parser.Parse(mml);
			var instrcGen = new SimpleMmlInstructionGenerator();
			foreach (var t in instrm.FreqUsers) instrcGen.AddFreqUsers(t);
			foreach (var n in instrm.NoteUsers) instrcGen.AddNoteUsers(n);
			var instrcs = instrcGen.GenerateInstructions(ast, ticksPerBeat, temper).ToList();

			var tick = new Tick(this.tempo, ticksPerBeat);

			var parameters = new Dictionary<string, VarController<float>>(instrm.Parameters) {
				{  SimpleMmlInstructionGenerator.PARAM_PART_VOLUME, vol },
			};

			var seq = new Sequencer(tick, parameters, instrcs);

			// TODO ステレオに正しく対応（キャストを除去）
			return (Node<float>) (instrm.Output * vol);
		}
	}
}
