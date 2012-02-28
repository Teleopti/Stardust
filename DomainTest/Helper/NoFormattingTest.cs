using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	class NoFormattingTest
	{
		[Test]
		public void ShouldReturnTextUnformatted()
		{
			var target = new NoFormatting();
			var text = "Hejsan hoppsan lillebror!";
			var result = target.Format(text);

			result.Should().Be.EqualTo(text);
		}
	}
}
