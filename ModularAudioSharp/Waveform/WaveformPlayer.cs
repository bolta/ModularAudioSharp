using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Waveform {

	public class WaveformPlayer {
		public static WaveformPlayer<T> Create<T>(Waveform<T> waveform, float masterFreq, Node<float> freq,
				int startOffset = 0, int? endOffset = null, int? loopOffset = null) where T : struct
				=> new WaveformPlayer<T>(waveform, masterFreq, freq, startOffset, endOffset, loopOffset);
	}

	/// <summary>
	/// Waveform を再生するノード
	/// TODO 名前は WaveformPlayerController とすべきか？　Node と NodeController の区別は必ずしも必要ないかも
	/// </summary>
	public class WaveformPlayer<T> : NodeController<T>, INotable where T : struct {
		private readonly Waveform<T> waveform;
		private readonly float masterFreq;
		private readonly IEnumerable<float> freq;
		private int startOffset;
		private int endOffset;
		private int? loopOffset;
		private float offset;
		private State state = State.Idle;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="waveform"></param>
		/// <param name="masterFreq"></param>
		/// <param name="freq"></param>
		/// <param name="startOffset"></param>
		/// <param name="endOffset"></param>
		/// <param name="loopOffset"></param>
		public WaveformPlayer(Waveform<T> waveform, float masterFreq, Node<float> freq,
				int startOffset = 0, int? endOffset = null, int? loopOffset = null) : base(true) {
			this.waveform = waveform;
			this.masterFreq = masterFreq;
			this.freq = freq.UseAsStream();
			this.offset = this.startOffset = startOffset;
			this.endOffset = endOffset ?? waveform.Length_smp;
			this.loopOffset = loopOffset;
		}

		///// <summary>
		///// 
		///// </summary>
		///// <param name="waveform"></param>
		///// <param name="masterFreq">waveform に収録された音の本来の周波数</param>
		///// <param name="notes"></param>
		///// <param name="freq"></param>
		///// <param name="startOffset"></param>
		///// <param name="endOffset"></param>
		///// <param name="loopOffset"></param>
		///// <returns></returns>
		protected override IEnumerable<T> Signal() {
			var interp = Util.OnChannelCountLazy(default(T),
					() => new LinearMonoInterpolator((this as WaveformPlayer<float>).waveform.Samples) as Interpolator<T>,
					() => new LinearStereoInterpolator((this as WaveformPlayer<Stereo<float>>).waveform.Samples) as Interpolator<T>);

			foreach (var f in this.freq) {
				if (this.state == State.Idle) {
					yield return default(T);
				} else if (this.state == State.Note) {
					yield return interp[this.offset];

					this.offset += this.waveform.SampleRate * f / ModuleSpace.SampleRate / this.masterFreq;
					if (this.offset >= this.endOffset && this.loopOffset.HasValue) {
						// ループ時は誤差を足してやらないとピッチがずれる
						this.offset = this.loopOffset.Value + (this.offset - this.endOffset);
					}
					if (this.offset >= this.waveform.Length_smp) {
						NoteOff();
					}
				} else Debug.Assert(false);

			}
		}

		public void NoteOn() {
			this.offset = this.startOffset;
			this.state = State.Note;
		}

		public void NoteOff() {
			this.state = State.Idle;
		}

		private enum State {
			/// <summary>
			/// 発音していない
			/// </summary>
			Idle = 0,

			/// <summary>
			/// 発音中
			/// </summary>
			Note = 1,
		}

	}
}
