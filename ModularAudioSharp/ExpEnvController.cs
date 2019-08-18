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
		private IEnumerable<float> ratioPerSample;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratioPerSec">1 秒ごとの減衰率</param>
		public ExpEnvController(Node ratioPerSec)
			: base(true)
		{
			this.ratioPerSample = ratioPerSec.AsFloat().UseAsStream().Select(rPS =>
					(float) Math.Pow(rPS, 1.0 / ModuleSpace.SampleRate));
		}

		protected override IEnumerable<float> Signal() {
			foreach (var r in this.ratioPerSample) {
				yield return this.amplitude;
				if (this.state == State.Idle) continue;

				this.amplitude *= r;
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
