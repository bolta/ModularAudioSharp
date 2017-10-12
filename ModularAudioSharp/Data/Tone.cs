using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	public struct Tone {
		/// <summary>
		/// オクターブ。440 Hz がオクターブ 4 の A
		/// </summary>
		public int Octave { get; set; }

		/// <summary>
		/// 音名（変化記号なし）。
		/// 変化記号を別に扱うのは、C♯ と D♭ の高さが違うような音律にも対応できるように
		/// </summary>
		public ToneName ToneName { get; set; }

		/// <summary>
		/// 変化記号。♯ は 1、♭ は -1、ダブルシャープは 2 など。
		/// ただし微分音が必要な場合などはこれ以外の意味で使ってもよい
		/// （♯ の半分を 1 とするとか）
		/// </summary>
		public int Accidental { get; set; }

		public override string ToString() => $"o{this.Octave}{this.ToneName}{Data.Util.RepeatString(this.Accidental > 0 ? "+" : "-", Math.Abs(this.Accidental))}";
	}
}
