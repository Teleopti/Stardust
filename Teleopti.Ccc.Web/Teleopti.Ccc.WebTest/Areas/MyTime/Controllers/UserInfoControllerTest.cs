using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class UserInfoControllerTest
	{
		[Test]
		public void Culture_WhenUsersCultureHasMondayAsFirstDay_ShouldSetWeekStartTo1()
		{
			var cultureStub = MockRepository.GenerateStub<IUserCulture>();
			cultureStub.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var target = new UserInfoController(cultureStub);
			
			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.WeekStart,Is.EqualTo(1));
			Assert.That(result,Is.Not.Null);
		}

		[Test]
		public void Culture_DateFormatIsCorrectForSwidishCulture()
		{
			var cultureStub = MockRepository.GenerateStub<IUserCulture>();
			cultureStub.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var target = new UserInfoController(cultureStub);

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.DateFormatForMoment, Is.EqualTo("YYYY-MM-DD"));
			Assert.That(result, Is.Not.Null);
		}
	}
}