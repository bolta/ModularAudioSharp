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

		private static Parser<DirectiveStatement> directiveStatement =>
				from _ in SParse.String("@").WithWhiteSpace()
				from name in SParse.Regex("[a-zA-Z0-9_]+").WithWhiteSpace()
				from args in exprList.Optional()
				from __ in SParse.LineEnd // TODO LineTerminator とはどう違う？
				select new DirectiveStatement {
					Name = name,
					Arguments = args.GetOrElse(new Expr[] { }).ToList(),
				};

		private static Parser<FloatLiteral> floatLiteral =>
				from val in SParse.Regex(@"[+-]?[0-9]+(\.[0-9]+)?|[+-]?\.[0-9]+").WithWhiteSpace()
				select new FloatLiteral { Value = Convert.ToSingle(val) };

		public readonly static Parser<IEnumerable<string>> trackSet =
				from tracks in SParse.Regex("[a-z0-9]").XAtLeastOnce()
				select tracks;

		private static Parser<TrackSetLiteral> trackSetLiteral =>
				from _ in SParse.String("^")
				from tracks in trackSet
				select new TrackSetLiteral { Value = new List<string>(tracks) };

		private static Parser<string> identifier => SParse.Regex(@"[a-zA-Z_][a-zA-Z_0-9]*");

		// TODO 途中で改行できるように
		private static Parser<Expr> moduleParamExpr =>
//				from id in identifier.WithWhiteSpace()
				from x in primaryExpr.WithWhiteSpace()
				from @params in (
					from _ in SParse.String("{").WithWhiteSpace()
					from @params in (
						from name in identifier.WithWhiteSpace()
						from __ in SParse.String(":").WithWhiteSpace()
						from value in expr.WithWhiteSpace()
						select Tuple.Create(name, value)
					).DelimitedBy(SParse.String(",").WithWhiteSpace())
					from ___ in SParse.String(",").WithWhiteSpace().Optional()
					from ____ in SParse.String("}").WithWhiteSpace()
					select @params
				).Optional()
				select ! @params.IsDefined
						? x
						: new ModuleParamExpr {
							//Identifier = id,
							ModuleDef = x,
							Parameters = new List<Tuple<string, Expr>>(@params.GetOrElse(Enumerable.Empty<Tuple<string, Expr>>())),
						};

		private static Parser<Expr> identifierExpr =>
				from id in identifier
				select new IdentifierExpr { Identifier = id };
				

		private static Parser<Expr> parenthesizedExpr =>
				from _ in SParse.String("(").WithWhiteSpace()
				from x in expr
				from __ in SParse.String(")").WithWhiteSpace()
				select x;

		private static Parser<Expr> primaryExpr =>
				((Parser<Expr>) floatLiteral)
				.Or(trackSetLiteral)
				//.Or(moduleCallExpr)
				.Or(identifierExpr)
				.Or(parenthesizedExpr)
				.Positioned();

		private static Parser<Expr> connectiveExpr =>
				BinaryExpr(moduleParamExpr, SParse.String("|").Text(), _ => new ConnectiveExpr());

		private static Parser<Expr> mulDivExpr =>
				BinaryExpr(connectiveExpr, SParse.Regex(@"\*|/"), op => op == "*"
						? (BinaryExpr) new MultiplicativeExpr()
						: new DivisiveExpr());

		private static Parser<Expr> addSubExpr =>
				BinaryExpr(mulDivExpr, SParse.Regex(@"\+|-"), op => op == "+"
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

		private static Parser<Expr> expr => addSubExpr;

		public readonly static Parser<IEnumerable<Expr>> exprList =
				from head in expr.WithWhiteSpace()
				from tail in SParse.String(",").Token().Then(_ => expr.WithWhiteSpace()).Many()
				select new[] { head }.Concat(tail);

		private static Parser<MmlStatement> mmlStatement =>
				from tracks in trackSet
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
		private static Parser<Statement> statement =>
				((Parser<Statement>) directiveStatement)
				.Or(mmlStatement)

				.Positioned();

		/// <summary>
		/// 変換単位全体。文を任意個並べたもの
		/// </summary>
		private static Parser<CompilationUnit> compilationUnit =>
				from ss in statement.Token().XMany().Token().End() // TODO 空白の扱い、これでよいか検証
				select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string moddl) => compilationUnit.Parse(RemoveComments(moddl + "\n"));

		private static string RemoveComments(string moddl) {
			return Regex.Replace(moddl, @"//[^\r\n]*(\r\n|\r|\n)", "\n");
		}

	}
}
