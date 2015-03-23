using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class IntradaySchedulePreferenceTransformerTest
	{
		private IIntradaySchedulePreferenceTransformer _target;
		private IScenario _scenario;
		private ICommonStateHolder _stateHolder;

		[SetUp]
		public void Setup()
		{
			_target = new IntradaySchedulePreferenceTransformer();
			_scenario = new Scenario("name");
			_scenario.SetId(Guid.NewGuid());
			_stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
		}

		[Test]
		public void VerifyTransform()
		{
			var scheduleParts = SchedulePartFactory.CreateSchedulePartCollection();
			var schedulePart = scheduleParts[0];

			IActivity activity = new Activity("Main");
			activity.SetId(Guid.NewGuid());
			var person = schedulePart.Person;
			IShiftCategory shiftCategory = new ShiftCategory("TopCat");
			shiftCategory.SetId(Guid.NewGuid());
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"));
			dayOffTemplate.SetId(Guid.NewGuid());

			IPreferenceRestriction dayRestriction = new PreferenceRestriction
			{
				//Add timezone compensation to make the test runnable on all machines(independent of local timezone)
				StartTimeLimitation =
					 new StartTimeLimitation(
					 new TimeSpan(6, 0, 0),
					 new TimeSpan(20, 0, 0)),
				EndTimeLimitation =
				new EndTimeLimitation(
					 new TimeSpan(6, 0, 0),
					 new TimeSpan(20, 0, 0)),
				ShiftCategory = shiftCategory,
				DayOffTemplate = dayOffTemplate

			};
			dayRestriction.SetId(Guid.NewGuid());
			dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction = new PreferenceDay(person, schedulePart.DateOnlyAsPeriod.DateOnly, dayRestriction);
			personRestriction.SetId(Guid.NewGuid());
			var restrictions = new List<IPreferenceDay> { personRestriction };
			//var schedule = (Schedule)schedulePart;
			//schedule.Add(personRestriction);

			_stateHolder.Stub(x => x.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> { person });
			_stateHolder.Stub(x => x.GetSchedulePartOnPersonAndDate(person, personRestriction.RestrictionDate, _scenario)).Return(schedulePart);
			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(restrictions, table, _stateHolder, _scenario);
				Assert.AreEqual(1, table.Rows.Count);
			}
		}

		[Test]
		public void VerifyNoPreferencesExist()
		{
			IPreferenceRestriction preference = null;
			Assert.IsFalse(_target.CheckIfPreferenceIsValid(preference));
		}

		[Test]
		public void ShouldJustTakeOneDayIfMoreThanOneOnSameDaySamePerson()
		{
			var scheduleParts = SchedulePartFactory.CreateSchedulePartCollection();
			var schedulePart = scheduleParts[0];

			IActivity activity = new Activity("Main");
			activity.SetId(Guid.NewGuid());
			var person = schedulePart.Person;

			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
			dayOffTemplate.SetId(Guid.NewGuid());

			IPreferenceRestriction dayRestriction = new PreferenceRestriction();
			dayRestriction.SetId(Guid.NewGuid());
			dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction = new PreferenceDay(person, schedulePart.DateOnlyAsPeriod.DateOnly, dayRestriction);
			personRestriction.SetId(Guid.NewGuid());

			IPreferenceRestriction dayRestriction2 = new PreferenceRestriction();
			dayRestriction2.SetId(Guid.NewGuid());
			dayRestriction2.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction2 = new PreferenceDay(person, schedulePart.DateOnlyAsPeriod.DateOnly, dayRestriction2);
			personRestriction.SetId(Guid.NewGuid());

			var restrictions = new List<IPreferenceDay> { personRestriction, personRestriction2 };

			_stateHolder.Stub(x => x.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> { person });
			_stateHolder.Stub(x => x.GetSchedulePartOnPersonAndDate(person, personRestriction.RestrictionDate, _scenario)).Return(schedulePart);
			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(restrictions, table, _stateHolder, _scenario);
				Assert.AreEqual(1, table.Rows.Count);
			}
		}

		[Test]
		public void ShouldJustTakeBothIfMoreThanOneOnSameDateDifferentPersons()
		{
			var scheduleParts = SchedulePartFactory.CreateSchedulePartCollection();
			var schedulePart = scheduleParts[0];
			var schedulePart2 = scheduleParts[1];

			IActivity activity = new Activity("Main");
			activity.SetId(Guid.NewGuid());
			var person = schedulePart.Person;
			var person2 = schedulePart2.Person;

			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
			dayOffTemplate.SetId(Guid.NewGuid());

			IPreferenceRestriction dayRestriction = new PreferenceRestriction();
			dayRestriction.SetId(Guid.NewGuid());
			dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction = new PreferenceDay(person, schedulePart.DateOnlyAsPeriod.DateOnly, dayRestriction);
			personRestriction.SetId(Guid.NewGuid());

			IPreferenceRestriction dayRestriction2 = new PreferenceRestriction();
			dayRestriction2.SetId(Guid.NewGuid());
			dayRestriction2.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction2 = new PreferenceDay(person2, schedulePart2.DateOnlyAsPeriod.DateOnly, dayRestriction2);
			personRestriction.SetId(Guid.NewGuid());

			var restrictions = new List<IPreferenceDay> { personRestriction, personRestriction2 };

			_stateHolder.Stub(x => x.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> { person });
			_stateHolder.Stub(x => x.GetSchedulePartOnPersonAndDate(person, personRestriction.RestrictionDate, _scenario)).Return(schedulePart);
			_stateHolder.Stub(x => x.GetSchedulePartOnPersonAndDate(person2, personRestriction2.RestrictionDate, _scenario)).Return(schedulePart2);
			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(restrictions, table, _stateHolder, _scenario);
				Assert.AreEqual(2, table.Rows.Count);
			}
		}
	}
}
