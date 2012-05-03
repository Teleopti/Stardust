using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	using System;
	using NUnit.Framework;
	using Rhino.Mocks;
	using SharpTestsEx;
	using Domain.Common;
	using Domain.Repositories;
	using Domain.Security.Principal;
	using TestCommon;
	using Web.Areas.MobileReports.Core;
	using Web.Areas.MobileReports.Models.Domain;
	using Web.Core.RequestContext;

	[TestFixture]
	public class UserSettingServiceTest
	{
		private BusinessUnit _businessUnit;
		private MockRepository _mocks;
		private Person _person;
		private PrincipalProviderForTest _principalProvider;
		private IWebReportUserInfoProvider _target;
		private CultureInfo _uiCulture;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();

			_businessUnit = new BusinessUnit("Bu");
			_businessUnit.SetId(Guid.NewGuid());
			_uiCulture = CultureInfo.GetCultureInfo("en");
			_person = new Person();
			_person.SetId(Guid.NewGuid());
			_person.PermissionInformation.SetUICulture(_uiCulture);
			_principalProvider = new PrincipalProviderForTest(_businessUnit, _person);
		}

		[Test]
		public void ShouldPopulateFromPrincipal()
		{
			var personRepository = _mocks.DynamicMock<IPersonRepository>();

			using (_mocks.Record())
			{
				Expect.Call(personRepository.Get(_person.Id.Value)).Return(_person);
			}

			_target = new WebReportUserInfoProvider(_principalProvider, personRepository);

			WebReportUserInformation webReportUserInformation;
			using (_mocks.Playback())
			{
				webReportUserInformation = _target.GetUserInformation();
			}

			webReportUserInformation.Should().Not.Be.Null();
			webReportUserInformation.BusinessUnitCode.Should().Be.EqualTo(_businessUnit.Id);
			webReportUserInformation.PersonCode.Should().Be.EqualTo(_person.Id);
			webReportUserInformation.TimeZoneCode.Should().Be.EqualTo("W. Europe Standard Time");
			webReportUserInformation.LanguageId.Should().Be.EqualTo(CultureInfo.CreateSpecificCulture(_uiCulture.TwoLetterISOLanguageName).LCID);
		}

		[TearDown]
		public void TearDown()
		{
		}

		protected class PrincipalProviderForTest : IPrincipalProvider
		{
			private readonly BusinessUnit _businessUnit;
			private readonly Person _person;
			private readonly ITeleoptiPrincipal _principal;

			public PrincipalProviderForTest(BusinessUnit businessUnit, Person person)
			{
				_businessUnit = businessUnit;
				_person = person;
				_principal =
					new TeleoptiPrincipal(
						new TeleoptiIdentity(_person.Name.ToString(), null, _businessUnit, null, AuthenticationTypeOption.Unknown), _person);
			}

			public ITeleoptiPrincipal Current()
			{
				return _principal;
			}
		}
	}
}