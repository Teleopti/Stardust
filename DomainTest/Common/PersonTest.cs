using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonTest
    {
        private IPerson _target;
        private MockRepository _mockRepository;
        private IPersonAccountUpdater _personAccountUpdater;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _personAccountUpdater = _mockRepository.StrictMock<IPersonAccountUpdater>();
            _target = new Person();
        }

	    [Test]
	    public void AddingNoteShouldBeTrimmed()
	    {
		    var person = PersonFactory.CreatePerson();
		    person.Note = "   hej   ";
			Assert.AreEqual("hej", person.Note);
			person.Note = "         ";
			Assert.IsTrue(string.IsNullOrEmpty(person.Note));
	    }

        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.AreEqual(new Name(), _target.Name);
            Assert.AreEqual(string.Empty, _target.Email);
            Assert.AreEqual(string.Empty, _target.Note);
            Assert.AreEqual(string.Empty, _target.EmploymentNumber);
            Assert.IsNotNull(_target.PermissionInformation);
            Assert.IsFalse(_target is IBelongsToBusinessUnit);
            Assert.AreSame(_target, _target.PermissionInformation.BelongsTo);
            Assert.AreEqual(0, _target.PersonPeriodCollection.Count());
            Assert.IsFalse(_target.TerminalDate.HasValue);
            Assert.AreEqual(0, _target.PersonSchedulePeriodCollection.Count);
           Assert.AreSame(_target, _target.PersonWriteProtection.BelongsTo);
        }

        [Test]
        public void CanSetProperties()
        {
            Name name = new Name("Roger", "M����r");
            string email = "roger@foo.bar";
            string note = "�h en s� flink agent!";
            string employmentNumber = "123";
            DateOnly date = new DateOnly(2059, 12, 31);
            _target.Name = name;
            _target.Email = email;
            _target.Note = note;
            _target.EmploymentNumber = employmentNumber;
            _target.TerminatePerson(date, _personAccountUpdater);
            Assert.AreEqual(date, _target.TerminalDate);
            Assert.AreEqual(name, _target.Name);
            Assert.AreEqual(email, _target.Email);
            Assert.AreEqual(note, _target.Note);
            Assert.AreEqual(employmentNumber, _target.EmploymentNumber);
        }

        [Test]
        public void CanAddPersonPeriod()
        {
            DateOnly date = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
            ITeam team = TeamFactory.CreateSimpleTeam();

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

            ISkill skill = SkillFactory.CreateSkill("test skill");
            IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);

			_target.AddPersonPeriod(personPeriod);
            _target.AddSkill(personSkill,personPeriod);

            Assert.IsTrue(_target.PersonPeriodCollection.Contains(personPeriod));
        }

        [Test]
        public void CannotAddPersonPeriod()
        {
            DateOnly date = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
            ITeam team = TeamFactory.CreateSimpleTeam();

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

            ISkill skill = SkillFactory.CreateSkill("test skill");
            IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);

			_target.AddPersonPeriod(personPeriod);
            _target.AddSkill(personSkill,personPeriod);

            Assert.IsTrue(_target.PersonPeriodCollection.Contains(personPeriod));
            Assert.AreEqual(1, _target.PersonPeriodCollection.Count);
            _target.AddPersonPeriod(personPeriod);
            Assert.AreEqual(1, _target.PersonPeriodCollection.Count);
        }

        [Test]
        public void CanRemovePersonPeriod()
        {
            DateOnly date = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
            ITeam team = TeamFactory.CreateSimpleTeam();

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

            ISkill skill = SkillFactory.CreateSkill("test skill");
            IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);

			_target.AddPersonPeriod(personPeriod);
            _target.AddSkill(personSkill, personPeriod);

            Assert.IsTrue(_target.PersonPeriodCollection.Contains(personPeriod));
            _target.DeletePersonPeriod(personPeriod);
            Assert.IsFalse(_target.PersonPeriodCollection.Contains(personPeriod));
        }

        [Test]
        public void CanFindMyTeam()
        {
            Assert.IsNull(_target.MyTeam(DateOnly.Today));

            DateOnly date = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract","Testing","Test1");
            ITeam team = TeamFactory.CreateSimpleTeam();
            ISite site = SiteFactory.CreateSimpleSite("Site");
            site.AddTeam(team);

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);
			_target.AddPersonPeriod(personPeriod); 

			ISkill skill = SkillFactory.CreateSkill("test skill");
            IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);

            _target.AddSkill(personSkill,personPeriod);

            Assert.AreSame(team, _target.MyTeam(DateOnly.Today));
        }

        [Test]
        public void VerifyRemoveAllPersonPeriods()
        {
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(10));
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(100));

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            _target.AddPersonPeriod(personPeriod3);

            Assert.AreEqual(3, _target.PersonPeriodCollection.Count());

            _target.RemoveAllPersonPeriods();

            Assert.AreEqual(0, _target.PersonPeriodCollection.Count());
        }

		[Test]
		public void ShouldSetNextAvailableDateForPersonPeriodOnDateChange()
		{
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(1));
			IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(10));

			_target.AddPersonPeriod(personPeriod1);
			_target.AddPersonPeriod(personPeriod2);
			_target.AddPersonPeriod(personPeriod3);

			_target.ChangePersonPeriodStartDate(DateOnly.Today,personPeriod3);

			personPeriod3.StartDate.Should().Be.EqualTo(DateOnly.Today.AddDays(2));
		}

        [Test]
        public void VerifyRemoveAllSchedulePeriods()
        {
            ISchedulePeriod schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(DateOnly.Today);
            ISchedulePeriod schedulePeriod2 =
                SchedulePeriodFactory.CreateSchedulePeriod(DateOnly.Today.AddDays(10));
            ISchedulePeriod schedulePeriod3 =
                SchedulePeriodFactory.CreateSchedulePeriod(DateOnly.Today.AddDays(100));

            _target.AddSchedulePeriod(schedulePeriod1);
            _target.AddSchedulePeriod(schedulePeriod2);
            _target.AddSchedulePeriod(schedulePeriod3);

            Assert.AreEqual(3, _target.PersonSchedulePeriodCollection.Count);

            _target.RemoveAllSchedulePeriods();

            Assert.AreEqual(0, _target.PersonSchedulePeriodCollection.Count);
        }

        [Test]
        public void CanAddSchedulePeriod()
        {
            SchedulePeriod period = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Day, 4);
            _target.AddSchedulePeriod(period);
            Assert.AreEqual(1, _target.PersonSchedulePeriodCollection.Count);
        }

        [Test]
        public void VerifyCanRemoveSchedulePeriod()
        {
            SchedulePeriod period = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Day, 4);
            _target.AddSchedulePeriod(period);
            Assert.AreEqual(1, _target.PersonSchedulePeriodCollection.Count);

            _target.RemoveSchedulePeriod(period);
            Assert.AreEqual(0, _target.PersonSchedulePeriodCollection.Count);
        }

        [Test]
        public void CanReturnPeriod()
        {
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 2), team1);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), team2);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), team3);

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            _target.AddPersonPeriod(personPeriod3);

            Assert.AreSame(personPeriod1, _target.Period(new DateOnly(2001, 1, 2)));
            Assert.AreSame(personPeriod2, _target.Period(new DateOnly(1998, 1, 1)));
            Assert.AreSame(personPeriod3, _target.Period(new DateOnly(2009, 12, 31)));
            Assert.IsNull(_target.Period(new DateOnly(1007, 12, 4)));
        }

        [Test]
        public void PersonPeriodCollectionDoesNotReturnPeriodsAfterTerminationDate()
        {
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), team1);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), team1);

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            Assert.AreEqual(2, _target.PersonPeriodCollection.Count());

            _target.TerminatePerson(new DateOnly(2002, 1, 1), _personAccountUpdater);

            Assert.AreEqual(1, _target.PersonPeriodCollection.Count());
        }

		[Test]
		public void PersonPeriodCollectionDoesNotReturnPeriodsAfterTerminationDate2()
		{
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), team1);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), team1);

			_target.AddPersonPeriod(personPeriod1);
			_target.AddPersonPeriod(personPeriod2);

			_target.TerminatePerson(new DateOnly(2002, 1, 2), _personAccountUpdater);

			Assert.AreEqual(_target.TerminalDate.Value, _target.PersonPeriodCollection[1].EndDate());
		}

        [Test]
        public void CanReturnSchedulePeriod()
        {
            SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
            SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);

            _target.AddSchedulePeriod(period1);
            _target.AddSchedulePeriod(period2);

            Assert.AreEqual(period1, _target.SchedulePeriod(new DateOnly(2005, 1, 1)));
            Assert.AreEqual(period2, _target.SchedulePeriod(new DateOnly(2007, 1, 1)));
            Assert.IsNotNull(_target.VirtualSchedulePeriod(new DateOnly(2007, 1, 1)));
        }

		 [Test]
		 public void ShouldReturnNextComingVirtualPeriodIfNotExists()
		 {
		 	var period = new SchedulePeriod(new DateOnly(2020, 1, 1), SchedulePeriodType.Day, 4);
		 	_target.AddSchedulePeriod(period);

		 	_target.VirtualSchedulePeriodOrNext(new DateOnly(2010, 1, 1))
		 		.DateOnlyPeriod.StartDate.Should().Be.EqualTo(new DateOnly(2020, 1, 1));
		 }

		 [Test]
		 public void ShouldReturnCurrentIfVirtualPeriodExists()
		 {
			 var period = new SchedulePeriod(new DateOnly(2020, 1, 1), SchedulePeriodType.Day, 4);
			 _target.AddSchedulePeriod(period);

			 _target.VirtualSchedulePeriodOrNext(new DateOnly(2020, 1, 1))
				 .DateOnlyPeriod.StartDate.Should().Be.EqualTo(new DateOnly(2020, 1, 1));
		 }

        [Test]
        public void SchedulePeriodShouldBeNullIfRequestedDateIsBeforeFirstPeriod()
        {
            SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
            SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);

            _target.AddSchedulePeriod(period1);
            _target.AddSchedulePeriod(period2);

            Assert.IsNull(_target.SchedulePeriod(new DateOnly(2004, 1, 1)));
        }

        [Test]
        public void CanGetNextPeriod()
        {
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team1);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 1), team3);

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            _target.AddPersonPeriod(personPeriod3);

            Assert.AreSame(personPeriod2, _target.NextPeriod(personPeriod1));
            Assert.IsNull(_target.NextPeriod(personPeriod3));
        }

        [Test]
        public void ShouldGetPreviousPeriod()
        {
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

            var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team1);
            var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
            var personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 1), team3);

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            _target.AddPersonPeriod(personPeriod3);

            Assert.AreSame(personPeriod2, _target.PreviousPeriod(personPeriod3));
            Assert.IsNull(_target.PreviousPeriod(personPeriod1));       
        }

        [Test]
        public void CanGetNextSchedulePeriod()
        {
            SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
            SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);

            _target.AddSchedulePeriod(period1);
            _target.AddSchedulePeriod(period2);

            Assert.AreEqual(period2, _target.NextSchedulePeriod(period1));
            Assert.IsNull(_target.NextSchedulePeriod(period2));
        }

        /// <summary>
        /// Determines whether this instance [can return person periods in time period].
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-24
        /// </remarks>
        [Test]
        public void VerifyPersonPeriods()
        {
            ITeam teamA = TeamFactory.CreateSimpleTeam("TeamA");
            ITeam teamB = TeamFactory.CreateSimpleTeam("TeamB");
            ITeam teamC = TeamFactory.CreateSimpleTeam("TeamC");

            IPersonPeriod personPeriodA1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 2), teamA);
            IPersonPeriod personPeriodB = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), teamB);
            IPersonPeriod personPeriodC = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), teamC);
            IPersonPeriod personPeriodA2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2003, 6, 1), teamA);

            _target.AddPersonPeriod(personPeriodA1);
            _target.AddPersonPeriod(personPeriodB);
            _target.AddPersonPeriod(personPeriodC);
            _target.AddPersonPeriod(personPeriodA2);

            IList<IPersonPeriod> personPeriodCollection;

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(4, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(1996, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(1, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(1992, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(0, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1996, 1, 1),new DateOnly(1997, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(1, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1998, 1, 1),new DateOnly(2020, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(4, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2001, 1, 1),new DateOnly(2020, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(3, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2003, 1, 1),new DateOnly(2020, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(2, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2004, 1, 1),new DateOnly(2020, 1, 1));
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(1, personPeriodCollection.Count);

            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2002, 1, 1),new DateOnly(2020, 1, 1));
            _target.TerminatePerson(new DateOnly(2001, 1, 1), _personAccountUpdater);
            personPeriodCollection = _target.PersonPeriods(dateOnlyPeriod);
            Assert.AreEqual(0, personPeriodCollection.Count);

            Assert.IsNull(_target.Period(new DateOnly(2001, 1, 2)));
        }

        [Test]
        public void CanReturnPersonSchedulePeriodsInTimePeriod()
        {
            SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
            SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);
            SchedulePeriod period3 = new SchedulePeriod(new DateOnly(2007, 1, 1), SchedulePeriodType.Month, 1);

            _target.AddSchedulePeriod(period1);
            _target.AddSchedulePeriod(period2);
            _target.AddSchedulePeriod(period3);

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2005, 10, 10), new DateOnly(2006, 10, 10));
            IList<ISchedulePeriod> personScheduleCollection = _target.PersonSchedulePeriods(dateOnlyPeriod);

            Assert.AreEqual(2, personScheduleCollection.Count);
            Assert.IsTrue(personScheduleCollection.Contains(period1));
            Assert.IsTrue(personScheduleCollection.Contains(period2));

            _target.TerminatePerson(new DateOnly(2005, 1, 1), _personAccountUpdater);
            dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2005, 10, 10), new DateOnly(2006, 10, 10));
            personScheduleCollection = _target.PersonSchedulePeriods(dateOnlyPeriod);
            Assert.AreEqual(0, personScheduleCollection.Count);
            Assert.IsNull(_target.SchedulePeriod(new DateOnly(2005, 1, 2)));
        }

        /// <summary>
        /// Verifies the person period collection is locked.
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-02-01
        /// </remarks>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifyPersonPeriodCollectionIsLocked()
        {
            Team team = TeamFactory.CreateSimpleTeam("Team1");
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 2), team);

            _target.PersonPeriodCollection.Add(personPeriod);
        }


        [Test]
        public void VerifyIsAgent()
        {
            DateOnly date = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
            ITeam team = TeamFactory.CreateSimpleTeam();

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

            _target.AddPersonPeriod(personPeriod);

            Assert.IsFalse(_target.IsAgent(date.AddDays(-1)));
            Assert.IsTrue(_target.IsAgent(date.AddDays(1)));
        }

        [Test]
        public void VerifySeniority()
        {
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), team1);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1), team2);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 1), team3);

            _target.AddPersonPeriod(personPeriod1);
            _target.AddPersonPeriod(personPeriod2);
            _target.AddPersonPeriod(personPeriod3);
            _target.TerminatePerson(new DateOnly(2009, 1, 8), _personAccountUpdater);

            Assert.AreEqual(109, _target.Seniority);
        }

        [Test]
        public void VerifyPeriodWithTerminalDate()
        {
            ITeam teamA = TeamFactory.CreateSimpleTeam("TeamA");

            DateOnly dateTime = new DateOnly(2000, 1, 2);

            DateOnly dateTime1 = new DateOnly(2000, 1, 1);

            IPersonPeriod personPeriodA1 = PersonPeriodFactory.CreatePersonPeriod(dateTime, teamA);

            _target.AddPersonPeriod(personPeriodA1);

            _target.TerminatePerson(dateTime, _personAccountUpdater);

            DateOnly dateOnly = new DateOnly(2000, 1, 2);
            Assert.IsNotNull(_target.Period(dateOnly));
            Assert.AreEqual(1, _target.PersonPeriods(new DateOnlyPeriod(dateTime, dateTime)).Count);

            _target.TerminatePerson(dateTime1, _personAccountUpdater);

            Assert.IsNull(_target.Period(dateOnly));
            Assert.AreEqual(0, _target.PersonPeriods(new DateOnlyPeriod(dateTime1, dateTime1)).Count);

        }

        [Test]
        public void VerifyPeriodWithTerminalDateForSchedulePeriod()
        {
            DateOnly dateTime = new DateOnly(2000, 1, 2);

            DateOnly dateTime1 = new DateOnly(2000, 1, 1);

            SchedulePeriod period1 = new SchedulePeriod(dateTime, SchedulePeriodType.Week, 4);

            _target.AddSchedulePeriod(period1);

            _target.TerminatePerson(dateTime,_personAccountUpdater);


            Assert.IsNotNull(_target.SchedulePeriod(dateTime));

            _target.TerminatePerson(dateTime1, _personAccountUpdater);

            Assert.IsNull(_target.SchedulePeriod(dateTime));
        }

        [Test]
        public void VerifyOneRotationCanReplacePreviousRotation()
        {
            _target.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            IRotation rotation1 = new Rotation("rotation1", 7);
            IRotation rotation2 = new Rotation("rotation2", 7);
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("test");
            rotation1.RotationDays[0].RestrictionCollection[0].ShiftCategory = shiftCategory;
            rotation2.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory;
            IPersonRotation personRotation1 = new PersonRotation(_target, rotation1, new DateOnly(2001, 1, 1), 0);
            IPersonRotation personRotation2 = new PersonRotation(_target, rotation2, new DateOnly(2001, 1, 8), 0);
            IList<IPersonRotation> rotations = new List<IPersonRotation>();
            rotations.Add(personRotation2);
            rotations.Add(personRotation1);

            DateOnly queryDate = new DateOnly(2001, 1, 1);
            Assert.AreEqual(shiftCategory, _target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
            queryDate = queryDate.AddDays(1);
            Assert.IsNull(_target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);

            queryDate = new DateOnly(2001, 1, 8);
            Assert.IsNull(_target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
            queryDate = queryDate.AddDays(1);
            Assert.AreEqual(shiftCategory, _target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
            
        }

        [Test]
        public void CanSetWorkflowControlSet()
        {
            Assert.IsNull(_target.WorkflowControlSet);
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("MyControlSet");
            _target.WorkflowControlSet = workflowControlSet;
            Assert.IsNotNull(_target.WorkflowControlSet);
            Assert.AreEqual(workflowControlSet, _target.WorkflowControlSet);
        }

        [Test]
        public void ShouldCheckPersonHasPersonPeriodBeforeGetContractTime()
        {
            var dateOnly = new DateOnly(2012, 12, 1);
            Assert.That(_target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void ShouldGetContractTimeFromContract()
        {
            var dateOnly = new DateOnly(2012, 12, 1);
            var team = TeamFactory.CreateSimpleTeam("Team");
            var personContract =
                new PersonContract(new Contract("contract") { WorkTimeSource = WorkTimeSource.FromContract },
                    new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
            var personPeriod = new PersonPeriod(dateOnly, personContract, team);
            _target.AddPersonPeriod(personPeriod);
            Assert.That(_target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime, Is.EqualTo(WorkTime.DefaultWorkTime.AvgWorkTimePerDay));
        }

        [Test]
        public void ShouldGetContractTimeFromSchedulePeriod()
        {
            var dateOnly = new DateOnly(2012, 12, 1);
            var team = TeamFactory.CreateSimpleTeam("Team");
            var personContract =
                new PersonContract(new Contract("contract") { WorkTimeSource = WorkTimeSource.FromSchedulePeriod },
                    new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
            var personPeriod = new PersonPeriod(dateOnly, personContract, team);
            _target.AddPersonPeriod(personPeriod);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
            schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
            _target.AddSchedulePeriod(schedulePeriod);
            Assert.That(_target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime, Is.EqualTo(TimeSpan.FromHours(6)));
        } 
        
        [Test]
        public void ShouldGetContractTimeFromVirtualSchedulePeriod()
        {
            var dateOnly = new DateOnly(2012, 7, 1);
            var team = TeamFactory.CreateSimpleTeam("Team");
            var contractSchedule = new ContractSchedule("Test1");
            var week = new ContractScheduleWeek();
            week.Add(DayOfWeek.Monday, true);
            week.Add(DayOfWeek.Tuesday, true);
            week.Add(DayOfWeek.Wednesday, true);
            week.Add(DayOfWeek.Thursday, true);
            week.Add(DayOfWeek.Friday, true);
            contractSchedule.AddContractScheduleWeek(week);
            var personContract =
                new PersonContract(new Contract("contract") { WorkTimeSource = WorkTimeSource.FromSchedulePeriod },
                    new PartTimePercentage("Testing"), contractSchedule);
       
            var personPeriod = new PersonPeriod(dateOnly, personContract, team);
            _target.AddPersonPeriod(personPeriod);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2012, 6, 1));
            schedulePeriod.PeriodType = SchedulePeriodType.Month;
            schedulePeriod.Number = 1;
            schedulePeriod.PeriodTime = new TimeSpan(0, 167, 42, 0);
            schedulePeriod.DaysOff = 12;
            _target.AddSchedulePeriod(schedulePeriod);
            Assert.That(_target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime, Is.EqualTo(TimeSpan.FromMinutes(schedulePeriod.PeriodTime.Value.TotalMinutes / 19)));
        }

	    [Test]
	    public void ShuoldPersonAccountUpdaterCalledWhenTerminatePerson()
	    {
			DateOnly dateOnly = new DateOnly();
		    MockRepository mocks = new MockRepository();
			var personAccountUpdater = mocks.StrictMock<IPersonAccountUpdater>();
	        using (mocks.Record())
		    {
			   Expect.Call(() => personAccountUpdater.Update(_target))
				   .Repeat.Once();
		    }
		    using (mocks.Playback())
		    {
				_target.TerminatePerson(dateOnly, personAccountUpdater);
		    }
	    }

		[Test]
		public void ShouldPersonAccountUpdaterCalledWhenActivatePerson()
		{
			MockRepository mocks = new MockRepository();
			var personAccountUpdater = mocks.StrictMock<IPersonAccountUpdater>();
            
			using (mocks.Record())
			{
				Expect.Call(() => personAccountUpdater.Update(_target))
					.Repeat.Once();
			}
			using (mocks.Playback())
			{
				_target.ActivatePerson(personAccountUpdater);
			}
		}
    }
}
