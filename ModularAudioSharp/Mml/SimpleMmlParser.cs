using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;

namespace ModularAudioSharp.Mml {
	public class SimpleMmlParser {
		/// <summary>
		/// 整数値。正負の符号をつけることができる
		/// 例：42, +42, -42
		/// </summary>
		public readonly static Parser<int> integer =
			SParse.Regex(@"[+-]?[0-9]+").Text().Select(Convert.ToInt32);

		/// <summary>
		/// 整数値。正負の符号をつけることができる
		/// 例：42, +42, -42
		/// </summary>
		public readonly static Parser<uint> unsignedInteger =
			SParse.Regex(@"[0-9]+").Text().Select(Convert.ToUInt32);

		public readonly static Parser<float> real =
			SParse.Regex(@"[+-]?[0-9]+(\.[0-9]+)?|[+-]?\.[0-9]+").Text().Select(Convert.ToSingle);

		/// <summary>
		/// 音長の要素。
		/// タイを使わず、数値（省略可能）と、0 個以上の付点を続けたもの
		/// 例：（空）, 16, 4..., .
		/// </summary>
		public readonly static Parser<LengthElement> lengthElement =
			from n in integer.Optional().Token()
			from ds in SParse.String(".").Many().Token()
			select new LengthElement { Number = ToNullable(n), Dots = ds.Count() };

		/// <summary>
		/// 音長。1 個以上の LengthElement を ^ で連結したもの。
		/// 例：（空）, 16, 16^4..., ^^^, ^4, 4^
		/// </summary>
		public readonly static Parser<Length> length
				= from l in lengthElement
				from ls in (
					from _ in SParse.String("^")
					from l1 in lengthElement
					select l1
				).Many()
				select new Length { Elements = new[] { l }.Concat(ls) };

		/// <summary>
		/// 変化記号の付かない音名
		/// 例（全て）：c, d, e, f, g, a, b
		/// 例でない：r
		/// TODO 大文字を許容する
		/// </summary>
		public readonly static Parser<string> baseName = SParse.Chars("cdefgab").Select(c => c.ToString());

		/// <summary>
		/// 変化記号。
		/// +（シャープ）または -（フラット）のいずれか一方を 0 個以上続けたもの
		/// 例：（空）, +, ---
		/// 例でない：#, +-
		/// </summary>
		public readonly static Parser<int> accidental = SParse.Char('+').Many().Select(ps => ps.Count())
		                                                  .Or(SParse.Char('-').Many().Select(ps => -ps.Count()));

		/// <summary>
		/// 音名に変化記号をつけたもの
		/// 例：c, e-, g+++
		/// </summary>
		public readonly static Parser<ToneName> toneName = from b in baseName.Token()
															 from a in accidental.Token()
															 select new ToneName { BaseName = b, Accidental = a };

		public readonly static Parser<Identifier> identifier = (from _ in SParse.Char('`')
																from id in SParse.Regex(@"[a-zA-Z0-9_.-]+")
																from __ in SParse.Char('`')
																select new Identifier { Name = id })
														.Or(
																from id in SParse.Regex(@"[0-9]+")
																select new Identifier { Name = int.Parse(id).ToString() });

		/// <summary>
		/// オクターブ指定
		/// 例：o4
		/// </summary>
		public readonly static Parser<OctaveCommand> octaveCommand = from _ in SParse.String("o").Text().Token()
																		   from n in integer.Token()
																		   select new OctaveCommand { Value = n };
		/// <summary>
		/// オクターブ一つ上げる
		/// 唯一の例：&gt;
		/// </summary>
		public readonly static Parser<OctaveIncrCommand> octaveIncrCommand = from _ in SParse.String(">").Token()
																				   select new OctaveIncrCommand();
		/// <summary>
		/// オクターブ一つ下げる
		/// 唯一の例：&lt;
		/// </summary>
		public readonly static Parser<OctaveDecrCommand> octaveDecrCommand = from _ in SParse.String("<").Token()
																				   select new OctaveDecrCommand();
		/// <summary>
		/// デフォルト音長指定。
		/// 音長は数値で指定するため付点をつけることはできない。
		/// 一方、負の値を指定することができる（これは禁止すべきかも）
		/// L は大文字でなければならない（これも、小文字を許してよいかも）
		/// 例：L8
		/// 例でない：L4.
		/// </summary>
		public readonly static Parser<LengthCommand> lengthCommand = from _ in SParse.String("L").Text().Token()
																		   from l in integer.Token()
																		   select new LengthCommand { Value = l };

		public readonly static Parser<VolumeCommand> volumeCommand = from _ in SParse.String("v").Token()
																		   from v in real.Token()
																		   select new VolumeCommand { Value = v };
		/// <summary>
		/// 音高・音長を指定して発音する。
		/// 音長は省略できる（音長の定義が空の音長を許容するため）
		/// 例：c, d+4, e--^.
		/// </summary>
		public readonly static Parser<ToneCommand> toneCommand = from t in toneName
																	   from l in length
																	   select new ToneCommand { ToneName = t, Length = l };

		public readonly static Parser<RestCommand> restCommand
				= from _ in SParse.String("r").Text().Token()
				  from l in length
				  select new RestCommand { Length = l };

		public readonly static Parser<ParameterCommand> parameterCommand
				= from _ in SParse.String("y").Text().Token()
				  from name in identifier
				  from __ in SParse.String(",").Text().Token()
				  from value in real.Token()
				  select new ParameterCommand { Name = name, Value = value };

		public readonly static Parser<LoopCommand> loopCommand
				= from _ in SParse.String("[").Text().Token()
				  from t in unsignedInteger.Token().Optional()
				  from c in command.Token().Many()
				  from __ in SParse.String("]")
				  let tn = ToNullable(t) ?? 2
				  select new LoopCommand { Times = tn == 0 ? null : (uint?) tn, Content = c };

		/// <summary>
		/// 任意の文
		/// </summary>
		public readonly static Parser<Command> command =
				((Parser<Command>) octaveCommand)
				.Or(octaveIncrCommand)
				.Or(octaveDecrCommand)
				.Or(lengthCommand)
				.Or(volumeCommand)
				.Or(toneCommand)
				.Or(restCommand)
				.Or(parameterCommand)
				.Or(loopCommand)
				;

		/// <summary>
		/// MML 全体。文を任意個並べたもの
		/// </summary>
		public readonly static Parser<CompilationUnit> compilationUnit = from ss in command.Many().Token()
																		   select new CompilationUnit { Commands = ss };

		public CompilationUnit Parse(string mml) => compilationUnit.Parse(mml);

		private static T? ToNullable<T>(IOption<T> opt) where T : struct =>
			opt.IsDefined ? opt.Get() : (T?) null;
	}
}
