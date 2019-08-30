using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Value {

		internal virtual float? TryAsFloat() => null;
		internal float AsFloat() => this.TryAsFloat() ?? this.ThrowTypeError<float>();

		internal virtual IEnumerable<string> TryAsTrackSet() => null;
		internal IEnumerable<string> AsTrackSet() => this.TryAsTrackSet() ?? this.ThrowTypeError<IEnumerable<string>>();

		internal virtual Module TryAsModule() => null;
		internal Module AsModule() => this.TryAsModule() ?? this.ThrowTypeError<Module>();

		private T ThrowTypeError<T>() {
			throw new ModdlTypeException();
		}
	}

	class FloatValue : Value {
		public float Value { get; set; }
		internal override float? TryAsFloat() => this.Value;
		internal override Module TryAsModule() => Module.FromFloat(this.Value);
	}

	class TrackSetValue : Value {
		public IEnumerable<string> Value { get; set; }
		internal override IEnumerable<string> TryAsTrackSet() => this.Value;
	}

	class ModuleValue : Value {
		public Module Value { get; set; }
		internal override Module TryAsModule() => this.Value;
	}
}
