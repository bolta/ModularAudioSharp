using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {

	/// <summary>
	/// 音律（音名から周波数への対応関係）を提供するクラス
	/// </summary>
	public abstract class Temperament {
		protected static readonly Dictionary<ToneName, int> TONE_NAME_TO_SEMITONE
				= new Dictionary<ToneName, int> {
			{ ToneName.C, -9 },
			{ ToneName.D, -7 },
			{ ToneName.E, -5 },
			{ ToneName.F, -4 },
			{ ToneName.G, -2 },
			{ ToneName.A,  0 },
			{ ToneName.B,  2 },
		};

		public abstract float this[Tone tone] { get; }

	}
}
