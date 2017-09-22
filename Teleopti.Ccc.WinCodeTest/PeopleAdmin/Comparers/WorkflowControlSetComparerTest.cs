using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	[TestFixture]
	public class WorkflowControlSetComparerTest
	{
		private WorkflowControlSetComparer _target;
		private IAuthorization _authorization;

		[SetUp]
		public void Setup()
		{
			_target = new WorkflowControlSetComparer();
			_authorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
		}

		[Test]
		public void VerifyAscendingAndDescending()
		{
			IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
			var personGeneralModelX = new PersonGeneralModel(personA, _authorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());
			personGeneralModelX.WorkflowControlSet = new WorkflowControlSet("A set");

			IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
			var personGeneralModelY = new PersonGeneralModel(personB, _authorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());
			personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

			Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
			Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
		}

		[Test]
		public void VerifyWithNulls()
		{
			IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
			var personGeneralModelX = new PersonGeneralModel(personA, _authorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());

			IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
			var personGeneralModelY = new PersonGeneralModel(personB, _authorization,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());

			Assert.AreEqual(0, _target.Compare(personGeneralModelX, personGeneralModelY));

			personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

			Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
			Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
		}

	}
}
