using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Waveform {

	public abstract class Waveform {
		/// <summary>
		/// この波形のサンプルレート
		/// </summary>
		public int SampleRate { get; private set; }

		public abstract int Channels { get; }

		public abstract int Length_smp { get; }

		protected Waveform(int sampleRate) {
			this.SampleRate = sampleRate;
		}
	}

	/// <summary>
	/// 波形を表すクラス。
	/// 振幅は ±1 を基準とするが、そこから外れた値でも収容できる。
	/// 補完はここでは扱わない
	/// </summary>
	public class Waveform<T> : Waveform where T : struct {

		//private readonly float[][] content;
		private readonly List<T> content;

		public Waveform(IEnumerable<T> content, int sampleRate) : base(sampleRate) {
			this.content = content.ToList();
		}

		public override int Channels => Util.OnChannelCount(default(T), 1, 2);

		public override int Length_smp => this.content.Count;

		//public float this[int offset] => this[0, offset];

		public T this[int channel, int offset] {
			get {
				// TODO 範囲チェック
				return this.content[offset];
			}
		}

		public IList<T> Samples => this.content;

		// TODO 編集を許可するか？　たとえばトリミングとか、最大化とか
		// 波形メモリ音源で、音色を動的に変化させながら発音したい場合を考えると編集は必要
	}
}
