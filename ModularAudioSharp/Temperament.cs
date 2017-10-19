using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {

	/// <summary>
	/// 音律のノードを提供する static クラス
	/// </summary>
	public static class Temperament {
		private static readonly Dictionary<ToneName, int> TONE_NAME_TO_SEMITONE
				= new Dictionary<ToneName, int> {
			{ ToneName.C, -9 },
			{ ToneName.D, -7 },
			{ ToneName.E, -5 },
			{ ToneName.F, -4 },
			{ ToneName.G, -2 },
			{ ToneName.A,  0 },
			{ ToneName.B,  2 },
		};

		public static Node<float> Equal(Node tone, float a4 = 440) {
			return new Node<float>(Equal(((Node<Tone>) tone).UseAsStream(), a4), false, tone);
		}

		private static IEnumerable<float> Equal(IEnumerable<Tone> tone, float a4) {
			return tone.Select(t => {
				var baseSemitone = TONE_NAME_TO_SEMITONE.TryGetStructValue(t.ToneName);
				if (baseSemitone.HasValue) {
					return (float) (a4 * Math.Pow(2, t.Octave - 4 + (baseSemitone.Value + t.Accidental) / 12.0));

				} else {
					return 0f;
				}
			});
		}
	}
}
