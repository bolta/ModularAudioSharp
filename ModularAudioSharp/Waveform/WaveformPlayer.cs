using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Waveform {

	/// <summary>
	/// Waveform を再生するノード
	/// </summary>
	public static class WaveformPlayer {
		// TODO クラスを設ける必要性はない。提供形態を検討
		public static Node<float> New(Waveform waveform, float masterFreq, Node<NoteOperation> notes, Node<float> freq,
				int? startOffset = null, int? endOffset = null, int? loopOffset = null)
				=> new Node<float>(Signal(waveform, masterFreq, notes.UseAsStream(), freq.UseAsStream(),
						startOffset, endOffset, loopOffset));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="waveform"></param>
		/// <param name="masterFreq">waveform に収録された音の本来の周波数</param>
		/// <param name="notes"></param>
		/// <param name="freq"></param>
		/// <param name="startOffset"></param>
		/// <param name="endOffset"></param>
		/// <param name="loopOffset"></param>
		/// <returns></returns>
		private static IEnumerable<float> Signal(Waveform waveform, float masterFreq, IEnumerable<NoteOperation> notes, IEnumerable<float> freq,
				int? startOffset = null, int? endOffset = null, int? loopOffset = null) {
			var startOffset_ = startOffset ?? 0;
			var endOffset_ = endOffset ?? waveform.Length_smp;
			// TODO ステレオ
			var interp = new LinearInterpolator(waveform.Samples(0));

			var state = State.Idle;
			var offset = 0f;
			foreach (var nf in notes.Zip(freq, Tuple.Create)) {
				if (nf.Item1 == NoteOperation.NoteOn) {
					offset = startOffset_;
					state = State.Note;
				} else if (nf.Item1 == NoteOperation.NoteOff) {
					state = State.Idle;
				}

				if (state == State.Idle) {
					yield return 0f;
				} else if (state == State.Note) {
					yield return interp[offset];

					// オフセットの進む速さはサンプリングレートと音高の分だけ速くなる
					offset += waveform.SampleRate * nf.Item2 / ModuleSpace.SampleRate / masterFreq;
					if (offset >= endOffset_ && loopOffset.HasValue) {
						// ループ時は誤差を足してやらないとピッチがずれる
						offset = loopOffset.Value + (offset - endOffset_);
					}
					if (offset >= waveform.Length_smp) {
						state = State.Idle;
					}
				}

			}
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
