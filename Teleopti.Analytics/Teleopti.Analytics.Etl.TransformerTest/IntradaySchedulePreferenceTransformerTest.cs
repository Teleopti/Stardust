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
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class IntradaySchedulePreferenceTransformerTest
    {
        private IIntradaySchedulePreferenceTransformer _target;
	    private MockRepository _mocks;
	    private IScenario _scenario;
	    private ICommonStateHolder _stateHolder;

	    [SetUp]
        public void Setup()
        {
            _target = new IntradaySchedulePreferenceTransformer(96);
	        _mocks = new MockRepository();
		    _scenario = new Scenario("name");
			_scenario.SetId(Guid.NewGuid());
		    _stateHolder = _mocks.DynamicMock<ICommonStateHolder>();
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
            var mainShift = new EditableShift(shiftCategory);
            mainShift.LayerCollection.Add(new EditorActivityLayer(activity, schedulePart.Period));
            IPersonAssignment assignment = new PersonAssignment(person, schedulePart.Scenario, schedulePart.DateOnlyAsPeriod.DateOnly);
            new EditableShiftMapper().SetMainShiftLayers(assignment, mainShift);
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
            IPreferenceDay personRestriction = new PreferenceDay(person, new DateOnly(schedulePart.DateOnlyAsPeriod.DateOnly), dayRestriction);
            personRestriction.SetId(Guid.NewGuid());
			var restrictions = new List<IPreferenceDay> {personRestriction};
            Schedule schedule = (Schedule)schedulePart;
            schedule.Add(assignment);
            schedule.Add(personRestriction);

			Expect.Call(_stateHolder.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> {person});
			Expect.Call(_stateHolder.GetSchedulePartOnPersonAndDate(person, personRestriction.RestrictionDate, _scenario)).Return(schedulePart);
			_mocks.ReplayAll();
            using (var table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(restrictions, table, _stateHolder, _scenario);
                Assert.AreEqual(1, table.Rows.Count);
            }
			_mocks.VerifyAll();
        }

        [Test]
        public void VerifyPreferenceExist()
        {
            IPreferenceRestriction preference = new PreferenceRestriction();

            // Day off
            preference.DayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // Shift category
            preference = new PreferenceRestriction();
            preference.ShiftCategory = new ShiftCategory("SC");
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // Start time early
            preference = new PreferenceRestriction();
            preference.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(1, 0, 0), null);
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // Start time late
            preference = new PreferenceRestriction();
            preference.StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(23,0,0));
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // End time early
            preference = new PreferenceRestriction();
				preference.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(1, 0, 0), null);
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // End time late
            preference = new PreferenceRestriction();
				preference.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(23, 0, 0));
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // Work time min
            preference = new PreferenceRestriction();
				preference.WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(1, 0, 0), null);
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

            // Work time max
            preference = new PreferenceRestriction();
				preference.WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(23, 0, 0));
            Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));

			//Activity
			preference = new PreferenceRestriction();
			IActivity activity = new Activity("activity");
			IActivityRestriction activityRestriction = new ActivityRestriction(activity);
			preference.AddActivityRestriction(activityRestriction);
			Assert.IsTrue(_target.CheckIfPreferenceIsValid(preference));
        }

        [Test]
        public void VerifyNoPreferencesExist()
        {
            IPreferenceRestriction preference = null;
            Assert.IsFalse(_target.CheckIfPreferenceIsValid(preference));

            preference = new PreferenceRestriction();
            Assert.IsFalse(_target.CheckIfPreferenceIsValid(preference));
        }
    }
}
