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

		public Module AsModule() {
			return ((ModuleValue) this)?.Value;
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
