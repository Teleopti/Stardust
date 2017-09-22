using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class SchedulePreferenceTransformerTest
	{
		private ISchedulePreferenceTransformer _target;

		[SetUp]
		public void Setup()
		{
			_target = new SchedulePreferenceTransformer();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyTransform()
		{

			IList<IScheduleDay> scheduleParts = SchedulePartFactory.CreateSchedulePartCollection();
			IScheduleDay schedulePart = scheduleParts[0];

			IActivity activity = new Activity("Main");
			activity.SetId(Guid.NewGuid());
			IPerson person = schedulePart.Person;
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
			Schedule schedule = (Schedule)schedulePart;
			//schedule.Add(assignment);
			schedule.Add(personRestriction);


			using (DataTable table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(scheduleParts, table);
				Assert.AreEqual(1, table.Rows.Count);
			}
		}

		[Test]
		public void VerifyNoPreferencesExist()
		{
			IPreferenceRestriction preference = null;
			Assert.IsFalse(_target.CheckIfPreferenceIsValid(preference));
		}
	}
}
