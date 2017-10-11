using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Waveform {
	public abstract class Interpolator {
		protected readonly IList<float> waveform;
		protected Interpolator(IEnumerable<float> waveform) { this.waveform = waveform.ToList(); }

		public abstract float this[float offset] { get; }
	}

	/// <summary>
	/// オフセットの整数部分のみを考慮する補間器
	/// </summary>
	public class HoldInterpolator : Interpolator {
		public HoldInterpolator(IEnumerable<float> waveform) : base(waveform) { }
		public override float this[float offset] =>
				this.waveform[(int) offset];
	}

	/// <summary>
	/// 前後のサンプルで線形補間を行う補間器
	/// </summary>
	public class LinearInterpolator : Interpolator {
		public LinearInterpolator(IEnumerable<float> waveform) : base(waveform) { }
		public override float this[float offset] {
			get {
				var offInt = (int) offset;
				var offFrac = offset - (int) offset;
				var left = this.waveform[offInt];
				// 最後のサンプルの次は先頭のサンプルとする
				var right = this.waveform[(offInt + 1) % this.waveform.Count];

				return (1 - offFrac) * left + offFrac * right;
			}
		}
	}
}
