using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Mml {
	//public class AstNode {
	//}

	public class CompilationUnit /*: AstNode*/ {
		public IEnumerable<Statement> Statements { get; set; }
	}

	public class Statement /*: AstNode*/ {

	}

	public class OctaveStatement : Statement { public int Value { get; set; } }
	public class LengthStatement : Statement { public int Value { get; set; } }
	public class OctaveIncrStatement : Statement { }
	public class OctaveDecrStatement : Statement { }
	public class ToneStatement : Statement {
		public ToneName ToneName { get; set; }
		public Length Length { get; set; } // optional
	}
	public class ToneName /*: AstNode*/ {
		/// <summary>
		/// 調号を含まない名前。大文字
		/// </summary>
		public string BaseName { get; set; }

		public int Accidental { get; set; }
	}
	public class Length { public IEnumerable<LengthElement> Elements { get; set; } }
	public class LengthElement {
		/// <summary>音長を示す数値。省略の場合は null。音長 4. に対して 4、.. に対して null となる</summary>
		public int? Number { get; set; }

		/// <summary>付点の数</summary>
		public int Dots { get; set; }
	}
}
