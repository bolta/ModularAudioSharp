using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moddl {
	// TODO Node のようにジェネリック版を作るべきか？
	class Module {
		public IList<ProxyController<float>> Input { get; private set; }
		public Node<float> Output { get; private set; }
		public IDictionary<string, ProxyController<float>> Parameters { get; private set; }
		public IEnumerable<INotable> NoteUsers => this.noteUsers;
		private readonly List<INotable> noteUsers;

		public IEnumerable<Control> Gui => this.gui;
		private readonly List<Control> gui;

		public Module(IEnumerable<ProxyController<float>> input, Node<float> output,
				IDictionary<string, ProxyController<float>> parameters,
				IEnumerable<INotable> noteUsers,
				IEnumerable<Control> gui = null) {
			this.Input = new List<ProxyController<float>>(input);
			this.Output = output;
			this.Parameters = new Dictionary<string, ProxyController<float>>(parameters);
			this.noteUsers = new List<INotable>(noteUsers);
			this.gui = new List<Control>(gui ?? Enumerable.Empty<Control>());
		}

		public Module(ProxyController<float> input, Node<float> output,
				IDictionary<string, ProxyController<float>> parameters,
				IEnumerable<INotable> noteUsers,
				IEnumerable<Control> gui = null) : this(new [] { input }, output, parameters, noteUsers, gui) { }

		public static Module FromFloat(float value) {
			return new Module(Enumerable.Empty<ProxyController<float>>(),
					Nodes.Var(value),
					new Dictionary<string, ProxyController<float>>(),
					Enumerable.Empty<INotable>());
		}

		internal void AddGui(params Control[] gui) => this.AddGui((IEnumerable<Control>) gui);

		internal void AddGui(IEnumerable<Control> gui) {
			this.gui.AddRange(gui);
		}

		private void AddParameters(IDictionary<string, ProxyController<float>> parameters) {
			foreach (var p in parameters) {
				this.Parameters.Add(p.Key, p.Value);
			}
		}

		private void AddNoteUsers(IEnumerable<INotable> users) {
			this.noteUsers.AddRange(users);
		}

		internal void AddLabel(string label) {
			this.Parameters = this.Parameters.ToDictionary(kv => $"{label}.{kv.Key}", kv => kv.Value);
		}

		internal void AssignModuleToParameter(string name, Module paramModule) {
			this.Parameters[name].Source = paramModule.Output;
			this.AddParameters(paramModule.Parameters);
			this.AddNoteUsers(paramModule.NoteUsers);
			this.AddGui(paramModule.gui);
		}

		/// <summary>
		/// this の出力を after の入力として 2 つの module を接続し、
		/// 全体で 1 つの module とする
		/// </summary>
		/// <param name="after"></param>
		/// <returns></returns>
		public Module Then(Module after) {
			// TODO input が 1 個ではない場合これでよいか？
			foreach (var i in after.Input) i.Source = this.Output;
			return new Module(this.Input, after.Output,
					// TODO キーが（プレフィックスをつけてもなお）重複すると例外が発生する。
					// その場合のエラー処理をちゃんとすること
					this.Parameters.Concat(after.Parameters).ToDictionary(kv => kv.Key, kv => kv.Value),
					this.NoteUsers.Concat(after.NoteUsers),
					this.gui.Concat(after.gui));
		}

		public Module Add(Module that) => this.Binary(that, (lhs, rhs) => (lhs + rhs).AsFloat());
		public Module Subtract(Module that) => this.Binary(that, (lhs, rhs) => (lhs - rhs).AsFloat());
		public Module Multiply(Module that) => this.Binary(that, (lhs, rhs) => (lhs * rhs).AsFloat());
		public Module Divide(Module that) => this.Binary(that, (lhs, rhs) => (lhs / rhs).AsFloat());
		public Module Modulo(Module that) => this.Binary(that, (lhs, rhs) => (lhs % rhs).AsFloat());
		public Module Power(Module that) => this.Binary(that, (lhs, rhs) => lhs.Pow(rhs).AsFloat());

		private Module Binary(Module that, Func<Node<float>, Node<float>, Node<float>> oper) {
			return new Module(
					this.Input.Concat(that.Input),
					oper(this.Output, that.Output),
					this.Parameters.Concat(that.Parameters).ToDictionary(kv => kv.Key, kv => kv.Value),
					this.NoteUsers.Concat(that.NoteUsers),
					this.gui.Concat(that.gui));
		}
	}
}
