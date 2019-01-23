using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportGraphicalDrawScheduleTest
	{
		private ScheduleReportGraphicalDrawSchedule _drawSchedule;
        private ScheduleReportGraphicalDrawSchedule _drawScheduleRtl;
        private ScheduleReportGraphicalDrawSchedule _drawSchedulePublicNote;
		private IScheduleDay _scheduleDay;
		private MockRepository _mockRepository;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _layerCollection;
		private IEnumerator<IVisualLayer> _enumerator;
		private IVisualLayer _layer;
		private IPayload _payload;
		private IPerson _person;
	    private IPersonPeriod _personPeriod;
	    private IPublicNote _publicNote;

		[SetUp]
		public void Setup()
		{
			var doc = new PdfDocument();
			var page = doc.Pages.Add();
			_mockRepository = new MockRepository();
			_scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
			_projectionService = _mockRepository.StrictMock<IProjectionService>();
			_layerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
			_layer = _mockRepository.StrictMock<IVisualLayer>();
			_enumerator = new List<IVisualLayer> { _layer }.GetEnumerator();
			_drawSchedule = new ScheduleReportGraphicalDrawSchedule(page, 0, 50, 0, _scheduleDay, false, false, new CultureInfo("sv-SE"));
            _drawSchedulePublicNote = new ScheduleReportGraphicalDrawSchedule(page, 0, 50, 0, _scheduleDay, false, true, new CultureInfo("sv-SE"));
            _drawScheduleRtl = new ScheduleReportGraphicalDrawSchedule(page, 0, 50, 0, _scheduleDay, true, false, new CultureInfo("sv-SE")); ;
			_payload = _mockRepository.StrictMock<IPayload>();
			_person = _mockRepository.StrictMock<IPerson>();
		    _personPeriod = _mockRepository.StrictMock<IPersonPeriod>();
		    _publicNote = _mockRepository.StrictMock<IPublicNote>();
		}

		[Test]
		public void ShouldDrawScheduleAndReturnTop()
		{
			var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var dateTime3 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2011, 1, 1);
			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

			using(_mockRepository.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_layerCollection);
				Expect.Call(_layerCollection.GetEnumerator()).Return(_enumerator);
				Expect.Call(_layer.Period).Return(new DateTimePeriod(dateTime1, dateTime3)).Repeat.AtLeastOnce();
				Expect.Call(_layer.Payload).Return(_payload);
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_payload.ConfidentialDisplayColor_DONTUSE(_person)).Return(Color.Blue);
			    Expect.Call(_person.Period(new DateOnly(2011, 1, 1))).Return(_personPeriod);
			    Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.ContractDayOff).Repeat.AtLeastOnce();
			}

			using(_mockRepository.Playback())
			{
				var top = _drawSchedule.Draw(new DateTimePeriod(2011, 1, 1, 2011, 1, 1));
				Assert.AreEqual(15, top);
			}	
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldDrawSchedulePublicNoteAndReturnTop()
        {
            var dateTime1 = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            var dateTime3 = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2011, 1, 1);
			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            const float fontSize = 9f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, new CultureInfo("sv-SE"));

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_layerCollection);
                Expect.Call(_layerCollection.GetEnumerator()).Return(_enumerator);
                Expect.Call(_layer.Period).Return(new DateTimePeriod(dateTime1, dateTime3)).Repeat.AtLeastOnce();
                Expect.Call(_layer.Payload).Return(_payload);
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_payload.ConfidentialDisplayColor_DONTUSE(_person)).Return(Color.Blue);
                Expect.Call(_person.Period(new DateOnly(2011, 1, 1))).Return(_personPeriod);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.PublicNoteCollection()).Return(new [] {_publicNote});
                Expect.Call(_publicNote.GetScheduleNote(new NoFormatting())).IgnoreArguments().Return("publicNote");
            }

            using (_mockRepository.Playback())
            {
                var top = _drawSchedulePublicNote.Draw(new DateTimePeriod(2011, 1, 1, 2011, 1, 1));
                Assert.AreEqual((int)(15 + font.Height + 2), (int)top);
            }
        }

        [Test]
        public void ShouldReturnXPosFromHour()
        {
            var timelineStart = new DateTime(2011, 1, 10, 8, 0, 0, DateTimeKind.Utc);
            var timelineEnd = new DateTime(2011, 1, 10, 18, 0, 0, DateTimeKind.Utc);
            var timelinePeriod = new DateTimePeriod(timelineStart, timelineEnd);
            var projectionRectangle = new Rectangle(0, 0, 100, 10);
            var xPos = _drawSchedule.HourX(10, timelinePeriod, projectionRectangle);
            Assert.AreEqual(20, xPos);
        }

        [Test]
        public void ShouldReturnXPosFromHourRtl()
        {
            var timelineStart = new DateTime(2011, 1, 10, 8, 0, 0, DateTimeKind.Utc);
            var timelineEnd = new DateTime(2011, 1, 10, 18, 0, 0, DateTimeKind.Utc);
            var timelinePeriod = new DateTimePeriod(timelineStart, timelineEnd);
            var projectionRectangle = new Rectangle(0, 0, 100, 10);
            var xPos = _drawScheduleRtl.HourX(10, timelinePeriod, projectionRectangle);
            Assert.AreEqual(80, xPos);
        }

        [Test]
        public void ShouldAdjustRectangleToHour()
        {
            var layerRectangle = new Rectangle(11, 0, 10, 10);
            var expectedRectangle = new Rectangle(10, 0, 10, 10);
            var projectionRectangle = new Rectangle(0, 0, 100, 10);
            var visualLayer = _mockRepository.StrictMock<IVisualLayer>();
            var timelineStart = new DateTime(2011, 1, 10, 8, 0, 0, DateTimeKind.Utc);
            var timelineEnd = new DateTime(2011, 1, 10, 18, 0, 0, DateTimeKind.Utc);
            var timelinePeriod = new DateTimePeriod(timelineStart, timelineEnd);

            var layerStart = new DateTime(2011, 1, 10, 9, 0, 0, DateTimeKind.Utc);
            var layerEnd = new DateTime(2011, 1, 10, 10, 0, 0, DateTimeKind.Utc);
            var layerPeriod = new DateTimePeriod(layerStart, layerEnd);

            using(_mockRepository.Record())
            {
                Expect.Call(visualLayer.Period).Return(layerPeriod).Repeat.AtLeastOnce();
            }

            using(_mockRepository.Playback())
            {
                var adjustedRectangle = _drawSchedule.AdjustRectangleToHour(layerRectangle, visualLayer, timelinePeriod, projectionRectangle);
                Assert.AreEqual(expectedRectangle, adjustedRectangle);
            }
        }

        [Test]
        public void ShouldAdjustRectangleToHourWhenLayerOnOtherDate()
        {
            var layerRectangle = new Rectangle(91, 0, 10, 10);
            var expectedRectangle = new Rectangle(90, 0, 10, 10);
            var projectionRectangle = new Rectangle(0, 0, 100, 10);
            var visualLayer = _mockRepository.StrictMock<IVisualLayer>();
            var timelineStart = new DateTime(2011, 1, 10, 22, 0, 0, DateTimeKind.Utc);
            var timelineEnd = new DateTime(2011, 1, 11, 8, 0, 0, DateTimeKind.Utc);
            var timelinePeriod = new DateTimePeriod(timelineStart, timelineEnd);

            var layerStart = new DateTime(2011, 1, 11, 7, 0, 0, DateTimeKind.Utc);
            var layerEnd = new DateTime(2011, 1, 11, 8, 0, 0, DateTimeKind.Utc);
            var layerPeriod = new DateTimePeriod(layerStart, layerEnd);

            using (_mockRepository.Record())
            {
                Expect.Call(visualLayer.Period).Return(layerPeriod).Repeat.AtLeastOnce();
            }

            using (_mockRepository.Playback())
            {
                var adjustedRectangle = _drawSchedule.AdjustRectangleToHour(layerRectangle, visualLayer, timelinePeriod, projectionRectangle);
                Assert.AreEqual(expectedRectangle, adjustedRectangle);
            }
        }

        [Test]
        public void ShouldNotDrawScheduleButReturnTopIfNoPersonPeriod()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_layerCollection);
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(new DateOnly(2011, 1, 1))).Return(null);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone));
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
            }

            using (_mockRepository.Playback())
            {
                var top = _drawSchedule.Draw(new DateTimePeriod(2011, 1, 1, 2011, 1, 1));
                Assert.AreEqual(15, top);
            }
        }
	}
}
