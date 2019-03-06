using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ShiftsPerDayGraphicalToPdfManagerTest
	{
		private ShiftsPerDayGraphicalToPdfManager _manager;
		private CultureInfo _culture;
		private IPerson _person1;
		private IPerson _person2;
		private DateOnlyPeriod _dateOnlyPeriod;
		private ISchedulingResultStateHolder _stateHolder;
		private MockRepository _mockRepository;
		private bool _rightToLeft;
		private IScheduleDictionary _dic;
		private IScheduleDay _day1;
		private IScheduleDay _day2;
		private IVisualLayerCollection _layerCollection1;
		private IVisualLayerCollection _layerCollection2;
		private IScheduleRange _scheduleRange1;
		private IScheduleRange _scheduleRange2;
		private IProjectionService _projectionService1;
		private IProjectionService _projectionService2;
		private IEnumerator<IVisualLayer> _enumerator1;
		private IEnumerator<IVisualLayer> _enumerator2;
		private IVisualLayer _layer1;
		private IVisualLayer _layer2;
		private IPayload _payload;
		private ScheduleReportDialogGraphicalModel _model;
		IDictionary<IPerson, string> _persons;
		private IPerson _p1;
		private IPerson _p2;
		private IPerson _p3;
		private DateOnly _dateOnly1;
		private DateOnly _dateOnly2;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_stateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
			_dic = _mockRepository.StrictMock<IScheduleDictionary>();
			_day1 = _mockRepository.StrictMock<IScheduleDay>();
			_day2 = _mockRepository.StrictMock<IScheduleDay>();
			_scheduleRange1 = _mockRepository.StrictMock<IScheduleRange>();
			_scheduleRange2 = _mockRepository.StrictMock<IScheduleRange>();
			_projectionService1 = _mockRepository.StrictMock<IProjectionService>();
			_projectionService2 = _mockRepository.StrictMock<IProjectionService>();
			_layerCollection1 = _mockRepository.StrictMock<IVisualLayerCollection>();
			_layerCollection2 = _mockRepository.StrictMock<IVisualLayerCollection>();
			_layer1 = _mockRepository.StrictMock<IVisualLayer>();
			_layer2 = _mockRepository.StrictMock<IVisualLayer>();
			_payload = _mockRepository.StrictMock<IPayload>();
			_person1 = PersonFactory.CreatePerson("person1");
		    _person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(_person1, new DateOnly());
			_person2 = PersonFactory.CreatePerson("放弃的传真");
		    _person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(_person2, new DateOnly());
			_persons = new Dictionary<IPerson, string>{{_person1, _person1.Name.ToString()},{_person2, _person2.Name.ToString()}};
			_culture = CultureInfo.GetCultureInfo("en-US");
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			_rightToLeft = false;
			_enumerator1 = new List<IVisualLayer> {_layer1}.GetEnumerator();
			_enumerator2 = new List<IVisualLayer> { _layer2 }.GetEnumerator();
			_model = new ScheduleReportDialogGraphicalModel();
			_manager = new ShiftsPerDayGraphicalToPdfManager(_culture, _persons, _dateOnlyPeriod,_stateHolder,_rightToLeft, _model);
			_p1 = PersonFactory.CreatePerson("AAA");
			_p2 = PersonFactory.CreatePerson("BBBB");
			_p3 = PersonFactory.CreatePerson("CCCCC");
			_dateOnly1 = new DateOnly(2011, 1, 26);
			_dateOnly2 = new DateOnly(2011, 1, 27);
		}

		[Test]
		public void ShouldReturnSortedListName()
		{
			IDictionary<IPerson, string> persons = new Dictionary<IPerson, string>{{_p2, _p2.Name.ToString()},{_p1, _p1.Name.ToString()},{_p3, _p3.Name.ToString()}};

			var sorted = ShiftsPerDayGraphicalToPdfManager.SortOnCommonAgentName(persons);

			Assert.AreEqual(_p1, sorted[0]);
			Assert.AreEqual(_p2, sorted[1]);
			Assert.AreEqual(_p3, sorted[2]);
		}

		[Test]
		public void ShouldReturnSortedListTime()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 9, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			IList<IScheduleDay> scheduleDays = new List<IScheduleDay> {_day1, _day2};

			using(_mockRepository.Record())
			{
				Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.AtLeastOnce();
				Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.AtLeastOnce();
				Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.AtLeastOnce();
				Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.AtLeastOnce();
				Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(_day1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_day2.Person).Return(_person2).Repeat.AtLeastOnce();
			}

			using(_mockRepository.Playback())
			{
				var sorted = ShiftsPerDayGraphicalToPdfManager.SortOnTime(scheduleDays, true);
				Assert.AreEqual(_person1, sorted[0]);
				Assert.AreEqual(_person2, sorted[1]);

				sorted = ShiftsPerDayGraphicalToPdfManager.SortOnTime(scheduleDays, false);
				Assert.AreEqual(_person2, sorted[0]);
				Assert.AreEqual(_person1, sorted[1]);
			}
		}

		[Test]
		public void ShouldGetMaxLengthName()
		{
			IDictionary<IPerson, string> persons = new Dictionary<IPerson, string>{{_p2, _p2.Name.ToString()},{_p1, _p1.Name.ToString()},{_p3, _p3.Name.ToString()}};

			var max = ShiftsPerDayGraphicalToPdfManager.GetMaxLengthStringPersons(persons);
			Assert.AreEqual(_p3.Name.ToString(), max);
		}

		[Test]
		public void ShouldGetMaxLengthDate()
		{
			IList<DateOnly> dateOnlies = new List<DateOnly> {new DateOnly(2011, 1, 1)};

			var max = _manager.GetMaxLengthStringDates(dateOnlies);
			Assert.AreEqual(dateOnlies[0].Date.ToString("d", _culture), max);
		}

		[Test]
		public void ShouldCreateChineseReport()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			_manager = new ShiftsPerDayGraphicalToPdfManager(CultureInfo.GetCultureInfo("zh-CN"), _persons, _dateOnlyPeriod, _stateHolder, _rightToLeft, _model);
			var dateOnly = new DateOnly(2011, 1, 1);

			using (_mockRepository.Record())
			{
				CreateExpections(period1, period2, dateTime1, dateTime2, dateOnly, dateOnly);
                Expect.Call(_day1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_day2.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				var doc = _manager.ExportTeamView();
				Assert.IsNotNull(doc);
			}
		}

		[Test]
		public void ShouldCreateUnicodeReport()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);
			var dateOnly = new DateOnly(2011, 1, 1);

			using (_mockRepository.Record())
			{
				CreateExpections(period1, period2, dateTime1, dateTime3, dateOnly, dateOnly);
			    Expect.Call(_day1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			    Expect.Call(_day2.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				var doc = _manager.ExportTeamView();
				Assert.IsNotNull(doc);
			}
		}

		[Test]
		public void ShouldAddPageOnDaylightTimesavingSwitch()
		{
			Assert.IsFalse(ShiftsPerDayGraphicalToPdfManager.AddPageForDaylightSavingTime(_dateOnly1, _dateOnly2, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			_dateOnly1 = new DateOnly(2011, 3, 26);
			_dateOnly2 = new DateOnly(2011, 3, 27);

			Assert.IsTrue(ShiftsPerDayGraphicalToPdfManager.AddPageForDaylightSavingTime(_dateOnly1, _dateOnly2, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			_dateOnly1 = new DateOnly(2011, 10, 29);
			_dateOnly2 = new DateOnly(2011, 3, 30);

			Assert.IsTrue(ShiftsPerDayGraphicalToPdfManager.AddPageForDaylightSavingTime(_dateOnly1, _dateOnly2, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
		}

		[Test]
		public void ShouldCreateAgentViewReport()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);
			var dateOnly = new DateOnly(2011, 1, 1);

			using (_mockRepository.Record())
			{
				CreateExpections(period1, period2, dateTime1, dateTime3, dateOnly, dateOnly);
                Expect.Call(_day1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_day2.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				var doc = _manager.ExportAgentView(new FakeTimeZoneGuard());
				Assert.IsNotNull(doc);
			}
		}

		private void CreateExpections(DateTimePeriod period1, DateTimePeriod period2, DateTime dateTime1, DateTime dateTime3, DateOnly day1, DateOnly day2)
		{
			Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
			Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.AtLeastOnce();
			Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.AtLeastOnce();
			Expect.Call(_scheduleRange1.ScheduledDay(day1)).Return(_day1).Repeat.AtLeastOnce();
			Expect.Call(_scheduleRange2.ScheduledDay(day2)).Return(_day2).Repeat.AtLeastOnce();
			Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.AtLeastOnce();
			Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.AtLeastOnce();
			Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.AtLeastOnce();
			Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.AtLeastOnce();
			Expect.Call(_layerCollection1.HasLayers).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_layerCollection2.HasLayers).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.AtLeastOnce();
			Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.AtLeastOnce();
			Expect.Call(_day1.Person).Return(_person1).Repeat.AtLeastOnce();
			Expect.Call(_day2.Person).Return(_person2).Repeat.AtLeastOnce();
			Expect.Call(_layerCollection1.GetEnumerator()).Return(_enumerator1);
			Expect.Call(_layerCollection2.GetEnumerator()).Return(_enumerator2);
			Expect.Call(_layer1.Period).Return(new DateTimePeriod(dateTime1, dateTime3)).Repeat.AtLeastOnce();
			Expect.Call(_layer2.Period).Return(new DateTimePeriod(dateTime1, dateTime3)).Repeat.AtLeastOnce();
			Expect.Call(_layer1.Payload).Return(_payload);
			Expect.Call(_layer2.Payload).Return(_payload);
			Expect.Call(_payload.ConfidentialDisplayColor_DONTUSE(_person1)).Return(Color.Blue);
			Expect.Call(_payload.ConfidentialDisplayColor_DONTUSE(_person2)).Return(Color.Blue);

			var dateOnlyAsPeriod1 = new DateOnlyAsDateTimePeriod(day1, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			var dateOnlyAsPeriod2 = new DateOnlyAsDateTimePeriod(day2, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            Expect.Call(_day1.DateOnlyAsPeriod).Return(dateOnlyAsPeriod1).Repeat.AtLeastOnce();
            Expect.Call(_day2.DateOnlyAsPeriod).Return(dateOnlyAsPeriod2).Repeat.AtLeastOnce();
		}
	}
}
