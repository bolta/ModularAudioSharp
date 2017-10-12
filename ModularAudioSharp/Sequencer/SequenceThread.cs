using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Sequencer;

namespace ModularAudioSharp.Sequencer {
	public class SequenceThread {

		private readonly IList<Instruction> instructions;

		/// <summary>
		/// これから実行するインストラクションの添字。
		/// ジャンプなどを自由に行えるよう、 IEnumerator ではなく単なる添字で保持する
		/// </summary>
		internal int Pointer { get; set; } = 0;

		public SequenceThread(IList<Instruction> instructions) {
			this.instructions = instructions;
		}

		///// <summary>
		///// 現在の出力値
		///// </summary>
		//internal T CurrentValue { get; set; } = default(T);

		///// <summary>
		///// 有効値を設定すると、その値を直後の 1 サンプルのみ、CurrentValue に優先して使う。
		///// 次のサンプルからは CurrentValue を出力する。
		///// Tick 単位ではなく 1 サンプルだけ出力したい値（NoteOn/Off など）があるときに使う
		///// </summary>
		//internal T? ValueOnce { get; set; } = null;

		/// <summary>
		/// 待機中の場合、正の値（tick 単位）。
		/// 0 のときは直ちに次のインストラクションを実行できる
		/// </summary>
		internal int Wait { get; set; } = 0;

		/// <summary>
		/// Sequencer の入力である tick が true になったときに呼び出される。スレッドごとの動作を行い、プログラムカウンタを進める
		/// </summary>
		/// <returns>
		/// このスレッドにまだ続きがあるかどうか
		/// </returns>
		internal void Tick() {
			if (this.Wait > 0) {
				-- this.Wait;
				if (this.Wait > 0) return;
			}

			// ウェイトを挟まずに並んでいるインストラクションは全て実行する
			while (this.Wait == 0 && this.Pointer < this.instructions.Count) {
				this.instructions[this.Pointer].Execute(this);
				++ this.Pointer;
			}
		}
	}

}
