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
		public IDictionary<string, Node> Parameters { get; private set; }
		public IEnumerable<VarController<Tone>> ToneUsers { get; private set; }
		public IEnumerable<INotable> NoteUsers { get; private set; }

		public Instrument(Node output, IDictionary<string, Node> parameters,
				IEnumerable<VarController<Tone>> toneUsers, IEnumerable<INotable> noteUsers) {
			this.Output = output;
			this.Parameters = parameters;
			this.ToneUsers = toneUsers;
			this.NoteUsers = noteUsers;
		} 
	}

	//abstract class Instrument<T> : Instrument where T : struct {


	//}
}
