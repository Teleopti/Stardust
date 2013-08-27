using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class SchedulePreferenceTransformerHelperTest
	{
		private IPreferenceRestriction preferenceRestriction;
		private IScheduleDay schedulepart;

		[SetUp]
		public void Setup()
		{
			var scheduleParts = SchedulePartFactory.CreateSchedulePartCollection();
			schedulepart = scheduleParts[0];

			var activcity = new Activity("Main");
			activcity.SetId(Guid.NewGuid());
			var shiftCategory = new ShiftCategory("Test");
			shiftCategory.SetId(Guid.NewGuid());
			var dayOffTemplate = new DayOffTemplate(new Description("Day off"));
			dayOffTemplate.SetId(Guid.NewGuid());


			preferenceRestriction = new PreferenceRestriction
			{
				StartTimeLimitation =
					new StartTimeLimitation(
						new TimeSpan(6, 0, 0),
						new TimeSpan(20, 0, 0)),
				EndTimeLimitation =
					new EndTimeLimitation(
						new TimeSpan(6, 0, 0),
						new TimeSpan(20, 0, 0)),
				WorkTimeLimitation =
					new WorkTimeLimitation(
						new TimeSpan(6, 0, 0),
						new TimeSpan(20, 0, 0)),
				ShiftCategory = shiftCategory,
				DayOffTemplate = dayOffTemplate,
			};
			preferenceRestriction.SetId(Guid.NewGuid());
			preferenceRestriction.AddActivityRestriction(new ActivityRestriction(activcity));

			var preferenceDay = new PreferenceDay(schedulepart.Person, new DateOnly(), preferenceRestriction);
			preferenceDay.SetId(Guid.NewGuid());
		}

		[Test]
		public void FillDataRow_ShouldUseLocalTime()
		{
			var preferenceDay = new PreferenceDay(schedulepart.Person, new DateOnly(2010, 11, 12), preferenceRestriction);
			preferenceDay.SetId(Guid.NewGuid());

			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);
				var row = table.NewRow();
				var result = SchedulePreferenceTransformerHelper.FillDataRow(row, preferenceRestriction, schedulepart);
				result["restriction_date"].Should().Be.EqualTo(new DateTime(2010, 11, 12));
			}
		}

		[Test]
		public void FillDataRow_AddDataToRow_ReturnValidDataRow()
		{
			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				SchedulePreferenceInfrastructure.AddColumnsToDataTable(table);
				var row = table.NewRow();
				var result = SchedulePreferenceTransformerHelper.FillDataRow(row, preferenceRestriction, schedulepart);
				result.Should().Not.Be.Null();

				var absence = new Absence();
				absence.SetId(Guid.NewGuid());
				preferenceRestriction.Absence = absence;
				result = SchedulePreferenceTransformerHelper.FillDataRow(row, preferenceRestriction, schedulepart);
				result.Should().Not.Be.Null();
			}
		}

		[Test]
		public void CheckIfPreferenceIsValid_ValidPreference_ReturnTrue()
		{
			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(null)
				.Should().Be.EqualTo(false);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {ShiftCategory = new ShiftCategory("ShiftCategory")})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {DayOffTemplate = new DayOffTemplate(new Description("DayOff"))})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), null)})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(20, 0, 0))})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {EndTimeLimitation = new EndTimeLimitation(new TimeSpan(6, 0, 0), null)})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction {EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(20, 0 ,0))})
			                                   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0 , 0), null )})
											   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Be.EqualTo(true);

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				new PreferenceRestriction { Absence = new Absence()})
											   .Should().Be.EqualTo(true);

			var preference = new PreferenceRestriction();
			preference.AddActivityRestriction(new ActivityRestriction());
			preference.AddActivityRestriction(new ActivityRestriction());

			SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(
				preference)
			                                   .Should().Be.EqualTo(true);

		}
	}
}
