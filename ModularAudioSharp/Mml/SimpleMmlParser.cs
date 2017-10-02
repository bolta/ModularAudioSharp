using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;

namespace ModularAudioSharp.Mml {
	public class SimpleMmlParser {
		public readonly static Parser<int> number = SParse.Digit.AtLeastOnce().Text().Select(Convert.ToInt32);
		public readonly static Parser<LengthElement> lengthElement =
			from n in number.Optional().Token()
			from ds in SParse.String(".").Many().Token()
			select new LengthElement { Number = ToNullable(n), Dots = ds.Count() };
		public readonly static Parser<Length> length = from l in lengthElement
														 from ls in (
															 from _ in SParse.String("^")
															 from l1 in lengthElement
															 select l1).Many()
														 select new Length { Elements = new[] { l }.Concat(ls) };

		public readonly static Parser<string> baseName = SParse.Chars("cdefgab").Select(c => c.ToString());
		public readonly static Parser<int> accidental = SParse.Char('+').Many().Select(ps => ps.Count())
		                                                  .Or(SParse.Char('-').Many().Select(ps => -ps.Count()));

		public readonly static Parser<ToneName> toneName = from b in baseName.Token()
															 from a in accidental.Token()
															 select new ToneName { BaseName = b, Accidental = a };

		public readonly static Parser<OctaveStatement> octaveStatement = from _ in SParse.String("o").Text().Token()
																		   from n in number.Token()
																		   select new OctaveStatement { Value = n };
		public readonly static Parser<OctaveIncrStatement> octaveIncrStatement = from _ in SParse.String(">").Token()
																				   select new OctaveIncrStatement();
		public readonly static Parser<OctaveDecrStatement> octaveDecrStatement = from _ in SParse.String("<").Token()
																				   select new OctaveDecrStatement();
		public readonly static Parser<LengthStatement> lengthStatement = from _ in SParse.String("L").Text().Token()
																		   from l in number.Token()
																		   select new LengthStatement { Value = l };
		public readonly static Parser<ToneStatement> toneStatement = from t in toneName
																	   from l in length
																	   select new ToneStatement { ToneName = t, Length = l };
		public readonly static Parser<Statement> statement =
				((Parser<Statement>) octaveStatement)
				.Or(octaveIncrStatement)
				.Or(octaveDecrStatement)
				.Or(lengthStatement)
				.Or(toneStatement);

		public readonly static Parser<CompilationUnit> compilationUnit = from ss in statement.Many().Token()
																		   select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string mml) => compilationUnit.Parse(mml);

		private static T? ToNullable<T>(IOption<T> opt) where T : struct =>
			opt.IsDefined ? opt.Get() : (T?) null;
	}
}
