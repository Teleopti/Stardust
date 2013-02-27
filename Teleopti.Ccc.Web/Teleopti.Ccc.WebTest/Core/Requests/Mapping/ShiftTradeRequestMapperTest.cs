using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestMapperTest
	{
		private IShiftTradeRequestMapper target;

		[SetUp]
		public void Setup()
		{
			target = new ShiftTradeRequestMapper();
		}

		[Test]
		public void ShouldMapSubject()
		{
			const string expected = "hejhej";
			var form = new ShiftTradeRequestForm {Subject = expected};
			
			var res = target.Map(form);
			res.GetSubject(new NoFormatting()).Should().Be.EqualTo(expected);
		}
	}
}