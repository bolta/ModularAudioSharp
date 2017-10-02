using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprache;
using static ModularAudioSharp.Mml.SimpleMmlParser;

namespace ModularAudioSharp.Mml {
	[TestClass]
	public class SimpleMmlParserTest {
		[TestMethod]
		public void TestLengthElement1() {
			var rule = lengthElement.End();
			{
				var result = rule.Parse("");
				Assert.IsFalse(result.Number.HasValue);
				Assert.AreEqual(0, result.Dots);
			}
			{
				var result = rule.Parse("4");
				Assert.AreEqual(4, result.Number);
				Assert.AreEqual(0, result.Dots);
			}
			{
				var result = rule.Parse("....");
				Assert.IsFalse(result.Number.HasValue);
				Assert.AreEqual(4, result.Dots);
			}
			{
				var result = rule.Parse("4 ...");
				Assert.AreEqual(4, result.Number);
				Assert.AreEqual(3, result.Dots);
			}
		}
	}
}
