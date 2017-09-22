using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    /// <summary>
    /// Tests for PersonPeriodModel
    /// </summary>
    [TestFixture]
    public class PersonPeriodChildModelTest : PeopleAdminTestBase
    {
        private PersonPeriodChildModel _target;
        private IPersonPeriod _personPeriod;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Mama Hawa");

            CreateSkills();

            CreateExternalLogOnCollection();

            PersonSkillCollection.Clear();

            CreatePersonSkillCollection();

            CreateSiteTeamCollection();

            _personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly1);
            
            _personPeriod.RuleSetBag = new RuleSetBag();

        	_personPeriod.BudgetGroup = new BudgetGroup();

			_person.AddExternalLogOn(ExternalLogOn1, _personPeriod);
			_person.AddExternalLogOn(ExternalLogOn2, _personPeriod);
			_person.AddExternalLogOn(ExternalLogOn3, _personPeriod);

            _person.AddPersonPeriod(_personPeriod);
	        _person.AddSkill(PersonSkill1, _personPeriod);
			_person.AddSkill(PersonSkill2, _personPeriod);

			_person.ChangeTeam(TeamBlue,_personPeriod);

            _target = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(_personPeriod);
            _target.SetPersonSkillCollection(PersonSkillCollection);
            _target.SetPersonExternalLogOnCollection(ExternalLogOnCollection);
            _target.SetSiteTeamAdapterCollection(SiteTeamAdapterCollection);
            _target.FullName = "Mama Ibba";
        }

        [TearDown]
        public void TestDispose()
        {
            _personPeriod = null;
            Skill1 = null;
            Skill2 = null;
            Skill3 = null;
            ExternalLogOn1 = null;
            ExternalLogOn2 = null;
            ExternalLogOn3 = null;
            _target = null;
        }

        #region Tests

        [Test]
        public void VerifyPropertiesNotNullOrEmpty()
        {
            Assert.IsNotEmpty(_target.FullName);
            Assert.IsNotNull(_target.PeriodDate);
            Assert.IsNotNull(_target.SiteTeam);
            Assert.IsNotNull(_target.Contract);
            Assert.IsNotNull(_target.ContractSchedule);
            Assert.IsNotNull(_target.PersonContract);
            Assert.IsNotNull(_target.PartTimePercentage);
            Assert.IsNotNull(_target.RuleSetBag);
            Assert.IsNotNull(_target.BudgetGroup);
            Assert.IsFalse(_target.CanGray);
            Assert.AreEqual("_skill1, _skill2", _target.PersonSkills);
            Assert.IsNotNull(_target.Period);
            Assert.IsNotEmpty(_target.ExternalLogOnNames);
            Assert.IsNull(_target.Note);
            Assert.AreEqual("Login name (DS), Login name (DS), Login name (DS)", _target.ExternalLogOnNames);
            Assert.IsNotNull(_target.Parent);
        }

        [Test]
        public void VerifyParentValues()
        {
            Assert.AreEqual(_person,_target.Parent);
        }

        [Test]
        public void VerifyPeriodDateCanBeSet()
        {
            _target.PeriodDate = DateOnly2;

            Assert.AreEqual(DateOnly2, _target.PeriodDate);
        }

        [Test]
        public void VerifyPeriodDateCannotBeSetToSameAsOtherPeriod()
        {
            _person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly2));
            _target.PeriodDate = DateOnly2;

            Assert.AreEqual(DateOnly2.AddDays(1), _target.PeriodDate);
        }

        [Test]
        public void VerifyNoteCanSet()
        {
            string note = "Mage Note Eka";
            _target.Note = "Mage Note Eka";
            Assert.AreEqual(note, _target.Note);

        }

        [Test]
        public void VerifyRuleSetBag()
        {
            RuleSetBag ruleSetBag = new RuleSetBag();
            _target.RuleSetBag = ruleSetBag;

            Assert.AreEqual(ruleSetBag, _target.RuleSetBag);
        }

		[Test]
		public void VerifyCurrentBudgetGroupCanSet()
		{
			BudgetGroup budgetGroup = new BudgetGroup();
			_target.BudgetGroup = budgetGroup;

			Assert.AreEqual(budgetGroup, _target.BudgetGroup);
		}

        [Test]
        public void VerifyCurrentContractCanSet()
        {
            IContract contract = ContractFactory.CreateContract("Contract1");
            _target.Contract = contract;

            Assert.AreEqual(contract, _target.Contract);

            _target.PersonContract = null;
            Assert.AreEqual(null, _target.Contract);
        }

        [Test]
        public void VerifyCurrentPersonContractCanSet()
        {
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            _target.PersonContract = personContract;

            Assert.AreEqual(personContract, _target.PersonContract);
        }

        [Test]
        public void VerifyCurrentPartTimePercentageCanSet()
        {
            IPartTimePercentage partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("PartTimePercentage1");
            _target.PartTimePercentage = partTimePercentage;

            Assert.AreEqual(partTimePercentage, _target.PartTimePercentage);

            _target.PersonContract = null;
            Assert.AreEqual(null, _target.PartTimePercentage);
        }

        [Test]
        public void VerifyCurrentContractScheduleCanSet()
        {
            IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("ContractSchedule1");
            _target.ContractSchedule = contractSchedule;

            Assert.AreEqual(contractSchedule, _target.ContractSchedule);

            _target.PersonContract = null;
            Assert.AreEqual(null, _target.ContractSchedule);
        }

        [Test]
        public void VerifyPersonSkillCanGetAndSet()
        {
            IPersonPeriod currentPeriod = _target.ContainedEntity;

            Assert.AreEqual(_personPeriod, currentPeriod);
         
            Assert.AreEqual("_skill1, _skill2", _target.PersonSkills);

            Assert.AreEqual(2, currentPeriod.PersonSkillCollection.Count());
            _target.PersonSkills = Skill3.Name;
            Assert.AreEqual(1, currentPeriod.PersonSkillCollection.Count());
            Assert.AreEqual("_skill3", _target.PersonSkills);
        }

        [Test]
        public void VerifyPersonExternalLogOnNamesCanGetAndSet()
        {
            IPersonPeriod currentPeriod = _target.ContainedEntity;

            Assert.AreEqual(_personPeriod, currentPeriod);
            Assert.AreEqual(ExternalLogOn1, currentPeriod.ExternalLogOnCollection.First());

            Assert.AreEqual(3, currentPeriod.ExternalLogOnCollection.Count());
            _target.ExternalLogOnNames = "Login name (DS)";
            Assert.AreEqual(3, currentPeriod.ExternalLogOnCollection.Count());
            Assert.AreEqual("Login name (DS), Login name (DS), Login name (DS)", _target.ExternalLogOnNames);
        }

        [Test]
        public void VerifySiteTeamCanGetAndSet()
        {
            _target.SiteTeam = SiteTeam1;
            Assert.AreEqual(BLUESITE + "/" + BLUETEAM, _target.SiteTeam.Description);

            _target.SiteTeam = SiteTeam2;
            Assert.AreEqual(REDSITE + "/" + REDTEAM, _target.SiteTeam.Description);
        }

        [Test]
        public void VerifyCanSetCanBold()
        {
            Assert.IsFalse(_target.CanBold);
            _target.CanBold = true;
            Assert.IsTrue(_target.CanBold);

        }

        #endregion
    }
}
