using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moddl.Language {
	[TestClass]
	public class ParserTest {
		[TestMethod]
		public void TestMmlStatement() {
			var parser = new Parser();
			var result = parser.Parse("abc o4L8v15cde");
			Assert.AreEqual(result.Statements.Count(), 1);
			var stmt = (MmlStatement) result.Statements.First();
			CollectionAssert.AreEqual(new [] { "a", "b", "c"}, stmt.Tracks.ToArray());
			Assert.AreEqual("o4L8v15cde", stmt.Mml);
		}

		[TestMethod]
		public void TestMultiStatement() {
			var parser = new Parser();
			var result = parser.Parse(
@"
a	mml1
b	mml2
");
			Assert.AreEqual(result.Statements.Count(), 2);
			
		}
	}
}
