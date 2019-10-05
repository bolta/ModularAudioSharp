using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModularAudioSharp.Data.Util;

namespace ModularAudioSharp.Mml {
	//public class AstNode {
	//}

	public class CompilationUnit /*: AstNode*/ {
		public IEnumerable<Command> Commands { get; set; }
		public override string ToString() => string.Join(" ", this.Commands.Select(s => s.ToString()));
	}

	public class Command /*: AstNode*/ {

	}

	public class OctaveCommand : Command {
		public int Value { get; set; }
		public override string ToString() => $"o{this.Value}";
	}
	public class OctaveIncrCommand : Command { public override string ToString() => ">"; }
	public class OctaveDecrCommand : Command { public override string ToString() => "<"; }
	public class LengthCommand : Command {
		public int Value { get; set; }
		public override string ToString() => $"L{this.Value}";
	}

	public class VolumeCommand : Command {
		public float Value { get; set; }
		public override string ToString() => $"V{this.Value}";
	}

	public class VelocityCommand : Command {
		public float Value { get; set; }
		public override string ToString() => $"v{this.Value}";
	}

	public class DetuneCommand : Command {
		public float Value { get; set; }
		public override string ToString() => $"@d{this.Value}";
	}

	public class ToneCommand : Command {
		public ToneName ToneName { get; set; }
		public Length Length { get; set; } // optional
		public bool Slur { get; set; } // optional
		public override string ToString() => string.Format("{0}{1}[2}",
				this.ToneName,
				this.Length?.ToString() ?? "",
				this.Slur ? "&" : "");
	}
	public class RestCommand : Command {
		public Length Length { get; set; } // optional
		public override string ToString() => $"r{this.Length?.ToString() ?? ""}";
	}

	public class ParameterCommand : Command {
		public Identifier Name { get; set; }
		public float Value { get; set; } // TODO いずれは数値以外も取れるように
		public override string ToString() => string.Format("y`{0}`,{1}", this.Name, this.Value);
	}

	public class LoopCommand : Command {
		/// <summary>
		/// 有効な値は有限ループ、null は無限ループを表す
		/// （MML では無限ループを [0 ... ] と書き、[ ... ] は [2 ... ] であることに注意）
		/// </summary>
		public uint? Times { get; set; }
		public IEnumerable<Command> Content { get; set; }
		public override string ToString() => $"[{this.Times ?? 0} {this.Content.Select(s => s.ToString())}]";
	}

	public class LoopBreakCommand : Command { }

	public class ToneName /*: AstNode*/ {
		/// <summary>
		/// 調号を含まない名前。大文字
		/// </summary>
		public string BaseName { get; set; }
		public int Accidental { get; set; }
		public override string ToString() {
			var accText = RepeatString(this.Accidental > 0 ? "+" : "-", Math.Abs(this.Accidental));
			return $"{this.BaseName}{accText}";
		}

	}
	public class Length {
		public IEnumerable<LengthElement> Elements { get; set; }
		public override string ToString() => string.Join("^", this.Elements);
	}
	public class LengthElement {
		/// <summary>音長を示す数値。省略の場合は null。音長 4. に対して 4、.. に対して null となる</summary>
		public int? Number { get; set; }

		/// <summary>付点の数</summary>
		public int Dots { get; set; }

		public override string ToString() => $"{this.Number?.ToString() ?? ""}{RepeatString(".", this.Dots)}";
	}

	/// <summary>
	/// MML 上で命名に使う識別子
	/// </summary>
	public class Identifier {
		public string Name { get; set; }
	}
}
