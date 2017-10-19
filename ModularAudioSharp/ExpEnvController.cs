using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {
	public class ExpEnvController : NodeController<float>, INotable {
		/// <summary>
		/// これより小さくなったら自動で NoteOff する。
		/// 16 ビット量子化では 0 に等しいレベル
		/// </summary>
		private static readonly float AMPLITUDE_MIN = 1f / 65536;

		/// <summary>
		/// 現在の振幅
		/// </summary>
		private float amplitude = 0;

		/// <summary>
		/// 現在の状態
		/// </summary>
		private State state = State.Idle;

		/// <summary>
		/// <strong>サンプル</strong>ごとの減衰率。プロパティ RatioPer<strong>Sec</strong> から設定される
		/// </summary>
		private float ratioPerSample;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratioPerSec">1 秒ごとの減衰率</param>
		public ExpEnvController(float ratioPerSec)
			: base(true)
		{
			this.RatioPerSec = ratioPerSec;
		}

		protected override IEnumerable<float> Signal() {
			while (true) {
				yield return this.amplitude;

				if (this.state == State.Idle) continue;

				this.amplitude *= this.ratioPerSample;
				if (this.amplitude < AMPLITUDE_MIN) this.NoteOff();
			}
		}

		/// <summary>
		/// ノートオン。最大レベルから減衰を開始する
		/// </summary>
		public void NoteOn() {
			this.amplitude = 1;
			this.state = State.Note;
		}

		/// <summary>
		/// ノートオフ。直ちに音を止める
		/// </summary>
		public void NoteOff() {
			this.amplitude = 0;
			this.state = State.Idle;
		}

		/// <summary>
		/// 1 秒ごとの減衰率を設定する
		/// </summary>
		public float RatioPerSec {
			set { this.ratioPerSample = ToRatioPerSample(value); }
		}

		/// <summary>
		/// 減衰率を秒単位からサンプル単位に変換
		/// </summary>
		/// <param name="ratioPerSec"></param>
		/// <returns></returns>
		private static float ToRatioPerSample(float ratioPerSec)
				=> (float) Math.Pow(ratioPerSec, 1.0 / ModuleSpace.SampleRate);

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
