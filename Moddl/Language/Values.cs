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

		internal virtual string TryAsIdentifier() => null;
		internal string AsIdentifier() => this.TryAsIdentifier() ?? throw new ModdlTypeException();

		internal virtual string TryAsMml() => null;
		internal string AsMml() => this.TryAsMml() ?? throw new ModdlTypeException();

		internal virtual IDictionary<string, Value> TryAsAssocArray() => null;
		internal IDictionary<string, Value> AsAssocArray() => this.TryAsAssocArray() ?? throw new ModdlTypeException();

		internal virtual Module TryAsModule() => null;
		internal Module AsModule() => this.TryAsModule() ?? throw new ModdlTypeException();

		internal virtual ModuleConstructor TryAsModuleConstructor() => null;
		internal ModuleConstructor AsModuleConstructor() => this.TryAsModuleConstructor() ?? throw new ModdlTypeException();

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

	class IdentifierValue : Value {
		public string Value { get; set; }
		internal override string TryAsIdentifier() => this.Value;
	}

	class MmlValue : Value {
		public string Value { get; set; }
		internal override string TryAsMml() => this.Value;
	}

	class AssocArrayValue : Value {
		public IDictionary<string, Value> Entries { get; set; }
		internal override IDictionary<string, Value> TryAsAssocArray() => this.Entries;
	}

	class ModuleValue : Value {
		public Module Value { get; set; }
		internal override Module TryAsModule() => this.Value;
	}

	class ModuleConstructorValue : Value {
		public ModuleConstructor Constructor { get; set; }
		internal override ModuleConstructor TryAsModuleConstructor() => this.Constructor;
		internal override Module TryAsModule() => this.Constructor.CreateModule();
	}
}
