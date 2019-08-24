﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;
using Sprache;
using System.Text.RegularExpressions;

namespace Moddl.Language {
	public class Parser {

		public readonly static Parser<DirectiveStatement> directiveStatement =
				from _ in SParse.String("@").WithWhiteSpace()
				from name in SParse.Regex("[a-zA-Z0-9_]+").WithWhiteSpace()
				from args in valueList.Optional()
				from __ in SParse.LineEnd // TODO LineTerminator とはどう違う？
				select new DirectiveStatement {
					Name = name,
					Arguments = args.GetOrElse(new Expr[] { }).ToList(),
				};

		public readonly static Parser<FloatLiteral> floatValue =
				from val in SParse.Regex(@"[+-]?[0-9]+(\.[0-9]+)?|[+-]?\.[0-9]+").WithWhiteSpace()
				select new FloatLiteral { Value = Convert.ToSingle(val) };

		public readonly static Parser<IEnumerable<string>> trackSet =
				from tracks in SParse.Chars("abc").AtLeastOnce() // TODO case insensitive で任意の英字を許す
				select tracks.Select(c => c.ToString());

		public readonly static Parser<TrackSetLiteral> trackSetValue =
				from _ in SParse.String("^")
				from tracks in trackSet
				select new TrackSetLiteral { Value = new List<string>(tracks) };

		public readonly static Parser<IdentifierLiteral> identifierValue =
				from id in SParse.Regex(@"[a-zA-Z_][a-zA-Z_0-9]*")
				select new IdentifierLiteral { Value = id };

		public readonly static Parser<Expr> primitiveExpr =
				((Parser<Expr>) floatValue)
				.Or(trackSetValue)
				.Or(identifierValue);

		public readonly static Parser<ConnectiveExpr> connectiveExpr =
				from head in primitiveExpr.WithWhiteSpace()
				from tail in (
					from _ in SParse.String("|").WithWhiteSpace()
					from p in primitiveExpr.WithWhiteSpace()
					select p
				).Many()
				select new ConnectiveExpr { Args = new[] { head }.Concat(tail) };

		public readonly static Parser<Expr> value =
				connectiveExpr
				;

		public readonly static Parser<IEnumerable<Expr>> valueList =
				from head in value.WithWhiteSpace()
				from tail in SParse.String(",").Token().Then(_ => value.WithWhiteSpace()).Many()
				select new[] { head }.Concat(tail);

		public readonly static Parser<MmlStatement> mmlStatement =
				from tracks in trackSet
				from _ in SParse.WhiteSpace.AtLeastOnce()
				from mml in SParse.Regex(@"[^\r\n]*")
				from __ in SParse.LineEnd
				select new MmlStatement {
					Tracks = tracks,
					Mml = mml,
				};

		/// <summary>
		/// 任意の文
		/// </summary>
		public readonly static Parser<Statement> statement =
				((Parser<Statement>) directiveStatement)
				.Or(mmlStatement);

		/// <summary>
		/// 変換単位全体。文を任意個並べたもの
		/// </summary>
		public readonly static Parser<CompilationUnit> compilationUnit =
				from ss in statement.Token().Many().Token() // TODO 空白の扱い、これでよいか検証
				select new CompilationUnit { Statements = ss };

		public CompilationUnit Parse(string moddl) => compilationUnit.Parse(RemoveComments(moddl + "\n"));

		private static string RemoveComments(string moddl) {
			return Regex.Replace(moddl, @"//[^\r\n]*(\r\n|\r|\n)", "\n");
		}

	}
}
