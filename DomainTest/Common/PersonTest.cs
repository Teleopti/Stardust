using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PersonTest
	{
		public MutableNow Now;
		public IJsonEventSerializer Serializer;
		public IJsonEventDeserializer Deserializer;

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
			var target = new Person();
			Assert.AreEqual(new Name(), target.Name);
			Assert.AreEqual(string.Empty, target.Email);
			Assert.AreEqual(string.Empty, target.Note);
			Assert.AreEqual(string.Empty, target.EmploymentNumber);
			Assert.IsNotNull(target.PermissionInformation);
			Assert.IsFalse(target is IBelongsToBusinessUnit);
			Assert.AreSame(target, target.PermissionInformation.BelongsTo);
			Assert.AreEqual(0, target.PersonPeriodCollection.Count());
			Assert.IsFalse(target.TerminalDate.HasValue);
			Assert.AreEqual(0, target.PersonSchedulePeriodCollection.Count);
			Assert.AreSame(target, target.PersonWriteProtection.BelongsTo);
		}

		[Test]
		public void CanSetProperties()
		{
			var target = new Person();
			Name name = new Name("Roger", "M����r");
			string email = "roger@foo.bar";
			string note = "�h en s� flink agent!";
			string employmentNumber = "123";
			DateOnly date = new DateOnly(2059, 12, 31);
			target.SetName(name);
			target.Email = email;
			target.Note = note;
			target.SetEmploymentNumber(employmentNumber);
			target.TerminatePerson(date, new PersonAccountUpdaterDummy());
			Assert.AreEqual(date, target.TerminalDate);
			Assert.AreEqual(name, target.Name);
			Assert.AreEqual(email, target.Email);
			Assert.AreEqual(note, target.Note);
			Assert.AreEqual(employmentNumber, target.EmploymentNumber);
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			target.AddSkill(personSkill, personPeriod);

			Assert.IsTrue(target.PersonPeriodCollection.Contains(personPeriod));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			target.AddSkill(personSkill, personPeriod);

			Assert.IsTrue(target.PersonPeriodCollection.Contains(personPeriod));
			Assert.AreEqual(1, target.PersonPeriodCollection.Count);
			target.AddPersonPeriod(personPeriod);
			Assert.AreEqual(1, target.PersonPeriodCollection.Count);
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			target.AddSkill(personSkill, personPeriod);

			Assert.IsTrue(target.PersonPeriodCollection.Contains(personPeriod));
			target.DeletePersonPeriod(personPeriod);
			Assert.IsFalse(target.PersonPeriodCollection.Contains(personPeriod));
		}

		[Test]
		public void CanFindMyTeam()
		{
			var target = new Person();
			Assert.IsNull(target.MyTeam(DateOnly.Today));

			DateOnly date = new DateOnly(2000, 1, 1);
			IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
			ITeam team = TeamFactory.CreateSimpleTeam();
			ISite site = SiteFactory.CreateSimpleSite("Site");
			site.AddTeam(team);

			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);
			target.AddPersonPeriod(personPeriod);

			ISkill skill = SkillFactory.CreateSkill("test skill");
			IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);

			target.AddSkill(personSkill, personPeriod);

			Assert.AreSame(team, target.MyTeam(DateOnly.Today));
		}

		[Test]
		public void VerifyRemoveAllPersonPeriods()
		{
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(10));
			IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(100));

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);

			Assert.AreEqual(3, target.PersonPeriodCollection.Count());

			target.RemoveAllPersonPeriods();

			Assert.AreEqual(0, target.PersonPeriodCollection.Count());
		}

		[Test]
		public void ShouldSetNextAvailableDateForPersonPeriodOnDateChange()
		{
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(1));
			IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(10));

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);

			target.ChangePersonPeriodStartDate(DateOnly.Today, personPeriod3);

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

			var target = new Person();
			target.AddSchedulePeriod(schedulePeriod1);
			target.AddSchedulePeriod(schedulePeriod2);
			target.AddSchedulePeriod(schedulePeriod3);

			Assert.AreEqual(3, target.PersonSchedulePeriodCollection.Count);

			target.RemoveAllSchedulePeriods();

			Assert.AreEqual(0, target.PersonSchedulePeriodCollection.Count);
		}

		[Test]
		public void CanAddSchedulePeriod()
		{
			var target = new Person();
			SchedulePeriod period = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Day, 4);
			target.AddSchedulePeriod(period);
			Assert.AreEqual(1, target.PersonSchedulePeriodCollection.Count);
		}

		[Test]
		public void VerifyCanRemoveSchedulePeriod()
		{
			var target = new Person();
			SchedulePeriod period = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Day, 4);
			target.AddSchedulePeriod(period);
			Assert.AreEqual(1, target.PersonSchedulePeriodCollection.Count);

			target.RemoveSchedulePeriod(period);
			Assert.AreEqual(0, target.PersonSchedulePeriodCollection.Count);
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);

			Assert.AreSame(personPeriod1, target.Period(new DateOnly(2001, 1, 2)));
			Assert.AreSame(personPeriod2, target.Period(new DateOnly(1998, 1, 1)));
			Assert.AreSame(personPeriod3, target.Period(new DateOnly(2009, 12, 31)));
			Assert.IsNull(target.Period(new DateOnly(1007, 12, 4)));
		}

		[Test]
		public void PersonPeriodCollectionDoesNotReturnPeriodsAfterTerminationDate()
		{
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), team1);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), team1);

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			Assert.AreEqual(2, target.PersonPeriodCollection.Count());

			target.TerminatePerson(new DateOnly(2002, 1, 1), new PersonAccountUpdaterDummy());

			Assert.AreEqual(1, target.PersonPeriodCollection.Count());
		}

		[Test]
		public void PersonPeriodCollectionDoesNotReturnPeriodsAfterTerminationDate2()
		{
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1995, 1, 1), team1);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 1, 2), team1);

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);

			target.TerminatePerson(new DateOnly(2002, 1, 2), new PersonAccountUpdaterDummy());

			Assert.AreEqual(target.TerminalDate.Value, target.PersonPeriodCollection[1].EndDate());
		}

		[Test]
		public void CanReturnSchedulePeriod()
		{
			SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
			SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);

			var target = new Person();
			target.AddSchedulePeriod(period1);
			target.AddSchedulePeriod(period2);

			Assert.AreEqual(period1, target.SchedulePeriod(new DateOnly(2005, 1, 1)));
			Assert.AreEqual(period2, target.SchedulePeriod(new DateOnly(2007, 1, 1)));
			Assert.IsNotNull(target.VirtualSchedulePeriod(new DateOnly(2007, 1, 1)));
		}

		[Test]
		public void ShouldReturnNextComingVirtualPeriodIfNotExists()
		{
			var period = new SchedulePeriod(new DateOnly(2020, 1, 1), SchedulePeriodType.Day, 4);

			var target = new Person();
			target.AddSchedulePeriod(period);

			target.VirtualSchedulePeriodOrNext(new DateOnly(2010, 1, 1))
				.DateOnlyPeriod.StartDate.Should().Be.EqualTo(new DateOnly(2020, 1, 1));
		}

		[Test]
		public void ShouldReturnCurrentIfVirtualPeriodExists()
		{
			var period = new SchedulePeriod(new DateOnly(2020, 1, 1), SchedulePeriodType.Day, 4);

			var target = new Person();
			target.AddSchedulePeriod(period);

			target.VirtualSchedulePeriodOrNext(new DateOnly(2020, 1, 1))
				.DateOnlyPeriod.StartDate.Should().Be.EqualTo(new DateOnly(2020, 1, 1));
		}

		[Test]
		public void SchedulePeriodShouldBeNullIfRequestedDateIsBeforeFirstPeriod()
		{
			SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
			SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);

			var target = new Person();
			target.AddSchedulePeriod(period1);
			target.AddSchedulePeriod(period2);

			Assert.IsNull(target.SchedulePeriod(new DateOnly(2004, 1, 1)));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);

			Assert.AreSame(personPeriod2, target.NextPeriod(personPeriod1));
			Assert.IsNull(target.NextPeriod(personPeriod3));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);

			Assert.AreSame(personPeriod2, target.PreviousPeriod(personPeriod3));
			Assert.IsNull(target.PreviousPeriod(personPeriod1));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriodA1);
			target.AddPersonPeriod(personPeriodB);
			target.AddPersonPeriod(personPeriodC);
			target.AddPersonPeriod(personPeriodA2);

			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1));
			IList<IPersonPeriod> personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(4, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(1996, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(1, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1990, 1, 1), new DateOnly(1992, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(0, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1996, 1, 1), new DateOnly(1997, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(1, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(1998, 1, 1), new DateOnly(2020, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(4, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2020, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(3, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2003, 1, 1), new DateOnly(2020, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(2, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2004, 1, 1), new DateOnly(2020, 1, 1));
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(1, personPeriodCollection.Count);

			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2002, 1, 1), new DateOnly(2020, 1, 1));
			target.TerminatePerson(new DateOnly(2001, 1, 1), new PersonAccountUpdaterDummy());
			personPeriodCollection = target.PersonPeriods(dateOnlyPeriod);
			Assert.AreEqual(0, personPeriodCollection.Count);

			Assert.IsNull(target.Period(new DateOnly(2001, 1, 2)));
		}

		[Test]
		public void CanReturnPersonSchedulePeriodsInTimePeriod()
		{
			SchedulePeriod period1 = new SchedulePeriod(new DateOnly(2005, 1, 1), SchedulePeriodType.Week, 4);
			SchedulePeriod period2 = new SchedulePeriod(new DateOnly(2006, 1, 1), SchedulePeriodType.Month, 1);
			SchedulePeriod period3 = new SchedulePeriod(new DateOnly(2007, 1, 1), SchedulePeriodType.Month, 1);

			var target = new Person();
			target.AddSchedulePeriod(period1);
			target.AddSchedulePeriod(period2);
			target.AddSchedulePeriod(period3);

			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2005, 10, 10), new DateOnly(2006, 10, 10));
			IList<ISchedulePeriod> personScheduleCollection = target.PersonSchedulePeriods(dateOnlyPeriod);

			Assert.AreEqual(2, personScheduleCollection.Count);
			Assert.IsTrue(personScheduleCollection.Contains(period1));
			Assert.IsTrue(personScheduleCollection.Contains(period2));

			target.TerminatePerson(new DateOnly(2005, 1, 1), new PersonAccountUpdaterDummy());
			dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2005, 10, 10), new DateOnly(2006, 10, 10));
			personScheduleCollection = target.PersonSchedulePeriods(dateOnlyPeriod);
			Assert.AreEqual(0, personScheduleCollection.Count);
			Assert.IsNull(target.SchedulePeriod(new DateOnly(2005, 1, 2)));
		}

		/// <summary>
		/// Verifies the person period collection is locked.
		/// </summary>
		/// <remarks>
		/// Created by: sumeda herath
		/// Created date: 2008-02-01
		/// </remarks>
		[Test]
		public void VerifyPersonPeriodCollectionIsLocked()
		{
			Team team = TeamFactory.CreateSimpleTeam("Team1");
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 2), team);

			var target = new Person();
			Assert.Throws<NotSupportedException>(() => target.PersonPeriodCollection.Add(personPeriod));
		}


		[Test]
		public void VerifyIsAgent()
		{
			DateOnly date = new DateOnly(2000, 1, 1);
			IPersonContract personContract = PersonContractFactory.CreatePersonContract("my first contract", "Testing", "Test1");
			ITeam team = TeamFactory.CreateSimpleTeam();

			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var target = new Person();
			target.AddPersonPeriod(personPeriod);

			Assert.IsFalse(target.IsAgent(date.AddDays(-1)));
			Assert.IsTrue(target.IsAgent(date.AddDays(1)));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod1);
			target.AddPersonPeriod(personPeriod2);
			target.AddPersonPeriod(personPeriod3);
			target.TerminatePerson(new DateOnly(2009, 1, 8), new PersonAccountUpdaterDummy());

			Assert.AreEqual(109, target.Seniority);
		}

		[Test]
		public void VerifyPeriodWithTerminalDate()
		{
			ITeam teamA = TeamFactory.CreateSimpleTeam("TeamA");

			DateOnly dateTime = new DateOnly(2000, 1, 2);

			DateOnly dateTime1 = new DateOnly(2000, 1, 1);

			IPersonPeriod personPeriodA1 = PersonPeriodFactory.CreatePersonPeriod(dateTime, teamA);

			var target = new Person();
			target.AddPersonPeriod(personPeriodA1);

			var personAccountUpdaterDummy = new PersonAccountUpdaterDummy();
			target.TerminatePerson(dateTime, personAccountUpdaterDummy);

			DateOnly dateOnly = new DateOnly(2000, 1, 2);
			Assert.IsNotNull(target.Period(dateOnly));
			Assert.AreEqual(1, target.PersonPeriods(new DateOnlyPeriod(dateTime, dateTime)).Count);

			target.TerminatePerson(dateTime1, personAccountUpdaterDummy);

			Assert.IsNull(target.Period(dateOnly));
			Assert.AreEqual(0, target.PersonPeriods(new DateOnlyPeriod(dateTime1, dateTime1)).Count);

		}

		[Test]
		public void VerifyPeriodWithTerminalDateForSchedulePeriod()
		{
			DateOnly dateTime = new DateOnly(2000, 1, 2);

			DateOnly dateTime1 = new DateOnly(2000, 1, 1);

			SchedulePeriod period1 = new SchedulePeriod(dateTime, SchedulePeriodType.Week, 4);

			var target = new Person();
			target.AddSchedulePeriod(period1);
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			target.TerminatePerson(dateTime, personAccountUpdater);

			Assert.IsNotNull(target.SchedulePeriod(dateTime));

			target.TerminatePerson(dateTime1, personAccountUpdater);

			Assert.IsNull(target.SchedulePeriod(dateTime));
		}

		[Test]
		public void VerifyOneRotationCanReplacePreviousRotation()
		{
			var target = new Person();
			IRotation rotation1 = new Rotation("rotation1", 7);
			IRotation rotation2 = new Rotation("rotation2", 7);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("test");
			rotation1.RotationDays[0].RestrictionCollection[0].ShiftCategory = shiftCategory;
			rotation2.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory;
			IPersonRotation personRotation1 = new PersonRotation(target, rotation1, new DateOnly(2001, 1, 1), 0);
			IPersonRotation personRotation2 = new PersonRotation(target, rotation2, new DateOnly(2001, 1, 8), 0);
			IList<IPersonRotation> rotations = new List<IPersonRotation> { personRotation2, personRotation1 };

			DateOnly queryDate = new DateOnly(2001, 1, 1);
			Assert.AreEqual(shiftCategory, target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
			queryDate = queryDate.AddDays(1);
			Assert.IsNull(target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);

			queryDate = new DateOnly(2001, 1, 8);
			Assert.IsNull(target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
			queryDate = queryDate.AddDays(1);
			Assert.AreEqual(shiftCategory, target.GetPersonRotationDayRestrictions(rotations, queryDate)[0].ShiftCategory);
		}

		[Test]
		public void CanSetWorkflowControlSet()
		{
			var target = new Person();
			Assert.IsNull(target.WorkflowControlSet);
			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("MyControlSet");
			target.WorkflowControlSet = workflowControlSet;
			Assert.IsNotNull(target.WorkflowControlSet);
			Assert.AreEqual(workflowControlSet, target.WorkflowControlSet);
		}

		[Test]
		public void ShouldCheckPersonHasPersonPeriodBeforeGetContractTime()
		{
			var dateOnly = new DateOnly(2012, 12, 1);
			var target = new Person();
			Assert.That(target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime.Value, Is.EqualTo(TimeSpan.Zero));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			Assert.That(target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime.Value, Is.EqualTo(WorkTime.DefaultWorkTime.AvgWorkTimePerDay));
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

			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
			schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
			target.AddSchedulePeriod(schedulePeriod);
			Assert.That(target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime.Value, Is.EqualTo(TimeSpan.FromHours(6)));
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
			var target = new Person();
			target.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2012, 6, 1));
			schedulePeriod.PeriodType = SchedulePeriodType.Month;
			schedulePeriod.Number = 1;
			schedulePeriod.PeriodTime = new TimeSpan(0, 167, 42, 0);
			schedulePeriod.DaysOff = 12;
			target.AddSchedulePeriod(schedulePeriod);
			Assert.That(target.AverageWorkTimeOfDay(dateOnly).AverageWorkTime.Value, Is.EqualTo(TimeSpan.FromMinutes(schedulePeriod.PeriodTime.Value.TotalMinutes / 19)));
		}

		[Test]
		public void ShuoldPersonAccountUpdaterCalledWhenTerminatePerson()
		{
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			var target = new Person();
			target.TerminatePerson(new DateOnly(), personAccountUpdater);
			personAccountUpdater.CallCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersonAccountUpdaterCalledWhenActivatePerson()
		{
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			var target = new Person();
			target.ActivatePerson(personAccountUpdater);
			personAccountUpdater.CallCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectVirtualSchedulePeriodWhenNextPeriodInteruptCurrent()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2015, 11, 23), SchedulePeriodType.Week, 4);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2015, 11, 30), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod1);
			person.AddSchedulePeriod(schedulePeriod2);

			var virtualSchedulePeriod = person.VirtualSchedulePeriod(new DateOnly(2015, 11, 23));

			virtualSchedulePeriod.DateOnlyPeriod.EndDate.Should().Be.EqualTo(new DateOnly(2015, 11, 29));
		}

		[Test]
		public void ShouldReturnCorrectVirtualSchedulePeriodWhenPeriodBeforeIsInterupted()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2015, 11, 23), SchedulePeriodType.Week, 4);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2015, 11, 30), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod1);
			person.AddSchedulePeriod(schedulePeriod2);

			var virtualSchedulePeriod = person.VirtualSchedulePeriod(new DateOnly(2015, 12, 1));

			virtualSchedulePeriod.DateOnlyPeriod.StartDate.Should().Be.EqualTo(new DateOnly(2015, 11, 30));
		}

		[Test]
		public void ShouldReturnCorrectAverageWorkTimePerDayWithPeriodTimeOverrideAndWorkTimeSourceFromSchedulePeriod()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var newPeriod = person.Period(DateOnly.MinValue);
			newPeriod.PersonContract.Contract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
			person.AddPersonPeriod(newPeriod);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2015, 10, 1), SchedulePeriodType.Month, 1);
			schedulePeriod1.PeriodTime = TimeSpan.FromHours(171.5);
			person.AddSchedulePeriod(schedulePeriod1);

			IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(new DateOnly(2015, 10, 30));
			virtualSchedulePeriod.AverageWorkTimePerDay.TotalHours.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldReturnCorrectDaysOffWithDaysOffOverrideAndWorkTimeSourceFromSchedulePeriod()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var newPeriod = person.Period(DateOnly.MinValue);
			newPeriod.PersonContract.Contract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
			person.AddPersonPeriod(newPeriod);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2015, 10, 1), SchedulePeriodType.Month, 1);
			schedulePeriod1.DaysOff = 8;
			person.AddSchedulePeriod(schedulePeriod1);

			IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(new DateOnly(2015, 10, 30));
			virtualSchedulePeriod.DaysOff().Should().Be.GreaterThan(0);
		}

		[Test]
		public void LastPhysicalPersonSchedulePeriodShouldBeReturned()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod1);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2011, 2, 7), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod2);
			var schedulePeriod3 = new SchedulePeriod(new DateOnly(2013, 2, 4), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod3);

			var schedulePeriods =
				person.PersonSchedulePeriods(new DateOnlyPeriod(new DateOnly(2015, 11, 30), new DateOnly(2015, 12, 13)));

			schedulePeriods.Count.Should().Be.EqualTo(1);
			schedulePeriods[0].DateFrom.Should().Be.EqualTo(new DateOnly(2013, 2, 4));
		}

		[Test]
		public void LastPhysicalPersonSchedulePeriodsShouldBeReturned()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod1);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2011, 2, 7), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod2);
			var schedulePeriod3 = new SchedulePeriod(new DateOnly(2013, 2, 4), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod3);

			var schedulePeriods =
				person.PersonSchedulePeriods(new DateOnlyPeriod(new DateOnly(2011, 2, 15), new DateOnly(2015, 12, 13)));

			schedulePeriods.Count.Should().Be.EqualTo(2);
			schedulePeriods[0].DateFrom.Should().Be.EqualTo(new DateOnly(2011, 2, 7));
			schedulePeriods[1].DateFrom.Should().Be.EqualTo(new DateOnly(2013, 2, 4));
		}

		[Test]
		public void ShouldGetSiteOpenHour()
		{
			var site = new Site("site1");
			var timePeriod = new TimePeriod(8, 10, 17, 0);
			var siteOpenHour = new SiteOpenHour()
			{
				WeekDay = DateOnly.Today.DayOfWeek,
				TimePeriod = timePeriod
			};
			site.AddOpenHour(siteOpenHour);
			var team = new Team() { Site = site };
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			person.SiteOpenHour(DateOnly.Today).TimePeriod.Should().Be.EqualTo(timePeriod);
		}

		[Test]
		public void ShouldPublishPersonPeriodChangedEventOnAddExternalLogOn()
		{
			var person = new Person();
			var period = new PersonPeriod("2016-09-26".Date(),
				new PersonContract(
					new Contract("_"), 
					new PartTimePercentage("_"), 
					new ContractSchedule("_")), 
				new Team());
			person.AddPersonPeriod(period);
			person.PopAllEvents();

			person.AddExternalLogOn(new ExternalLogOn(), period);

			person.PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishTeamChangedEventOnAddExternalLogOn()
		{
			var person = new Person();
			var team1 = new Team();
			var team2 = new Team();
			var period = new PersonPeriod("2016-09-26".Date(),
				new PersonContract(
					new Contract("_"),
					new PartTimePercentage("_"),
					new ContractSchedule("_")),
				team1);
			person.AddPersonPeriod(period);
			person.PopAllEvents();

			person.ChangeTeam(team2, period);

			person.PopAllEvents().OfType<PersonTeamChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishEventsSerializable()
		{
			Now.Is("2016-10-19 12:00");
			var person = new Person();
			var team1 = new Team();
			var team2 = new Team();
			var period = new PersonPeriod("2016-10-01".Date(),
				new PersonContract(
					new Contract("_"),
					new PartTimePercentage("_"),
					new ContractSchedule("_")),
				team1);
			person.AddPersonPeriod(period);
			person.ChangeTeam(team2, period);
			person.AddExternalLogOn(new ExternalLogOn(), period);

			var events = person.PopAllEvents();

			var serialized = events.Select(x =>
				new
				{
					e = Serializer.SerializeEvent(x),
					t = x.GetType()
				}).ToArray();

			serialized.ForEach(s =>
			{
				Assert.DoesNotThrow(() => Deserializer.DeserializeEvent(s.e, s.t));
			});
		}
	}
}
