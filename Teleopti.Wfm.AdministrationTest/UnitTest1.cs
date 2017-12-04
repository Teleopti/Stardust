using System;
using NUnit.Framework;
using SharpTestsEx;

namespace Teleopti.Wfm.AdministrationTest
{
	[TestFixture]
	public class UnitTest1
	{
		[Test, Ignore("under construction")]
		public void TestMethod1()
		{
			var text = "hej";
			text.Should().Be("hej");
		}
	}
}
