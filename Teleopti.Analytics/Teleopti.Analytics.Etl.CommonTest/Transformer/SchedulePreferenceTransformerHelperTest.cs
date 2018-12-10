using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
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
				var result = SchedulePreferenceTransformer.FillDataRow(row, preferenceRestriction, schedulepart);
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
				var result = SchedulePreferenceTransformer.FillDataRow(row, preferenceRestriction, schedulepart);
				result.Should().Not.Be.Null();

				var absence = new Absence();
				absence.SetId(Guid.NewGuid());
				preferenceRestriction.Absence = absence;
				result = SchedulePreferenceTransformer.FillDataRow(row, preferenceRestriction, schedulepart);
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

		[Test]
		public void ShouldResolveStandardPreference()
		{
			//--Shift Category (standard) Preference = 1
			//WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') 
			//+ ISNULL(f.EndTimeMaximum,'') +  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' 
			//AND f.shift_category_code IS NOT NULL AND f.activity_code IS NULL THEN 1

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), null)})
			                                   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(20, 0, 0))})
			                                   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction {EndTimeLimitation = new EndTimeLimitation(new TimeSpan(6, 0, 0), null)})
			                                   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction {EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(20, 0 ,0))})
			                                   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0 , 0), null )})
											   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Not.Be.EqualTo(1);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { Absence = new Absence()})
											   .Should().Not.Be.EqualTo(1);

			var preference = new PreferenceRestriction();
			preference.AddActivityRestriction(new ActivityRestriction());
			preference.AddActivityRestriction(new ActivityRestriction());

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference)
			                                   .Should().Not.Be.EqualTo(1);

			preference = new PreferenceRestriction();
			preference.ShiftCategory = new ShiftCategory("hej");
			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldResolveDayOffPreference()
		{
			//--Day Off Preference = 2
			//WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') 
			//+ ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' 
			//AND f.day_off_name IS NOT NULL AND f.activity_code IS NULL THEN 2
			var preference = new PreferenceRestriction();
			preference.DayOffTemplate = new DayOffTemplate();
			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference).Should().Be.EqualTo(2);

			preference.WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(20, 0, 0));
			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference).Should().Not.Be.EqualTo(2);

			preference = new PreferenceRestriction();
			preference.DayOffTemplate = new DayOffTemplate();
			preference.AddActivityRestriction(new ActivityRestriction());
			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference).Should().Not.Be.EqualTo(2);
		}

		[Test]
		public void ShouldResolveExtendedPreference()
		{
			//--Extended Preference = 3
			//WHEN f.StartTimeMinimum IS NOT NULL OR f.EndTimeMinimum IS NOT NULL OR f.StartTimeMaximum IS NOT NULL 
			//OR f.EndTimeMaximum IS NOT NULL OR f.WorkTimeMinimum IS NOT NULL OR f.WorkTimeMaximum IS NOT NULL 
			//OR f.activity_code IS NOT NULL THEN 3

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { EndTimeLimitation = new EndTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Be.EqualTo(3);

			

			var preference = new PreferenceRestriction();
			preference.AddActivityRestriction(new ActivityRestriction());
			preference.AddActivityRestriction(new ActivityRestriction());

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference)
											   .Should().Be.EqualTo(3);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { Absence = new Absence() })
											   .Should().Not.Be.EqualTo(3);
		}

		[Test]
		public void ShouldResolveAbsencePreference()
		{
			//--Absence Preference = 4
			//WHEN ISNULL(f.StartTimeMinimum,'') + ISNULL(f.EndTimeMinimum,'') + ISNULL(f.StartTimeMaximum,'') + ISNULL(f.EndTimeMaximum,'') 
			//+  ISNULL(f.WorkTimeMinimum,'') + ISNULL(f.WorkTimeMaximum,'') = '' 
			//AND f.absence_code IS NOT NULL AND f.activity_code IS NULL THEN 4

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { EndTimeLimitation = new EndTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0, 0), null) })
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(20, 0, 0)) })
											   .Should().Not.Be.EqualTo(4);

			var preference = new PreferenceRestriction();
			preference.AddActivityRestriction(new ActivityRestriction());
			preference.AddActivityRestriction(new ActivityRestriction());

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference)
											   .Should().Not.Be.EqualTo(4);

			SchedulePreferenceTransformerHelper.GetPreferenceTypeId(
				new PreferenceRestriction { Absence = new Absence() })
											   .Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldThrowIfPreferenceTypeCanNotBeResolved()
		{
			var preference = new PreferenceRestriction();
			Assert.Throws<ArgumentException>(() => SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preference));
		}
	}
}
