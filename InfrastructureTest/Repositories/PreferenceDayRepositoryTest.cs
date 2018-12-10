using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PreferenceDayRepositoryTest : RepositoryTest<IPreferenceDay>
	{

		private IPerson _person;
		private DateOnly _dateOnly;
		private IActivity _activity;

		protected override void ConcreteSetup()
		{
			_person = PersonFactory.CreatePerson();
			_dateOnly = new DateOnly(2009, 2, 2);

			_activity = new Activity("'aå");

			PersistAndRemoveFromUnitOfWork(_activity);
			PersistAndRemoveFromUnitOfWork(_person);
		}

		[Test]
		public void CanFindPreferenceDaysBetweenDatesAndOnPersons()
		{
			DateOnlyPeriod period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(new DateOnly(2009, 2, 2), _person, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(new DateOnly(2009, 3, 2), _person, _activity));
			IEnumerable<IPerson> persons = new Collection<IPerson> { _person };

			IList<IPreferenceDay> days = new PreferenceDayRepository(CurrUnitOfWork).Find(period, persons);
			Assert.AreEqual(2, days.Count);
			LazyLoadingManager.IsInitialized(days[0].Restriction.ActivityRestrictionCollection).Should().Be.True();
		}

		[Test]
		public void CannotFindPreferenceDaysWhenEmptyPersonCollection()
		{
			DateOnlyPeriod period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
			IEnumerable<IPerson> persons = new Collection<IPerson>();

			IList<IPreferenceDay> days = new PreferenceDayRepository(CurrUnitOfWork).Find(period, persons);
			Assert.AreEqual(0, days.Count);
		}

		[Test]
		public void CanFindPreferenceDaysOnDayAndPerson()
		{
			DateOnly date = new DateOnly(2009, 2, 1);
			IPerson person2 = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date, _person, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date.AddDays(1), _person, _activity));

			IList<IPreferenceDay> days = new PreferenceDayRepository(CurrUnitOfWork).Find(date, _person);
			Assert.AreEqual(1, days.Count);
			Assert.AreEqual(1, days[0].Restriction.ActivityRestrictionCollection.Count);
			Assert.AreEqual(_activity, days[0].Restriction.ActivityRestrictionCollection[0].Activity);
			Assert.AreEqual(TimeSpan.FromHours(11), ((RestrictionBase)days[0].Restriction.ActivityRestrictionCollection[0]).StartTimeLimitation.StartTime);
		}

		[Test]
		public void CanFindAndLockPreferenceDaysOnDayAndPerson()
		{
			DateOnly date = new DateOnly(2009, 2, 1);
			IPerson person2 = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date, _person, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date.AddDays(1), _person, _activity));

			IList<IPreferenceDay> days = new PreferenceDayRepository(CurrUnitOfWork).FindAndLock(date, _person);
			Assert.AreEqual(1, days.Count);
			Assert.AreEqual(1, days[0].Restriction.ActivityRestrictionCollection.Count);
			Assert.AreEqual(_activity, days[0].Restriction.ActivityRestrictionCollection[0].Activity);
			Assert.AreEqual(TimeSpan.FromHours(11), ((RestrictionBase)days[0].Restriction.ActivityRestrictionCollection[0]).StartTimeLimitation.StartTime);
		}

		[Test]
		public void ShouldAddMustHaveWhenUnderLimit()
		{
			var date = new DateOnly(2009, 2, 1);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriodAndMustHave(_person, date);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDayWithoutMustHave(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDayWithoutMustHave(date.AddDays(1), person2, _activity));

			var repository = new PreferenceDayRepository(CurrUnitOfWork);
			var result = new MustHaveRestrictionSetter(repository).SetMustHave(date, person2, true);
			Assert.AreEqual(true, result);
		}

		[Test]
		public void ShouldNotAddMustHaveWhenOverLimit()
		{
			var date = new DateOnly(2009, 2, 1);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriodAndMustHave(_person, date);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDayWithoutMustHave(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date.AddDays(1), person2, _activity));

			var repository = new PreferenceDayRepository(CurrUnitOfWork);
			var result = new MustHaveRestrictionSetter(repository).SetMustHave(date, person2, true);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldNotAddMustHaveWhenPreferenceEmpty()
		{
			var date = new DateOnly(2009, 2, 1);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriodAndMustHave(_person, date);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDayWithoutMustHave(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date.AddDays(1), person2, _activity));

			var repository = new PreferenceDayRepository(CurrUnitOfWork);
			var result = new MustHaveRestrictionSetter(repository).SetMustHave(date.AddDays(3), person2, true);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldRemoveMustHave()
		{
			var date = new DateOnly(2009, 2, 1);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriodAndMustHave(_person, date);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date, person2, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(date.AddDays(1), person2, _activity));

			var repository = new PreferenceDayRepository(CurrUnitOfWork);
			var result = new MustHaveRestrictionSetter(repository).SetMustHave(date, person2, false);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			IPreferenceDay preferenceDay = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(preferenceDay);

			IPreferenceDay loaded = new PreferenceDayRepository(CurrUnitOfWork).LoadAggregate(preferenceDay.Id.Value);
			Assert.AreEqual(preferenceDay.Id, loaded.Id);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.Person));
		}

		protected override IPreferenceDay CreateAggregateWithCorrectBusinessUnit()
		{
			PreferenceDay preferenceDay = CreatePreferenceDay(_dateOnly, _person, _activity);
			_dateOnly = _dateOnly.AddDays(1);
			return preferenceDay;
		}

		private static PreferenceDay CreatePreferenceDay(DateOnly date, IPerson person, IActivity activity)
		{
			var preferenceDay = CreatePreferenceDayWithoutMustHave(date, person, activity);
			preferenceDay.Restriction.MustHave = true;
			return preferenceDay;
		}

		private static PreferenceDay CreatePreferenceDayWithoutMustHave(DateOnly date, IPerson person, IActivity activity)
		{
			PreferenceRestriction preferenceRestrictionNew = new PreferenceRestriction();
			PreferenceDay preferenceDay = new PreferenceDay(person, date, preferenceRestrictionNew);
			ActivityRestriction activityRestriction = new ActivityRestriction(activity);
			activityRestriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0));
			preferenceDay.Restriction.AddActivityRestriction(activityRestriction);
			preferenceDay.TemplateName = "My template";

			return preferenceDay;
		}

		protected override void VerifyAggregateGraphProperties(IPreferenceDay loadedAggregateFromDatabase)
		{
			IPreferenceDay org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
			Assert.AreEqual(1, loadedAggregateFromDatabase.Restriction.ActivityRestrictionCollection.Count);
			Assert.AreEqual(_activity.Name, loadedAggregateFromDatabase.Restriction.ActivityRestrictionCollection[0].Activity.Name);
			Assert.AreEqual(org.TemplateName, loadedAggregateFromDatabase.TemplateName);
		}

		protected override Repository<IPreferenceDay> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PreferenceDayRepository(currentUnitOfWork);
		}

		[Test]
		public void CanFindPreferenceDaysNewerThan()
		{
			var newerThan = DateTime.UtcNow.AddHours(-1);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(new DateOnly(2013, 2, 2), _person, _activity));
			PersistAndRemoveFromUnitOfWork(CreatePreferenceDay(new DateOnly(2013, 3, 2), _person, _activity));
			
			var days = new PreferenceDayRepository(CurrUnitOfWork).FindNewerThan(newerThan);
			Assert.AreEqual(3, days.Count);
			LazyLoadingManager.IsInitialized(days[0].Restriction.ActivityRestrictionCollection).Should().Be.True();
		}
	}
}
