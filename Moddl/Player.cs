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

		private readonly Dictionary<string, Instrument> instruments = new Dictionary<string, Instrument>();
		private readonly Evaluator evaluator = new Evaluator();

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
					foreach (var track in mmlStmt.Tracks) {
						// TODO パート名が正しいかチェック
						mmls[track].AppendLine(mmlStmt.Mml);
					}
				}
			}

			var nodes = mmls.Select(kv => this.MmlToNode(kv.Key, kv.Value.ToString()));

			var master = nodes.Aggregate(Const(0f), (acc, node) => (Node<float>)(acc + node));
			var masterVol = 0.25f;

			return ModuleSpace.Play((master * masterVol).AsFloat());

		}

		private void ProcessDirectiveStatement(DirectiveStatement stmt) {
			if (stmt.Name == "tempo") {
				// TODO エラーチェック
				this.tempo = this.evaluator.Evaluate(stmt.Arguments[0]).AsFloat()
						// TODO エラーチェック
						.Value;

			} else if (stmt.Name == "instrument") {
				// TODO 引数の型・数のチェック
				var tracks = this.evaluator.Evaluate(stmt.Arguments[0]).AsTrackSet();

				foreach (var track in tracks) {
					var instrm = this.evaluator.Evaluate(stmt.Arguments[1]).AsInstrument();
					// TODO 重複設定はエラーにする
					this.instruments.Add(track, instrm);
				}
			}
		}

		private Node<float> MmlToNode(string track, string mml) {
			var ticksPerBeat = 96;
			// TODO 該当トラックにインストゥルメントが定義されていないとこけるので、エラー処理
			var instrm = this.instruments[track];
			var temper = new EqualTemperament(440f);
			var vol = Var(1f);

			var parser = new SimpleMmlParser();
			var ast = parser.Parse(mml);
			var instrcGen = new SimpleMmlInstructionGenerator();
			var freq = Var<float>();
			// TODO Input がちょうど 1 つでない場合はエラー
			instrm.Input[0].Source = freq;
			instrcGen.AddFreqUsers(freq);
			foreach (var n in instrm.NoteUsers) instrcGen.AddNoteUsers(n);
			var instrcs = instrcGen.GenerateInstructions(ast, ticksPerBeat, temper).ToList();

			var tick = new Tick(this.tempo, ticksPerBeat);

			var parameters = new Dictionary<string, VarController<float>>(instrm.Parameters) {
				{  SimpleMmlInstructionGenerator.PARAM_TRACK_VOLUME, vol },
			};

			var seq = new Sequencer(tick, parameters, instrcs);

			// TODO ステレオに正しく対応（キャストを除去）
			return (Node<float>) (instrm.Output * vol);
		}
	}
}
