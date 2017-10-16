using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModularAudioSharp;

namespace ModularAudioSharpTest {
	[TestClass]
	public class DelayBufferTest {
		[TestMethod]
		public void TestPush() {
			var buffer = new DelayBuffer<int>(2);
			Assert.AreEqual(0, buffer[-1]);
			Assert.AreEqual(0, buffer[0]);

			buffer.Push(42);
			Assert.AreEqual(0, buffer[-1]);
			Assert.AreEqual(42, buffer[0]);

			buffer.Push(33);
			Assert.AreEqual(42, buffer[-1]);
			Assert.AreEqual(33, buffer[0]);

			buffer.Push(800);
			Assert.AreEqual(33, buffer[-1]);
			Assert.AreEqual(800, buffer[0]);
		}

	}
}
