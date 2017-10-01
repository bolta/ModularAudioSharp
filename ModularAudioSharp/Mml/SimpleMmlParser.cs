using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;

namespace ModularAudioSharp.Mml {
	public class SimpleMmlParser {
		internal readonly static Parser<int> number = SParse.Decimal.Select(Convert.ToInt32);
		internal readonly static Parser<LengthElement> lengthElement = from n in number.Optional().Token()
																	   from ds in SParse.String(".").Many().Token()
																	   select new LengthElement { Number = ToNullable(n), Dots = ds.Count() };
		internal readonly static Parser<Length> length = from l in lengthElement
														 from ls in (
															 from _ in SParse.String("^")
															 from l1 in lengthElement
															 select l1).Many()
														 select new Length { Elements = new[] { l }.Concat(ls) };

		internal readonly static Parser<string> baseName = SParse.Chars("cdefgab").Select(c => c.ToString());
		internal readonly static Parser<int> accidental = SParse.Char('+').Many().Select(ps => ps.Count())
		                                                  .Or(SParse.Char('-').Many().Select(ps => -ps.Count()));

		internal readonly static Parser<ToneName> toneName = from b in baseName.Token()
															 from a in accidental.Token()
															 select new ToneName { BaseName = b, Accidental = a };

		internal readonly static Parser<OctaveStatement> octaveStatement = from _ in SParse.String("o").Text().Token()
																		   from n in number.Token()
																		   select new OctaveStatement { Value = n };
		internal readonly static Parser<OctaveIncrStatement> octaveIncrStatement = from _ in SParse.String(">").Token()
																				   select new OctaveIncrStatement();
		internal readonly static Parser<OctaveDecrStatement> octaveDecrStatement = from _ in SParse.String("<").Token()
																				   select new OctaveDecrStatement();
		internal readonly static Parser<LengthStatement> lengthStatement = from _ in SParse.String("L").Text().Token()
																		   from l in number.Token()
																		   select new LengthStatement { Value = l };
		internal readonly static Parser<ToneStatement> toneStatement = from t in toneName
																	   from l in length
																	   select new ToneStatement { ToneName = t, Length = l };
		internal readonly static Parser<Statement> statement =
				((Parser<Statement>) octaveStatement)
				.Or(octaveIncrStatement)
				.Or(octaveDecrStatement)
				.Or(lengthStatement)
				.Or(toneStatement);

		internal readonly static Parser<CompilationUnit> compilationUnit = from ss in statement.Many().Token()
																		   select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string mml) => compilationUnit.Parse(mml);

		private static T? ToNullable<T>(IOption<T> opt) where T : struct =>
			opt.IsDefined ? opt.Get() : (T?) null;
	}
}
