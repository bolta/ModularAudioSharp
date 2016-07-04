using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModularAudioSharp {
	/// <summary>
	/// ライブラリのユーザを NAudio に依存させないためのクラス。
	/// NAudio.Wave.WaveOut をラップしているだけ
	/// </summary>
	public class Player : IDisposable {
		private WaveOut waveOut;

		internal Player(WaveOut waveOut) { this.waveOut = waveOut; }

		public void Dispose() {
			this.waveOut.Dispose();
		}
	}
}
