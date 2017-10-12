using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		/// <summary>
		/// 待機中の場合、正の値（tick 単位）。
		/// 0 のときは直ちに次のインストラクションを実行できる
		/// </summary>
		internal int Wait { get; set; } = 0;

		/// <summary>
		/// Sequencer.Tick() を契機として呼び出される。スレッドごとの動作を行い、インストラクションポインタを進める
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
