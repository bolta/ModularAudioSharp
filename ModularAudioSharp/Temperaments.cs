using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {
	public class EqualTemperament : Temperament {
		private readonly float a4;

		public EqualTemperament(float a4 = 440) {
			this.a4 = a4;
		}

		public override float this[Tone tone] {
			get {
				var baseSemitone = TONE_NAME_TO_SEMITONE.TryGetStructValue(tone.ToneName);
				if (baseSemitone.HasValue) {
					return (float) (a4 * Math.Pow(2, tone.Octave - 4 + (baseSemitone.Value + tone.Accidental) / 12.0));

				} else {
					return 0f;
				}
			}
		}
	}
}
