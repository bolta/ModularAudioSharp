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

		private readonly Dictionary<string, Module> instruments = new Dictionary<string, Module>();
		private readonly Evaluator evaluator = new Evaluator();

		// TODO ModularAudioSharp.Player は実装詳細なので外に出さないようにしたいが…
		public ModularAudioSharp.Player Play(string moddl) {
			var ast = new Parser().Parse(moddl);
			var mmls = new Dictionary<string, StringBuilder>();

			foreach (var stmt in ast.Statements) {
				if (stmt is DirectiveStatement) {
					this.ProcessDirectiveStatement((DirectiveStatement) stmt);

				} else if (stmt is MmlStatement) {
					var mmlStmt = (MmlStatement) stmt;
					foreach (var track in mmlStmt.Tracks) {
						if (! mmls.ContainsKey(track)) {
							mmls.Add(track, new StringBuilder());
						}
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
					var instrm = this.evaluator.Evaluate(stmt.Arguments[1]).AsModule();
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
			// TODO input が複数の場合はとりあえず全てに freq を設定するが、
			// Input がちょうど 1 つでない場合はエラーにすべきか？
			foreach (var i in instrm.Input) i.Source = freq;
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
