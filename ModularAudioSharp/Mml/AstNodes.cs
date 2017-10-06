using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModularAudioSharp.Data.Util;

namespace ModularAudioSharp.Mml {
	//public class AstNode {
	//}

	public class CompilationUnit /*: AstNode*/ {
		public IEnumerable<Statement> Statements { get; set; }
		public override string ToString() => string.Join(" ", this.Statements.Select(s => s.ToString()));
	}

	public class Statement /*: AstNode*/ {

	}

	public class OctaveStatement : Statement {
		public int Value { get; set; }
		public override string ToString() => $"o{this.Value}";
	}
	public class OctaveIncrStatement : Statement { public override string ToString() => ">"; }
	public class OctaveDecrStatement : Statement { public override string ToString() => "<"; }
	public class LengthStatement : Statement {
		public int Value { get; set; }
		public override string ToString() => $"L{this.Value}";
	}
	public class ToneStatement : Statement {
		public ToneName ToneName { get; set; }
		public Length Length { get; set; } // optional
		public override string ToString() => $"{this.ToneName}{this.Length?.ToString() ?? ""}";
	}
	public class ToneName /*: AstNode*/ {
		/// <summary>
		/// 調号を含まない名前。大文字
		/// </summary>
		public string BaseName { get; set; }
		public int Accidental { get; set; }
		public override string ToString() {
			var accText = RepeatString(this.Accidental > 0 ? "+" : "-", Math.Abs(this.Accidental));
			return $"{this.BaseName}{accText}";
		}

	}
	public class Length {
		public IEnumerable<LengthElement> Elements { get; set; }
		public override string ToString() => string.Join("^", this.Elements);
	}
	public class LengthElement {
		/// <summary>音長を示す数値。省略の場合は null。音長 4. に対して 4、.. に対して null となる</summary>
		public int? Number { get; set; }

		/// <summary>付点の数</summary>
		public int Dots { get; set; }

		public override string ToString() => $"{this.Number?.ToString() ?? ""}{RepeatString(".", this.Dots)}";
	}
}
