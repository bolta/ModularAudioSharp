using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Mml {
	//public class AstNode {
	//}

	public class CompilationUnit /*: AstNode*/ {
		internal IEnumerable<Statement> Statements { get; set; }
	}

	public class Statement /*: AstNode*/ {

	}

	public class OctaveStatement : Statement { internal int Value { get; set; } }
	public class LengthStatement : Statement { internal Length Value { get; set; } }
	public class OctaveIncrStatement : Statement { }
	public class OctaveDecrStatement : Statement { }
	public class ToneStatement : Statement {
		internal ToneName ToneName { get; set; }
		internal Length Length { get; set; } // optional
	}
	public class ToneName /*: AstNode*/ {
		/// <summary>
		/// 調号を含まない名前。大文字
		/// </summary>
		internal string BaseName { get; set; }

		internal int Accidental { get; set; }
	}
	public class Length { internal IEnumerable<LengthElement> Elements { get; set; } }
	public class LengthElement {
		/// <summary>音長を示す数値。省略の場合は null。音長 4. に対して 4、.. に対して null となる</summary>
		internal int? Number { get; set; }

		/// <summary>付点の数</summary>
		internal int Dots { get; set; }
	}
}
