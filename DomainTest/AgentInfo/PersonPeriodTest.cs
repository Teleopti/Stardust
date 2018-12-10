using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture]
	public class PersonPeriodTest
	{
		private PersonPeriod _target;
		private IPerson _person;

		/// <summary>
		/// Setups this instance.
		/// </summary>
		[SetUp]
		public void Setup()
		{
			DateOnly startDate = new DateOnly(2000, 1, 1);
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();
			ITeam team = TeamFactory.CreateSimpleTeam();

			_target = new PersonPeriod(startDate, personContract, team);

			_person = PersonFactory.CreatePerson();
			_person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Local));
			_person.AddPersonPeriod(_target);
		}

		/// <summary>
		/// Verifies the default properties.
		/// </summary>
		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.IsNotNull(_target.PersonContract);
			Assert.IsNotNull(_target.Team);
			Assert.IsNotNull(_target.PersonSkillCollection);
			Assert.IsNull(_target.RuleSetBag);
		}

		/// <summary>
		/// Verifies the period can be set and get.
		/// </summary>
		[Test]
		public void VerifyPeriodCanBeSetAndGet()
		{
			DateOnly startDate = new DateOnly(2000,01,01);
			_target.StartDate = startDate;
			
			Assert.AreEqual(_target.StartDate,startDate);
		}

		[Test]
		public void CanGetEndDate()
		{
			ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
			ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");
		
			IPerson person = PersonFactory.CreatePerson();
			IPerson person2 = PersonFactory.CreatePerson();

			IPersonPeriod period1 = _target;
			IPersonPeriod period2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
			IPersonPeriod period3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team3);

			person.AddPersonPeriod(period1);
			person.AddPersonPeriod(period2);

			person2.AddPersonPeriod(period3);
			person2.TerminatePerson(new DateOnly(2003,1,1), new MockRepository().StrictMock<IPersonAccountUpdater>());

			Assert.AreEqual(period2.StartDate.AddDays(-1),period1.EndDate());
			Assert.AreEqual(new DateOnly(2059, 12, 31), period2.EndDate());
			Assert.AreEqual(period3.EndDate(),person2.TerminalDate);
		}

		[Test]
		public void CanGetPeriod()
		{
			ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
			ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

			IPerson person = PersonFactory.CreatePerson();
			IPerson person2 = PersonFactory.CreatePerson();

			IPersonPeriod period1 = _target;
			IPersonPeriod period2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
			IPersonPeriod period3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team3);

			person.AddPersonPeriod(period1);
			person.AddPersonPeriod(period2);

			person2.AddPersonPeriod(period3);
			person2.TerminatePerson(new DateOnly(2003, 1, 1), new MockRepository().StrictMock<IPersonAccountUpdater>());

			Assert.AreEqual(period2.Period.StartDate.AddDays(-1), period1.Period.EndDate);
			Assert.AreEqual(new DateOnly(2059, 12, 31), period2.Period.EndDate);
			Assert.AreEqual(period3.Period.EndDate,  person2.TerminalDate);
		}

		[Test]
		public void CanAddPersonSkill()
		{
			IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill("test", 1);
			_target.AddPersonSkill(personSkill);

			Assert.IsTrue(_target.PersonSkillCollection.Contains(personSkill));
			Assert.AreEqual(1, _target.PersonSkillCollection.Count());
		}

		[Test]
		public void CannotAddPersonSkillsWithSameSkill()
		{
			ISkill skill = SkillFactory.CreateSkill("test skill");
			IPersonSkill personSkill1 = PersonSkillFactory.CreatePersonSkill(skill, 1);
			IPersonSkill personSkill2 = PersonSkillFactory.CreatePersonSkill(skill, 1);
			_target.AddPersonSkill(personSkill1);
			Assert.Throws<ArgumentException>(() => _target.AddPersonSkill(personSkill2));
		}

		[Test]
		public void VerifyNoPublicEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
		}

		[Test]
		public void VerifyConstructorTeamMustNotBeNull()
		{
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();

			Assert.Throws<ArgumentNullException>(() => _target = new PersonPeriod(new DateOnly(), personContract, null));
		}

		[Test]
		public void VerifyCanSetRuleSetBag()
		{
			RuleSetBag rules = new RuleSetBag();
			_target.RuleSetBag = rules;
		}

		[Test]
		public void VerifyTeamCanSet()
		{
			ITeam team1 = TeamFactory.CreateSimpleTeam();
			_target.Team = team1;
			Assert.AreEqual(team1, _target.Team);
		}

		[Test]
		public void VerifyPersonContractCanSet()
		{
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();

			_target.PersonContract = personContract;
			Assert.AreEqual(personContract, _target.PersonContract);
		}

		[Test]
		public void VerifyCanDeletePersonSkill()
		{
			IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill("test skill", 1);
			_target.AddPersonSkill(personSkill);

			Assert.AreEqual(1, _target.PersonSkillCollection.Count());

			_target.DeletePersonSkill(personSkill);

			Assert.AreEqual(0, _target.PersonSkillCollection.Count());
		}

		[Test]
		public void VerifyCanAddLogin()
		{
			IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
			_target.AddExternalLogOn(login);

			Assert.AreEqual(1,_target.ExternalLogOnCollection.Count());
			Assert.IsTrue(_target.ExternalLogOnCollection.Contains(login));
		}

		[Test]
		public void VerifyCanRemoveLogin()
		{
			IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
			_target.AddExternalLogOn(login);

			Assert.AreEqual(1, _target.ExternalLogOnCollection.Count());
			Assert.IsTrue(_target.ExternalLogOnCollection.Contains(login));

			_target.RemoveExternalLogOn(login);

			Assert.AreEqual(0, _target.ExternalLogOnCollection.Count());
		}

		[Test]
		public void VerifyRemoveLoginCannotTakeNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target.RemoveExternalLogOn(null));
		}

		[Test]
		public void VerifyAddLoginCannotTakeNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target.AddExternalLogOn(null));
		}

		[Test]
		public void VerifyCannotRemoveLogin()
		{
			IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
			_target.RemoveExternalLogOn(login);

			Assert.AreEqual(0, _target.ExternalLogOnCollection.Count());
		}

		[Test]
		public void VerifyCannotAddDoubleLogins()
		{
			IExternalLogOn login = ExternalLogOnFactory.CreateExternalLogOn();
			_target.AddExternalLogOn(login);
			_target.AddExternalLogOn(login);

			Assert.AreEqual(1, _target.ExternalLogOnCollection.Count());
		}

		[Test]
		public void CanClone()
		{
			_target.ResetPersonSkill();
			_target.ResetExternalLogOn();
			_target.AddPersonSkill(PersonSkillFactory.CreatePersonSkill("TestSkill", 1));
			_target.AddExternalLogOn(ExternalLogOnFactory.CreateExternalLogOn());
			_target.SetId(Guid.NewGuid());

			IPersonPeriod personPeriodClone = (IPersonPeriod)_target.Clone();
			Assert.IsFalse(personPeriodClone.Id.HasValue);
			Assert.AreEqual(_target.PersonSkillCollection.Count(), personPeriodClone.PersonSkillCollection.Count());
			Assert.AreSame(_target, _target.PersonSkillCollection.First().Parent);
			Assert.AreSame(personPeriodClone, personPeriodClone.PersonSkillCollection.First().Parent);
			Assert.AreEqual(_target.ExternalLogOnCollection.Count(), personPeriodClone.ExternalLogOnCollection.Count());
			Assert.AreEqual(_target.ExternalLogOnCollection.First(), personPeriodClone.ExternalLogOnCollection.First());
			Assert.AreEqual(_target.Note, personPeriodClone.Note);
			Assert.AreEqual(_target.Period, personPeriodClone.Period);
			Assert.AreNotEqual(_target.PersonContract, personPeriodClone.PersonContract);
			Assert.AreEqual(_target.PersonContract.Contract, personPeriodClone.PersonContract.Contract);
			Assert.AreEqual(_target.PersonContract.ContractSchedule, personPeriodClone.PersonContract.ContractSchedule);
			Assert.AreEqual(_target.PersonContract.PartTimePercentage, personPeriodClone.PersonContract.PartTimePercentage);
			Assert.AreEqual(_target.RuleSetBag, personPeriodClone.RuleSetBag);
			Assert.AreEqual(_target.StartDate, personPeriodClone.StartDate);
			Assert.AreEqual(_target.Team, personPeriodClone.Team);

			personPeriodClone = _target.NoneEntityClone();
			Assert.IsFalse(personPeriodClone.Id.HasValue);
			Assert.AreEqual(_target.PersonSkillCollection.Count(), personPeriodClone.PersonSkillCollection.Count());
			Assert.AreSame(_target, _target.PersonSkillCollection.First().Parent);
			Assert.AreSame(personPeriodClone, personPeriodClone.PersonSkillCollection.First().Parent);
			Assert.AreEqual(_target.ExternalLogOnCollection.Count(), personPeriodClone.ExternalLogOnCollection.Count());
			Assert.AreEqual(_target.ExternalLogOnCollection.First(), personPeriodClone.ExternalLogOnCollection.First());
			Assert.AreEqual(_target.Note, personPeriodClone.Note);
			Assert.AreEqual(_target.Period, personPeriodClone.Period);
			Assert.AreNotEqual(_target.PersonContract, personPeriodClone.PersonContract);
			Assert.AreEqual(_target.PersonContract.Contract, personPeriodClone.PersonContract.Contract);
			Assert.AreEqual(_target.PersonContract.ContractSchedule, personPeriodClone.PersonContract.ContractSchedule);
			Assert.AreEqual(_target.PersonContract.PartTimePercentage, personPeriodClone.PersonContract.PartTimePercentage);
			Assert.AreEqual(_target.RuleSetBag, personPeriodClone.RuleSetBag);
			Assert.AreEqual(_target.StartDate, personPeriodClone.StartDate);
			Assert.AreEqual(_target.Team, personPeriodClone.Team);

			personPeriodClone = _target.EntityClone();
			Assert.AreEqual(_target.Id.Value, personPeriodClone.Id.Value);
			Assert.AreEqual(_target.PersonSkillCollection.Count(), personPeriodClone.PersonSkillCollection.Count());
			Assert.AreSame(_target, _target.PersonSkillCollection.First().Parent);
			Assert.AreSame(personPeriodClone, personPeriodClone.PersonSkillCollection.First().Parent);
			Assert.AreEqual(_target.ExternalLogOnCollection.Count(), personPeriodClone.ExternalLogOnCollection.Count());
			Assert.AreEqual(_target.ExternalLogOnCollection.First(), personPeriodClone.ExternalLogOnCollection.First());
			Assert.AreEqual(_target.Note, personPeriodClone.Note);
			Assert.AreEqual(_target.Period, personPeriodClone.Period);
			Assert.AreNotEqual(_target.PersonContract, personPeriodClone.PersonContract);
			Assert.AreEqual(_target.PersonContract.Contract, personPeriodClone.PersonContract.Contract);
			Assert.AreEqual(_target.PersonContract.ContractSchedule, personPeriodClone.PersonContract.ContractSchedule);
			Assert.AreEqual(_target.PersonContract.PartTimePercentage, personPeriodClone.PersonContract.PartTimePercentage);
			Assert.AreEqual(_target.RuleSetBag, personPeriodClone.RuleSetBag);
			Assert.AreEqual(_target.StartDate, personPeriodClone.StartDate);
			Assert.AreEqual(_target.Team, personPeriodClone.Team);
		}

		[Test]
		public void VerifyResetPersonSkill()
		{
			IPersonSkill skill = PersonSkillFactory.CreatePersonSkill("Test", 1);
			
			_target.AddPersonSkill(skill);
			_target.ResetPersonSkill();
			Assert.AreEqual(0, _target.PersonSkillCollection.Count());
		}

		/// <summary>
		/// Verifies the reset external log on.
		/// </summary>
		[Test]
		public void VerifyResetExternalLogOn()
		{
			IExternalLogOn externalLogOn = ExternalLogOnFactory.CreateExternalLogOn();
			_target.AddExternalLogOn(externalLogOn);
			_target.ResetExternalLogOn();
			Assert.AreEqual(0, _target.ExternalLogOnCollection.Count());
		}

		[Test]
		public void VerifyCanSetNote()
		{
			const string note = "Mage Note Eka";
			_target.Note = note;
			Assert.AreEqual(note, _target.Note);
		}

		[Test]
		public void ShouldSetBudgetGroupToPersonPeriod()
		{
			var budgetGroup = new BudgetGroup();
			_target.BudgetGroup = budgetGroup;
			Assert.AreSame(budgetGroup, _target.BudgetGroup);
		}

        [Test]
        public void ShouldThrowIfForecastSourceIsNotNonBlendSkill()
        {
            var mocks = new MockRepository();
            var skill = mocks.DynamicMock<ISkill>();
            var skillType = mocks.DynamicMock<ISkillType>();
            var personSkill = mocks.DynamicMock<IPersonSkill>();

            Expect.Call(personSkill.Skill).Return(skill);
            Expect.Call(skill.SkillType).Return(skillType);
            Expect.Call(skillType.ForecastSource).Return(ForecastSource.OutboundTelephony);
            mocks.ReplayAll();
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.AddPersonNonBlendSkill(personSkill));
            mocks.VerifyAll();
        }

		[Test]
		public void ShouldModifyStartDateIfInFarFuture()
		{
			_target = new PersonPeriod(new DateOnly(5051, 12, 5), PersonContractFactory.CreatePersonContract(), TeamFactory.CreateSimpleTeam("Team2"));
			Assert.AreEqual(_target.StartDate, new DateOnly(2059,12,30));
		}

	}
}