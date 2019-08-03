using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	// TODO Node のようにジェネリック版を作るべきか？
	class Instrument {
		public Node Output { get; private set; }
		public IDictionary<string, VarController<float>> Parameters { get; private set; }

		// TODO Node に値を設定するだけでなく他のこと（メソッドを呼ぶとか）もしたい可能性がある
		//private readonly IDictionary<string, VarController<float>> parameters;

		public IEnumerable<VarController<Tone>> ToneUsers { get; private set; }
		public IEnumerable<INotable> NoteUsers { get; private set; }

		public Instrument(Node output, IDictionary<string, VarController<float>> parameters,
				IEnumerable<VarController<Tone>> toneUsers, IEnumerable<INotable> noteUsers) {
			this.Output = output;
			this.Parameters  = new Dictionary<string, VarController<float>>(parameters);
			this.ToneUsers = toneUsers;
			this.NoteUsers = noteUsers;
		}
	}

	//abstract class Instrument<T> : Instrument where T : struct {


	//}
}
