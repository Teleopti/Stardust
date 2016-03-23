using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	[TestFixture]
	public class WorkflowControlSetComparerTest
	{
		private WorkflowControlSetComparer _target;
		private IPrincipalAuthorization _principalAuthorization;

		[SetUp]
		public void Setup()
		{
			_target = new WorkflowControlSetComparer();
			_principalAuthorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
		}

		[Test]
		public void VerifyAscendingAndDescending()
		{
			IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
			var personGeneralModelX = new PersonGeneralModel(personA, _principalAuthorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());
			personGeneralModelX.WorkflowControlSet = new WorkflowControlSet("A set");

			IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
			var personGeneralModelY = new PersonGeneralModel(personB, _principalAuthorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());
			personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

			Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
			Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
		}

		[Test]
		public void VerifyWithNulls()
		{
			IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
			var personGeneralModelX = new PersonGeneralModel(personA, _principalAuthorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());

			IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
			var personGeneralModelY = new PersonGeneralModel(personB, _principalAuthorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());

			Assert.AreEqual(0, _target.Compare(personGeneralModelX, personGeneralModelY));

			personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

			Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
			Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
		}

	}
}
