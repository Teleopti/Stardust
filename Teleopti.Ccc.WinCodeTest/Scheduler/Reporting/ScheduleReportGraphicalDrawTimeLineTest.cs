using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Pdf;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportGraphicalDrawTimelineTest
	{
		private ScheduleReportGraphicalDrawTimeline _drawTimeline;
		private CultureInfo _culture;
		private IList<IPerson> _persons;
		private IPerson _person1;
		private IPerson _person2;
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
			_person1 = PersonFactory.CreatePerson("person1");
			_person2 = PersonFactory.CreatePerson("放弃的传真");
			_persons = new List<IPerson> { _person1, _person2 };
			_culture = CultureInfo.GetCultureInfo("en-US");
			_rightToLeft = false;
			var doc = new PdfDocument();
			var page = doc.Pages.Add();
			_drawTimeline = new ScheduleReportGraphicalDrawTimeline(_culture,_rightToLeft,0, page, 0, 50);	
		}

		[Test]
		public void ShouldReturnGenericPeriodForTimeline()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 22, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			var dateTimeExpectedStart = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTimeExpectedEnd = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);

			var dateOnly = new DateOnly(2011, 1, 1);

			var expectedPeriod = new DateTimePeriod(dateTimeExpectedStart, dateTimeExpectedEnd);

			using (_mockRepository.Record())
			{
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.Twice();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.Twice();
                Expect.Call(_scheduleRange1.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day1).Repeat.Twice();
                Expect.Call(_scheduleRange2.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day2).Repeat.Twice();
                Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.Twice();
                Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.Twice();
                Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.Twice();
                Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.Twice();
                Expect.Call(_layerCollection1.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection2.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.Twice();
                Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				IList<DateOnly> dateOnlies = new List<DateOnly> { new DateOnly(2011, 1, 1) };
				Assert.AreEqual(expectedPeriod, _drawTimeline.TimelinePeriod(dateOnlies, _stateHolder, _persons, dateOnly));
			}
		}

		[Test]
		public void ShouldReturnGenericPeriodForTimelineArabic()
		{
			_culture = CultureInfo.GetCultureInfo("ar-SA");
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 22, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			var dateTimeExpectedStart = new DateTime(1432, 1, 26, 8, 0, 0, 0, _culture.Calendar,  DateTimeKind.Utc);
			var dateTimeExpectedEnd = new DateTime(1432, 1, 27, 7, 0, 0, 0, _culture.Calendar, DateTimeKind.Utc);
		    
			var dateOnly = new DateOnly(2011, 1, 1);
			var expectedPeriod = new DateTimePeriod(dateTimeExpectedStart, dateTimeExpectedEnd);

			using (_mockRepository.Record())
			{
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.Twice();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.Twice();
                Expect.Call(_scheduleRange1.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day1).Repeat.Twice();
                Expect.Call(_scheduleRange2.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day2).Repeat.Twice();
                Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.Twice();
                Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.Twice();
                Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.Twice();
                Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.Twice();
                Expect.Call(_layerCollection1.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection2.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.Twice();
                Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				var doc = new PdfDocument();
				var page = doc.Pages.Add();
				_drawTimeline = new ScheduleReportGraphicalDrawTimeline(_culture, _rightToLeft, 0, page, 0, 50);
	
				IList<DateOnly> dateOnlies = new List<DateOnly> { new DateOnly(2011, 1, 1) };
				Assert.AreEqual(expectedPeriod, _drawTimeline.TimelinePeriod(dateOnlies, _stateHolder, _persons, dateOnly));
			}	
		}

        [Test]
        public void ShouldUseDefaultPeriodOnNoLayers()
        {
            var dateTimeExpectedStart = TimeZoneHelper.ConvertToUtc(new DateTime(2011, 1, 1, 8, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var dateTimeExpectedEnd = TimeZoneHelper.ConvertToUtc(new DateTime(2011, 1, 1, 17, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var dateOnly = new DateOnly(2011, 1, 1);
            var expectedPeriod = new DateTimePeriod(dateTimeExpectedStart, dateTimeExpectedEnd);

            using (_mockRepository.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.Twice();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.Twice();
                Expect.Call(_scheduleRange1.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day1).Repeat.Twice();
                Expect.Call(_scheduleRange2.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day2).Repeat.Twice();
                Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.Twice();
                Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.Twice();
                Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.Twice();
                Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.Twice();
                Expect.Call(_layerCollection1.HasLayers).Return(false).Repeat.Twice();
                Expect.Call(_layerCollection2.HasLayers).Return(false).Repeat.Twice();
            }

            using (_mockRepository.Playback())
            {
                IList<DateOnly> dateOnlies = new List<DateOnly> { new DateOnly(2011, 1, 1) };
                Assert.AreEqual(expectedPeriod, _drawTimeline.TimelinePeriod(dateOnlies, _stateHolder, _persons, dateOnly));
            }      
        }

		[Test]
		public void ShouldReturnPeriodConvertedToDate()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 22, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			var dateTimeExpectedStart = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTimeExpectedEnd = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);

			var expectedPeriod = new DateTimePeriod(dateTimeExpectedStart, dateTimeExpectedEnd);

			var dateOnly = new DateOnly(2011, 1, 1);

			using (_mockRepository.Record())
			{
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.Twice();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.Twice();
                Expect.Call(_scheduleRange1.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day1).Repeat.Twice();
                Expect.Call(_scheduleRange2.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day2).Repeat.Twice();
                Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.Twice();
                Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.Twice();
                Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.Twice();
                Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.Twice();
                Expect.Call(_layerCollection1.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection2.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.Twice();
                Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				IList<DateOnly> dateOnlies = new List<DateOnly> {dateOnly};
				Assert.AreEqual(expectedPeriod, _drawTimeline.TimelinePeriod(dateOnlies,_stateHolder, _persons, dateOnly));
			}	
		}

		[Test]
		public void ShouldDrawAndReturnTop()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(dateTime1, dateTime2);

			var dateTime3 = new DateTime(2011, 1, 1, 22, 0, 0, DateTimeKind.Utc);
			var dateTime4 = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(dateTime3, dateTime4);

			using (_mockRepository.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Twice();
				Expect.Call(_dic[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day1).Repeat.Twice();
                Expect.Call(_scheduleRange2.ScheduledDay(new DateOnly(2011, 1, 1))).Return(_day2).Repeat.Twice();
                Expect.Call(_day1.ProjectionService()).Return(_projectionService1).Repeat.Twice();
                Expect.Call(_day2.ProjectionService()).Return(_projectionService2).Repeat.Twice();
                Expect.Call(_projectionService1.CreateProjection()).Return(_layerCollection1).Repeat.Twice();
                Expect.Call(_projectionService2.CreateProjection()).Return(_layerCollection2).Repeat.Twice();
                Expect.Call(_layerCollection1.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection2.HasLayers).Return(true).Repeat.Twice();
                Expect.Call(_layerCollection1.Period()).Return(period1).Repeat.Twice();
                Expect.Call(_layerCollection2.Period()).Return(period2).Repeat.Twice();
			}

			using (_mockRepository.Playback())
			{
				IList<DateOnly> dateOnlies = new List<DateOnly> { new DateOnly(2011, 1, 1) };
				var top =_drawTimeline.Draw(dateOnlies, _stateHolder, _persons, new DateOnly(2011, 1, 1));
				Assert.AreEqual(30, top);
			}	
		}
	}
}
