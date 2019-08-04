using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SParse = Sprache.Parse;

namespace Moddl.Language {
	static class ParserExtensions {
		/// <summary>
		/// 
		/// http://www.magnuslindhe.com/2014/09/parsing-whitespace-and-new-lines-with-sprache/
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static Parser<T> WithWhiteSpace<T>(this Parser<T> parser)
		{
			if (parser == null) throw new ArgumentNullException("parser");

			return from leading in SParse.WhiteSpace.Except(NewLine).Many()
				   from item in parser
				   from trailing in SParse.WhiteSpace.Except(NewLine).Many()
				   select item;
		}

		public static readonly Parser<string> NewLine = SParse
				.String(Environment.NewLine)
				.Text()
				.Named("new line");
	}
}
