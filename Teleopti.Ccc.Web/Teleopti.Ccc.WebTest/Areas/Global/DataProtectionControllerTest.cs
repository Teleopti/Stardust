using System;
using System.Globalization;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
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
			var target = new DataProtectionController(personalSettingDataRepository, null, null);
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
			var target = new DataProtectionController(personalSettingDataRepository, null, null);
			DateTime utcTimeStamp = DateTime.UtcNow;
			IHttpActionResult result = target.No();

			var response = personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			response.Response.Should().Be.EqualTo(DataProtectionEnum.No);
			response.ResponseDate.Should().Be.GreaterThanOrEqualTo(utcTimeStamp);
			result.Should().Be.OfType<OkResult>();
		}

		[Test]
		public void ShouldReturnTranslatedDataProtectionTextWithUserInformation()
		{
			var person = PersonFactory.CreatePerson(new Name("john", "Doe"));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("en-GB"));
			person.Email = "my@email.com";
			var loggedOnUser = new FakeLoggedOnUser(person);
			var datasource = new FakeCurrentDatasource("my ds");
			var target = new DataProtectionController(null, loggedOnUser, datasource);
			DefinedLicenseDataFactory.SetLicenseActivator(datasource.CurrentName(), new FakeLicenseActivator("the customer"));
			var actionResult = target.QuestionText();

			var response = actionResult as OkNegotiatedContentResult<string>;
			response.Content.Should().Contain(person.Email.Trim());
			response.Content.Should().Contain(person.Name.FirstName);
			response.Content.Should().Contain(person.Name.LastName);
			response.Content.Should().Contain(person.PermissionInformation.UICulture().DisplayName);
			response.Content.Should().Contain("the customer");
			DefinedLicenseDataFactory.ClearLicenseActivators();
		}
	}
}
