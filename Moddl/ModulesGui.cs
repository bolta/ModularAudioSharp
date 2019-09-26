using Moddl.Language;
using ModularAudioSharp;
using ModularAudioSharp.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moddl {
	static partial class Modules {
		// TODO 全ての GUI インスタンスを捕捉するためにグローバル変数を使っている。この方法がベストだとは思っていないが…
		internal static IEnumerable<Control> AllGui => allGui;
		private static List<Control> allGui = new List<Control>();

		internal static Module Slider(IDictionary<string, Value> constrParams) {
			// TODO コピペ解消
			var min = constrParams.TryGetClassValue("min")?.AsFloat() ?? 0f;
			var max = constrParams.TryGetClassValue("max")?.AsFloat() ?? 100f;
			var init = constrParams.TryGetClassValue("init")?.AsFloat() ?? 50f;

			// TODO min <= val <= max, min < max であることのチェック

			var output = Nodes.Proxy(init);
			var guiMinimum = 0;
			var guiMaximum = 10000;

			var bar = new TrackBar {
				Width = 400,
				Minimum = guiMinimum,
				Maximum = guiMaximum,
				Value = (int) ((init - min) * (guiMaximum - guiMinimum) / (max - min) + guiMinimum),
			};
			//string makeText() => bar.Value.ToString();
			var text = new TextBox {
				Width = 100,
				TextAlign = HorizontalAlignment.Right,
				Text = init.ToString(),
			};
			bar.ValueChanged += (s, e) => {
				var outVal = (max - min) * (bar.Value - bar.Minimum) / (bar.Maximum - bar.Minimum) + min;
				output.Set(outVal);
				text.Text = outVal.ToString();
			};

			var panel = ArrangeHorizontally(bar, text);

			allGui.Add(panel);

			return new Module(new ProxyController<float>[] { }, output,
					new Dictionary<string, ProxyController<float>>() { },
					new INotable[] { })
					.WithGui(panel);
		}

		private static Control ArrangeHorizontally(IEnumerable<Control> ctrls) {
			var panel = new Panel();

			var x = 0;
			var h = 0;
			foreach (var ctrl in ctrls) {
				ctrl.Location = new Point(x, 0);
				panel.Controls.Add(ctrl);

				x += ctrl.Width;
				h = Math.Max(h, ctrl.Height);
			}
			panel.Size = new Size(x, h);

			return panel;
		}
		private static Control ArrangeHorizontally(params Control[] ctrls) => ArrangeHorizontally((IEnumerable<Control>) ctrls);

	}
}
