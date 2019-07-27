using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Mml {

	// TODO 各ノードの ToString() で実装してもいいかも……
	public class AstDumper : AstVisitor {
		private readonly TextWriter output;
		private int depth = 0;

		public AstDumper(TextWriter output) {
			this.output = output;
		}

		public override void Visit(CompilationUnit visitee) {
			this.WriteLineWithType(visitee);
			VisitChildren(visitee.Statements, s => s.Accept(this));
		}

		public override void Visit(OctaveStatement visitee) {
			this.WriteLineWithType(visitee, $"o{visitee.Value}");
		}

		public override void Visit(OctaveIncrStatement visitee) {
			this.WriteLineWithType(visitee, ">");
		}
		public override void Visit(OctaveDecrStatement visitee) {
			this.WriteLineWithType(visitee, "<");
		}
		public override void Visit(LengthStatement visitee) {
			this.WriteLineWithType(visitee, $"L{visitee.Value}");
		}
		public override void Visit(ToneStatement visitee) {
			this.WriteLineWithType(visitee, $"{visitee.ToneName} {visitee.Length?.ToString() ?? ""}");
		}

		private void VisitChildren<Child>(IEnumerable<Child> children, Action<Child> action) {
			++ this.depth;
			try {
				foreach (var child in children) action(child);
			} finally {
				-- this.depth;
			}
		}

		private void WriteLineWithType<T>(T obj, string content = "") =>
				this.WriteLine($"[{obj.GetType().Name}] {content}");

		private void WriteLine(string content) =>
				this.output.WriteLine(string.Concat(Enumerable.Repeat("\t", this.depth)) + content);
	}
}
