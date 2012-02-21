namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	using System;

	using NUnit.Framework;

	using Rhino.Mocks;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Common;
	using Teleopti.Ccc.Domain.Repositories;
	using Teleopti.Ccc.Domain.Security.Principal;
	using Teleopti.Ccc.TestCommon;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
	using Teleopti.Ccc.Web.Core.RequestContext;

	[TestFixture]
	public class UserSettingServiceTest
	{
		private BusinessUnit _businessUnit;

		private MockRepository _mocks;

		private Person _person;

		private PrincipalForTest _printcipal;

		private IWebReportUserInfoProvider _target;

		[SetUp]
		public void Setup()
		{
			this._mocks = new MockRepository();

			this._businessUnit = new BusinessUnit("Bu");
			this._businessUnit.SetId(Guid.NewGuid());
			this._person = new Person();
			this._person.SetId(Guid.NewGuid());
			this._printcipal = new PrincipalForTest(this._businessUnit, this._person);
		}

		[Test]
		public void ShouldPopulateFromPrincipal()
		{
			var personRepository = this._mocks.DynamicMock<IPersonRepository>();

			using (this._mocks.Record())
			{
				Expect.Call(personRepository.Get(this._person.Id.Value)).Return(this._person);
			}

			this._target = new WebReportUserInfoProvider(this._printcipal, personRepository);

			WebReportUserInformation webReportUserInformation;
			using (this._mocks.Playback())
			{
				webReportUserInformation = this._target.GetUserInformation();
			}

			webReportUserInformation.Should().Not.Be.Null();
			webReportUserInformation.BusinessUnitCode.Should().Be.EqualTo(this._businessUnit.Id);
			webReportUserInformation.PersonCode.Should().Be.EqualTo(this._person.Id);
			webReportUserInformation.TimeZoneCode.Should().Be.EqualTo("W. Europe Standard Time");
		}

		[TearDown]
		public void TearDown()
		{
		}

		protected class PrincipalForTest : ICurrentPrincipalProvider
		{
			private readonly BusinessUnit _businessUnit;

			private readonly Person _person;

			private readonly TeleoptiPrincipalForTest _principalForTest;

			public PrincipalForTest(BusinessUnit businessUnit, Person person)
			{
				this._businessUnit = businessUnit;
				this._person = person;
				this._principalForTest =
					new TeleoptiPrincipalForTest(
						new TeleoptiIdentity(this._person.Name.ToString(), null, this._businessUnit, null), this._person);
			}

			public TeleoptiPrincipal Current()
			{
				return this._principalForTest;
			}
		}
	}
}