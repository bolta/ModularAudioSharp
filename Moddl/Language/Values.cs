using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Value {

		internal virtual float? TryAsFloat() => null;
		internal float AsFloat() => this.TryAsFloat() ?? throw new ModdlTypeException();

		internal virtual IEnumerable<string> TryAsTrackSet() => null;
		internal IEnumerable<string> AsTrackSet() => this.TryAsTrackSet() ?? throw new ModdlTypeException();

		internal virtual IDictionary<string, Value> TryAsAssocArray() => null;
		internal IDictionary<string, Value> AsAssocArray() => this.TryAsAssocArray() ?? throw new ModdlTypeException();

		internal virtual Module TryAsModule() => null;
		internal Module AsModule() => this.TryAsModule() ?? throw new ModdlTypeException();

		internal virtual Func<Module> TryAsModuleDef() => null;
		internal Func<Module> AsModuleDef() => this.TryAsModuleDef() ?? throw new ModdlTypeException();

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

	class AssocArrayValue : Value {
		public IDictionary<string, Value> Entries { get; set; }
		internal override IDictionary<string, Value> TryAsAssocArray() => this.Entries;
	}

	class ModuleValue : Value {
		public Module Value { get; set; }
		internal override Module TryAsModule() => this.Value;
	}

	class ModuleDefValue : Value {
		public Func<Module> Value { get; set; }
		internal override Func<Module> TryAsModuleDef() => this.Value;
		internal override Module TryAsModule() => this.Value();
	}
}
