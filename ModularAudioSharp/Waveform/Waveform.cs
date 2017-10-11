using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Waveform {
	/// <summary>
	/// 波形を表すクラス。
	/// 振幅は ±1 を基準とするが、そこから外れた値でも収容できる。
	/// 補完はここでは扱わない
	/// </summary>
	public class Waveform {

		private readonly float[][] content;

		/// <summary>
		/// この波形のサンプルレート
		/// </summary>
		public int SampleRate { get; private set; }

		public Waveform(IEnumerable<IEnumerable<float>> content, int sampleRate) {
			if (content.Select(ch => ch.Count()).Distinct().Count() != 1) {
				throw new ArgumentException("Waveform lengths of all channels are not consistent");
			}

			this.content = content.Select(Enumerable.ToArray).ToArray();
			this.SampleRate = sampleRate;
		}

		public Waveform(IEnumerable<float> content, int sampleRate) : this(new [] { content }, sampleRate) { }

		public int Channels => this.content.Length;

		public int Length_smp => this.content[0].Length;

		//public float this[int offset] => this[0, offset];

		public float this[int channel, int offset] {
			get {
				// TODO 範囲チェック
				return this.content[channel][offset];
			}
		}

		public IList<float> Samples(int channel) => this.content[channel];

		// TODO 編集を許可するか？　たとえばトリミングとか、最大化とか
		// 波形メモリ音源で、音色を動的に変化させながら発音したい場合を考えると編集は必要
	}
}
