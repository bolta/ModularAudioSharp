using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
using System.Diagnostics;

namespace ModularAudioSharp {
	/// <summary>
	/// ADSR エンベロープを提供する NodeController。
	/// 出力値は 0 から 1 で、線形なので振幅として使う場合は指数変換が必要
	/// ↑…と思ったが、指数変換をかますとかえって変になる。なしでよさそうだが、理由不明
	/// </summary>
	public class AdsrEnvController : NodeController<float>, INotable {

		/// <summary>
		/// 現在の音量
		/// </summary>
		private float volume = 0;

		/// <summary>
		/// 現在の状態
		/// </summary>
		private State state = State.Idle;

		private IEnumerable<float> attackRatePerSample;
		private IEnumerable<float> decayRatePerSample;
		private IEnumerable<float> sustainLevelVol;
		private IEnumerable<float> releaseRatePerSample;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratioPerSec">1 秒ごとの減衰率</param>
		public AdsrEnvController(Node attackTimeSecs, Node decayTimeSecs, Node sustainLevelVol, Node releaseTimeSecs)
			: base(true)
		{
			// サンプルごとの変化量に変換
			Func<float, float> toRatePerSample = secs => {
				if (secs <= 0f) {
					return 1f;
				} else {
					// rate[smp] = 1 / time[smp] = 1 / (smpRate[smp/s] * time[s]) 
					return 1f / ModuleSpace.SampleRate / secs;
				}
			};
			this.attackRatePerSample = attackTimeSecs.AsFloat().UseAsStream().Select(toRatePerSample);
			this.decayRatePerSample = decayTimeSecs.AsFloat().UseAsStream().Select(toRatePerSample);
			this.sustainLevelVol = sustainLevelVol.AsFloat().UseAsStream();
			this.releaseRatePerSample = releaseTimeSecs.AsFloat().UseAsStream().Select(toRatePerSample);
		}

		protected override IEnumerable<float> Signal() {
			var adsrStream = this.attackRatePerSample.Zip(
					this.decayRatePerSample,
					this.sustainLevelVol,
					this.releaseRatePerSample,
					(a, d, s, r) => new { a, d, s, r }); 

			foreach (var adsr in adsrStream) {
				yield return this.volume;

				if (this.state == State.Idle) {
					// nop; wait for NoteOn

				} else if (this.state == State.Attack) {
					this.volume += adsr.a;
					if (this.volume >= 1f) {
						this.volume = 1f;
						this.state = State.Decay;
					}

				} else if (this.state == State.Decay) {
					this.volume -= adsr.d;
					if (this.volume <= adsr.s) {
						this.volume = adsr.s;
						this.state = State.Sustain;
					}

				} else if (this.state == State.Sustain) {
					// nop; wait for NoteOff

				} else if (this.state == State.Release) {
					this.volume -= adsr.r;
					if (this.volume <= 0f) {
						this.volume = 0f;
						this.state = State.Idle;
					}

				} else {
					Debug.Assert(false);
				}
			}
		}

		/// <summary>
		/// ノートオン。音量 0 からアタック
		/// </summary>
		public void NoteOn() {
			this.volume = 0;
			this.state = State.Attack;
		}

		/// <summary>
		/// ノートオフ。リリースへ移行
		/// </summary>
		public void NoteOff() {
			this.state = State.Release;
		}

		private enum State {
			/// <summary>
			/// 発音していない
			/// </summary>
			Idle = 0,

			/// <summary>
			/// アタック中
			/// </summary>
			Attack = 1,

			/// <summary>
			/// ディケイ中
			/// </summary>
			Decay = 2,

			/// <summary>
			/// サステイン中
			/// </summary>
			Sustain = 3,

			/// <summary>
			/// リリース中
			/// </summary>
			Release = 4,
		}
	}
}
