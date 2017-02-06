using System;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class DataProtectionControllerTest
	{
		[Test]
		public void ShouldPersistUserResponseYes()
		{
			var personalSettingDataRepository = new FakePersonalSettingDataRepository();
			var target = new DataProtectionController(personalSettingDataRepository);
			DateTime utcTimeStamp = DateTime.UtcNow;
			IHttpActionResult result = target.Yes();
			
			var response = personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			response.Response.Should().Be.EqualTo(DataProtectionEnum.Yes);
			response.ResponseDate.Should().Be.GreaterThanOrEqualTo(utcTimeStamp);
			result.Should().Be.OfType<OkResult>();
		}

		[Test]
		public void ShouldPersistUserResponseNo()
		{
			var personalSettingDataRepository = new FakePersonalSettingDataRepository();
			var target = new DataProtectionController(personalSettingDataRepository);
			DateTime utcTimeStamp = DateTime.UtcNow;
			IHttpActionResult result = target.No();

			var response = personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			response.Response.Should().Be.EqualTo(DataProtectionEnum.No);
			response.ResponseDate.Should().Be.GreaterThanOrEqualTo(utcTimeStamp);
			result.Should().Be.OfType<OkResult>();
		}
	}
}
