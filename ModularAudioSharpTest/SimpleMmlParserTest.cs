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

		[TestMethod]
		public void TestNumber() {
			var rule = integer.End();
			{
				var result = rule.Parse("42");
				Assert.AreEqual(42, result);
			}
			{
				var result = rule.Parse("+42");
				Assert.AreEqual(42, result);
			}
			{
				var result = rule.Parse("-42");
				Assert.AreEqual(-42, result);
			}
			{
				try {
					var result = rule.Parse("*42");
					Assert.Fail("there must occur an exception");
				} catch (ParseException) {
					Assert.IsTrue(true);
				}
			}
			{
				try {
					var result = rule.Parse("42*");
					Assert.Fail("there must occur an exception");
				} catch (ParseException) {
					Assert.IsTrue(true);
				}
			}
		}
	}
}
