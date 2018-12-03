using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class ScheduleDayOffCountTransformerTest
	{
		private ScheduleDayOffCountTransformer _target;
		private IPerson _person;
		private IList<IScheduleDay> _schedules;
		private IList<IScheduleDay> _schedules2;
		private IScenario _scenario;
		private DateTimePeriod _period1;
		private IScheduleDay _schedulePart1;
		private IScheduleDay _schedulePart2;
		private IScheduleDay _schedulePart3;
		private const int intervalsPerDay = 96;
		private IScheduleDictionary _dic;
		private MockRepository _mocks;
		private CultureInfo _cultureInfo;
		[SetUp]
		public void Setup()
		{
			_person = new Person();
			_person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("UTC")));
			_person.SetId(Guid.NewGuid());
			_schedules = new List<IScheduleDay>();
			_schedules2 = new List<IScheduleDay>();
			_scenario = new Scenario("default");
			_scenario.SetId(Guid.NewGuid());
			_period1 = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			_dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_period1), new Dictionary<IPerson, IScheduleRange>());
			_schedulePart1 = ExtractedSchedule.CreateScheduleDay(_dic, _person, new DateOnly(2000, 1, 1), CurrentAuthorization.Make());
			_schedulePart2 = ExtractedSchedule.CreateScheduleDay(_dic, _person, new DateOnly(2000, 1, 3), CurrentAuthorization.Make());
			_schedulePart3 = ExtractedSchedule.CreateScheduleDay(_dic, _person, new DateOnly(2000, 1, 5), CurrentAuthorization.Make());

			var dayOff1 = new DayOffTemplate(new Description("test"));
			dayOff1.SetId(Guid.NewGuid());
			dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
			dayOff1.Anchor = TimeSpan.Zero;

			_schedulePart1.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, _schedulePart1.DateOnlyAsPeriod.DateOnly, dayOff1));

			var dayOff2 = new DayOffTemplate(new Description("test"));
			dayOff2.SetId(Guid.NewGuid());
			dayOff2.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
			dayOff2.Anchor = new TimeSpan(1, 15, 0);
			_schedulePart2.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, _schedulePart2.DateOnlyAsPeriod.DateOnly, dayOff2));


			RaptorTransformerHelper.SetUpdatedOn(_schedulePart1.PersonAssignment(), DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(_schedulePart2.PersonAssignment(), DateTime.Now);

			_schedules.Add(_schedulePart1);
			_schedules.Add(_schedulePart2);

			_schedules2.Add(_schedulePart3);
			_cultureInfo = Thread.CurrentThread.CurrentCulture;
			_target = new ScheduleDayOffCountTransformer();
		}

		[Test]
		public void VerifyRowCount()
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(_schedules, dataTable, intervalsPerDay);
				Assert.AreEqual(_schedules.Count, dataTable.Rows.Count);
			}
		}

		[Test]
		public void VerifyCreateDataRow()
		{
			IList<DataRow> dataRowCollection;
			using (var dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(dataTable);
				dataRowCollection = ScheduleDayOffCountTransformer.CreateDataRows(_schedules, dataTable);
			}

			Assert.AreEqual(dataRowCollection[0]["schedule_date_local"], _schedulePart1.PersonAssignment().DayOff().Anchor.Date);
			Assert.AreEqual(dataRowCollection[0]["person_code"], _schedulePart1.Person.Id);
			Assert.AreEqual(dataRowCollection[0]["scenario_code"], _schedulePart1.Scenario.Id);
			Assert.AreEqual(dataRowCollection[1]["starttime"], _schedulePart2.PersonAssignment().DayOff().Anchor);
			Assert.AreEqual(dataRowCollection[0]["day_off_code"], _schedulePart1.PersonAssignment().DayOff().DayOffTemplateId);
			Assert.AreEqual(dataRowCollection[0]["day_count"], 1);
			Assert.AreEqual(dataRowCollection[0]["business_unit_code"], _schedulePart1.Scenario.BusinessUnit.Id);
			Assert.AreEqual(dataRowCollection[1]["datasource_update_date"], _schedulePart2.PersonAssignment().UpdatedOn.Value);
		}

		[Test]
		public void VerifySchedulePartWithoutDayOff()
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(dataTable);
				var target = new ScheduleDayOffCountTransformer();
				target.Transform(_schedules2, dataTable, intervalsPerDay);
				Assert.AreEqual(0, dataTable.Rows.Count);
			}
		}

		[Test]
		public void ShouldReturnNullIfDataTableIsNull()
		{
			_mocks = new MockRepository();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			Assert.That(ScheduleDayOffCountTransformer.CreateDataRow(scheduleDay, null), Is.Null);
		}

		[Test]
		public void ShouldReturnEmptyRowIfScheduleDayIsNull()
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Locale = _cultureInfo;
				ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(dataTable);
				var row = ScheduleDayOffCountTransformer.CreateDataRow(null, dataTable);
				Assert.That(row, Is.Not.Null);
				Assert.That(row["schedule_date_local"].ToString(), Is.EqualTo(""));
			}

		}
	}
}
