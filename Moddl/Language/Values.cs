using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Value {

		// TODO エラー処理をちゃんとする。暗黙の型変換もあればここで

		public float? AsFloat() {
			return ((FloatValue) this)?.Value;
		}

		// nullable
		public IEnumerable<string> AsTrackSet() {
			return ((TrackSetValue) this)?.Value;
		}

		// nullable
		public Module AsModule() {
			if (this is ModuleValue) {
				return ((ModuleValue) this).Value;
			} else if (this is FloatValue) {
				return Module.FromFloat(((FloatValue) this).Value);
			} else {
				return null;
			}
		}
	}

	class FloatValue : Value {
		public float Value { get; set; }
	}

	class TrackSetValue : Value {
		public IEnumerable<string> Value { get; set; }
	}

	class ModuleValue : Value {
		public Module Value { get; set; }
	}
}
