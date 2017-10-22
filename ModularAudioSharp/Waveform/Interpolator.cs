using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Waveform {
	public abstract class Interpolator<T> {
		protected readonly IList<T> waveform;
		protected Interpolator(IEnumerable<T> waveform) { this.waveform = waveform.ToList(); }

		public abstract T this[float offset] { get; }
	}

	/// <summary>
	/// オフセットの整数部分のみを考慮する補間器
	/// </summary>
	public class HoldInterpolator<T> : Interpolator<T> where T : struct {
		public HoldInterpolator(IEnumerable<T> waveform) : base(waveform) { }
		public override T this[float offset] =>
				this.waveform[(int) offset];
	}

	//public class HoldStereoInterpolator : Interpolator<Stereo<float>> {
	//	public HoldStereoInterpolator(IEnumerable<Stereo<float>> waveform) : base(waveform) { }
	//	public override Stereo<float> this[float offset] =>
	//			this.waveform[(int) offset];
	//}

	/// <summary>
	/// 前後のサンプルで線形補間を行う補間器
	/// </summary>
	public abstract class LinearInterpolator<T> : Interpolator<T> {
		public LinearInterpolator(IEnumerable<T> waveform) : base(waveform) { }
		protected Tuple<T, T, float> MakeInterpolationParameters(float offset) {
			var offInt = (int) offset;
			var offFrac = offset - (int) offset;
			var former = this.waveform[offInt];
			// 最後のサンプルの次は先頭のサンプルとする
			var latter = this.waveform[(offInt + 1) % this.waveform.Count];

			return Tuple.Create(former, latter, offFrac);
		}

		protected float Interpolate(float former, float latter, float offFrac)
				=> (1 - offFrac) * former + offFrac * latter;
	}

	public class LinearMonoInterpolator : LinearInterpolator<float> {
		public LinearMonoInterpolator(IEnumerable<float> waveform) : base(waveform) { }
		public override float this[float offset] {
			get {
				var ps = this.MakeInterpolationParameters(offset);
				var former = ps.Item1;
				var latter = ps.Item2;
				var offFrac = ps.Item3;
				return this.Interpolate(former, latter, offFrac);
			}
		}
	}

	public class LinearStereoInterpolator : LinearInterpolator<Stereo<float>> {
		public LinearStereoInterpolator(IEnumerable<Stereo<float>> waveform) : base(waveform) { }
		public override Stereo<float> this[float offset] {
			get {
				var ps = this.MakeInterpolationParameters(offset);
				var former = ps.Item1;
				var latter = ps.Item2;
				var offFrac = ps.Item3;
				return Stereo.Create(this.Interpolate(former.Left, latter.Left, offFrac),
						this.Interpolate(former.Right, latter.Right, offFrac));
			}
		}
	}
}
