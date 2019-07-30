﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	public class CompilationUnit {
		public IEnumerable<Statement> Statements { get; set; }
		public override string ToString() => string.Join(" ", this.Statements.Select(s => s.ToString()));
	}

	public class Statement {

	}

	public class MmlStatement : Statement {
		public IEnumerable<string> Parts { get; set; }
		public string Mml { get; set; }
		public override string ToString() => string.Format("{0} {1}",
				string.Join("", this.Parts),
				this.Mml);
	}
}