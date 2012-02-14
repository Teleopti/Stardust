using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class UserSettingServiceTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();

			_businessUnit = new BusinessUnit("Bu");
			_businessUnit.SetId(Guid.NewGuid());
			_person = new Person();
			_person.SetId(Guid.NewGuid());
			_printcipal = new PrincipalForTest(_businessUnit, _person);
		}

		[TearDown]
		public void TearDown()
		{
		}

		#endregion

		private IWebReportUserInfoProvider target;
		private PrincipalForTest _printcipal;
		private BusinessUnit _businessUnit;
		private Person _person;
		private MockRepository mocks;

		protected class PrincipalForTest : ICurrentPrincipalProvider
		{
			private readonly BusinessUnit _businessUnit;
			private readonly Person _person;
			private readonly TeleoptiPrincipalForTest _principalForTest;

			public PrincipalForTest(BusinessUnit businessUnit, Person person)
			{
				_businessUnit = businessUnit;
				_person = person;
				_principalForTest =
					new TeleoptiPrincipalForTest(new TeleoptiIdentity(_person.Name.ToString(), null, _businessUnit, null), _person);
			}

			#region ICurrentPrincipalProvider Members

			public TeleoptiPrincipal Current()
			{
				return _principalForTest;
			}

			#endregion
		}

		[Test]
		public void ShouldPopulateFromPrincipal()
		{
			var personRepository = mocks.DynamicMock<IPersonRepository>();

			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(_person.Id.Value)).Return(_person);
			}

			target = new WebReportUserInfoProvider(_printcipal, personRepository);

			WebReportUserInformation webReportUserInformation;
			using (mocks.Playback())
			{
				webReportUserInformation = target.GetUserInformation();
			}

			webReportUserInformation.Should().Not.Be.Null();
			webReportUserInformation.BusinessUnitCode.Should().Be.EqualTo(_businessUnit.Id);
			webReportUserInformation.PersonCode.Should().Be.EqualTo(_person.Id);
			webReportUserInformation.TimeZoneCode.Should().Be.EqualTo("W. Europe Standard Time");
		}
	}
}