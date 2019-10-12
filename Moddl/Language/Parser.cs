using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;
using System.Text.RegularExpressions;

namespace Moddl.Language {
	public class Parser {

		private static Parser<DirectiveStatement> DirectiveStatement =>
				from _ in SParse.String("@").WithWhiteSpace()
				from name in SParse.Regex("[a-zA-Z0-9_]+").WithWhiteSpace()
				from args in ExprList.Optional()
				from __ in SParse.LineEnd // TODO LineTerminator とはどう違う？
				select new DirectiveStatement {
					Name = name,
					Arguments = args.GetOrElse(new Expr[] { }).ToList(),
				};

		private static Parser<FloatLiteral> FloatLiteral =>
				from val in SParse.Regex(@"[+-]?[0-9]+(\.[0-9]+)?|[+-]?\.[0-9]+").WithWhiteSpace()
				select new FloatLiteral { Value = Convert.ToSingle(val) };

		public static Parser<IEnumerable<string>> TrackSet =>
				from tracks in SParse.Regex("[a-z0-9]").XAtLeastOnce()
				select tracks;

		private static Parser<TrackSetLiteral> TrackSetLiteral =>
				from _ in SParse.String("^")
				from tracks in TrackSet
				select new TrackSetLiteral { Value = new List<string>(tracks) };

		private static Parser<IdentifierLiteral> IdentifierLiteral =>
				from _ in SParse.String("`")
				from value in Identifier
				from __ in SParse.String("`")
				select new IdentifierLiteral { Value = value };

		private static Parser<MmlLiteral> MmlLiteral =>
				from _ in SParse.String("'")
				from value in SParse.Regex(@"[^']*")
				from __ in SParse.String("'")
				select new MmlLiteral { Value = value };

		private static Parser<AssocArrayLiteral> AssocArrayLiteral =>
				from _ in SParse.String("{").WithWhiteSpace()
				from entries in NamedEntryList
				from ____ in SParse.String("}").WithWhiteSpace()
				select new AssocArrayLiteral { Entries = entries.ToList() };

		private static Parser<string> Identifier => SParse.Regex(@"[a-zA-Z_][a-zA-Z_0-9]*");

		private static Parser<IEnumerable<Tuple<string, Expr>>> NamedEntryList =>
				from entries in (
					from name in Identifier.WithWhiteSpace()
					from __ in SParse.String(":").WithWhiteSpace()
					from value in Expr.WithWhiteSpace()
					select Tuple.Create(name, value)
				).DelimitedBy(SParse.String(",").WithWhiteSpace())
				from ___ in SParse.String(",").WithWhiteSpace().Optional()
				select entries;


		private static Parser<Expr> IdentifierExpr =>
				from id in Identifier
				select new IdentifierExpr { Identifier = id };

		private static Parser<Expr> LambdaExpr =>
				from _ in SParse.String(@"\").WithWhiteSpace()
				from inputParam in Identifier.WithWhiteSpace()
				from __ in SParse.String("->").WithWhiteSpace()
				from body in Expr
				select new LambdaExpr {
					InputParam = inputParam,
					Body = body,
				};

		private static Parser<Expr> ParenthesizedExpr =>
				from _ in SParse.String("(").WithWhiteSpace()
				from x in Expr
				from __ in SParse.String(")").WithWhiteSpace()
				select x;

		private static Parser<Expr> PrimaryExpr =>
				((Parser<Expr>) FloatLiteral)
				.Or(TrackSetLiteral)
				.Or(IdentifierLiteral)
				.Or(MmlLiteral)
				.Or(AssocArrayLiteral)
				.Or(IdentifierExpr)
				.Or(LambdaExpr)
				.Or(ParenthesizedExpr)
				.Positioned();

		//private static Parser<Expr> ModuleLabelExpr =>
		//		from x in PrimaryExpr.WithWhiteSpace()
		//		from l in (
		//			from _ in SParse.String("@").WithWhiteSpace()
		//			from l in Identifier.WithWhiteSpace()
		//			select l
		//		).Optional()
		//		select l.IsDefined
		//				? new ModuleLabelExpr { Expr = x, Label = l.Get(), }
		//				: x;

		// TODO 途中で改行できるように
		private static Parser<Expr> ModuleParamExpr =>
				from x in PrimaryExpr.WithWhiteSpace()
				from label in (
					from _ in SParse.String("@").WithWhiteSpace()
					from l in Identifier.WithWhiteSpace()
					select l
				).Optional()
				from constrParams in (
					from _ in SParse.String("(").WithWhiteSpace()
					from @params in NamedEntryList
					from ____ in SParse.String(")").WithWhiteSpace()
					select @params
				).Optional()
				from signalParams in (
					from _ in SParse.String("{").WithWhiteSpace()
					from @params in NamedEntryList
					from ____ in SParse.String("}").WithWhiteSpace()
					select @params
				).Optional()
				select ! label.IsDefined && ! constrParams.IsDefined && ! signalParams.IsDefined
						? x
						: new ModuleParamExpr {
							ModuleDef = x,
							Label = label.GetOrDefault(),
							ConstructorParameters = new List<Tuple<string, Expr>>(constrParams.GetOrElse(Enumerable.Empty<Tuple<string, Expr>>())),
							SignalParameters = new List<Tuple<string, Expr>>(signalParams.GetOrElse(Enumerable.Empty<Tuple<string, Expr>>())),
						};

		private static Parser<Expr> ConnectiveExpr =>
				BinaryExpr(ModuleParamExpr, SParse.String("|").Text(), _ => new ConnectiveExpr());

		private static Parser<Expr> PowerExpr =>
				BinaryExpr(ConnectiveExpr, SParse.String("^").Text(), _ => new PowerExpr());

		private static Parser<Expr> MulDivModExpr =>
				BinaryExpr(PowerExpr, SParse.Regex(@"\*|/|%"), op =>
						op == "*" ? (BinaryExpr) new MultiplicativeExpr()
						: op == "/" ? (BinaryExpr) new DivisiveExpr()
						: (BinaryExpr) new ModuloExpr());

		private static Parser<Expr> AddSubExpr =>
				BinaryExpr(MulDivModExpr, SParse.Regex(@"\+|-"), op => op == "+"
						? (BinaryExpr) new AdditiveExpr()
						: new SubtractiveExpr());

		public static Parser<Expr> BinaryExpr(Parser<Expr> constituentExpr, Parser<string> oper,
				Func<string, BinaryExpr> makeNodeByOperator)
				=> (
					from head in constituentExpr.WithWhiteSpace()
					from tail in (
						from o in oper.WithWhiteSpace()
						from x in constituentExpr.WithWhiteSpace()
						select new { o, x }
					).XMany()
					select tail.Aggregate(head, (lhs, rhs) => {
						var result = makeNodeByOperator(rhs.o);
						result.Lhs = lhs;
						result.Rhs = rhs.x;
						return result;
					})
				).Positioned();

		private static Parser<Expr> Expr => AddSubExpr;

		private static Parser<IEnumerable<Expr>> ExprList =>
				from head in Expr.WithWhiteSpace()
				from tail in SParse.String(",").Token().Then(_ => Expr.WithWhiteSpace()).Many()
				select new[] { head }.Concat(tail);

		private static Parser<MmlStatement> MmlStatement =>
				from tracks in TrackSet
				from _ in SParse.WhiteSpace.XAtLeastOnce()
				from mml in SParse.Regex(@"[^\r\n]*")
				from __ in SParse.LineEnd
				select new MmlStatement {
					Tracks = tracks,
					Mml = mml,
				};

		/// <summary>
		/// 任意の文
		/// </summary>
		private static Parser<Statement> Statement =>
				((Parser<Statement>) DirectiveStatement)
				.Or(MmlStatement)

				.Positioned();

		/// <summary>
		/// 変換単位全体。文を任意個並べたもの
		/// </summary>
		private static Parser<CompilationUnit> CompilationUnit =>
				from ss in Statement.Token().XMany().Token().End() // TODO 空白の扱い、これでよいか検証
				select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string moddl) => CompilationUnit.Parse(RemoveComments(moddl + "\n"));

		private static string RemoveComments(string moddl) {
			return Regex.Replace(moddl, @"//[^\r\n]*(\r\n|\r|\n)", "\n");
		}

	}
}
