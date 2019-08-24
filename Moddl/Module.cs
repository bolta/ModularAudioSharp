using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	// TODO Node のようにジェネリック版を作るべきか？
	class Module {
		public IList<ProxyController<float>> Input { get; private set; }
		public Node<float> Output { get; private set; }
		public IDictionary<string, VarController<float>> Parameters { get; private set; }
		public IEnumerable<INotable> NoteUsers { get; private set; }

		public Module(IEnumerable<ProxyController<float>> input, Node<float> output,
				IDictionary<string, VarController<float>> parameters,
				IEnumerable<INotable> noteUsers) {
			this.Input = new List<ProxyController<float>>(input);
			this.Output = output;
			this.Parameters = new Dictionary<string, VarController<float>>(parameters);
			this.NoteUsers = noteUsers;
		}

		public Module(ProxyController<float> input, Node<float> output, IDictionary<string, VarController<float>> parameters,
				IEnumerable<INotable> noteUsers) : this(new [] { input }, output, parameters, noteUsers) { }

		/// <summary>
		/// this の出力を after の入力として 2 つの module を接続し、
		/// 全体で 1 つの module とする
		/// </summary>
		/// <param name="after"></param>
		/// <returns></returns>
		public Module Then(Module after) {
			// TODO input が 1 個ではない場合は例外を投げる
			after.Input[0].Source = this.Output;
			return new Module(this.Input, after.Output,
					// TODO キーが重複すると例外が発生する。
					// 重複を回避できるよう名前にプレフィックスをつけるしくみを設け、
					// それでも重複する場合のエラー処理をちゃんとすること
					this.Parameters.Concat(after.Parameters).ToDictionary(kv => kv.Key, kv => kv.Value),
					this.NoteUsers.Concat(after.NoteUsers));
		}
	}
}
