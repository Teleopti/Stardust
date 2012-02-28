using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	class NormalizeTextTest
	{
		[Test]
		public void ShouldReturnTextNormalized()
		{
			var target = new NormalizeText();
			var text = "< means less than";
			var doc = new XmlDocument();
			var expected = doc.CreateElement("A");
			expected.InnerText = text;

			var result = target.Format(text);

			result.Should().Be.EqualTo(expected.InnerXml);
		}
	}
}