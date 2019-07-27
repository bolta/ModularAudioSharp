using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;

namespace Moddl.Language {
	public class Parser {

		public readonly static Parser<MmlStatement> mmlStatement =
				from parts in SParse.Chars("abc").AtLeastOnce() // TODO case insensitive で任意の英字を許す
				from _ in SParse.WhiteSpace.AtLeastOnce()
				from mml in SParse.Regex(@"[^\r\n]*")
				from __ in SParse.LineEnd // TODO LineTerminator とはどう違う？
				select new MmlStatement {
					Parts = parts.Select(c => c.ToString()),
					Mml = mml,
				};

		/// <summary>
		/// 任意の文
		/// </summary>
		public readonly static Parser<Statement> statement =
				((Parser<Statement>) mmlStatement);


		/// <summary>
		/// 変換単位全体。文を任意個並べたもの
		/// </summary>
		public readonly static Parser<CompilationUnit> compilationUnit = from ss in statement.Many().Token()
																		   select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string moddl) => compilationUnit.Parse(moddl + "\n");
	}
}
