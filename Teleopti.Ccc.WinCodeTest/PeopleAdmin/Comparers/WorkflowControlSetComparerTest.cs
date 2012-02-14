using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
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
            _principalAuthorization = new PrincipalAuthorization(TeleoptiPrincipal.Current);
        }

        [Test]
        public void VerifyAscendingAndDescending()
        {
            IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
            PersonGeneralModel personGeneralModelX = new PersonGeneralModel(personA, new UserDetail(personA), _principalAuthorization);
            personGeneralModelX.WorkflowControlSet = new WorkflowControlSet("A set");

            IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
            PersonGeneralModel personGeneralModelY = new PersonGeneralModel(personB, new UserDetail(personB), _principalAuthorization);
            personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

            Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
            Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
        }

        [Test]
        public void VerifyWithNulls()
        {
            IPerson personA = PersonFactory.CreatePerson("Kalle", "Kula");
            PersonGeneralModel personGeneralModelX = new PersonGeneralModel(personA, new UserDetail(personA), _principalAuthorization);

            IPerson personB = PersonFactory.CreatePerson("Bosse", "Batong");
            PersonGeneralModel personGeneralModelY = new PersonGeneralModel(personB, new UserDetail(personB), _principalAuthorization);

            Assert.AreEqual(0, _target.Compare(personGeneralModelX, personGeneralModelY));
            
            personGeneralModelY.WorkflowControlSet = new WorkflowControlSet("B set");

            Assert.AreEqual(-1, _target.Compare(personGeneralModelX, personGeneralModelY));
            Assert.AreEqual(1, _target.Compare(personGeneralModelY, personGeneralModelX));
        }

    }
}
