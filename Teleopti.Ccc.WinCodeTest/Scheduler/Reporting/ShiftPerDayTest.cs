using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class ShiftPerDayTest
    {
        private ShiftsPerDayToPdfManager _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _stateHolder;
        private TimeZoneInfo _timeZoneInfo;
        private IPerson _person1;
        private IPerson _person2;
        private DateOnlyPeriod _period;
        private IScheduleDictionary _dic;
        Scenario _scenario;
        IScheduleParameters _parameters;
        IScheduleParameters _parameters2;
        ScheduleRange _scheduleRange;
        ScheduleRange _scheduleRange2;
        DateTimePeriod _dateTimePeriod;
		IDictionary<IPerson, string> _persons;

        private PdfFont _font;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _target = new ShiftsPerDayToPdfManager();
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _timeZoneInfo = (
                TimeZoneInfo.FindSystemTimeZoneById(
                    "W. Europe Standard Time"));
            _person1 = new Person();
            _person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(_person1, new DateOnly());
            // arabic
            _person1.WithName(new Name("Ola", "حالات غياب")) ;
            //thai doesn't work
            //_person1.Name = new Name("Ola", "ตอบคำถามเพื่อสมาชิก");
            _person2 = new Person();
            //och kinesiska
            _person2.WithName(new Name("ola", "放弃的传真"));
            _person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(_person2, new DateOnly());
			_persons = new Dictionary<IPerson, string> { { _person2, _person2.Name.ToString() }, { _person1, _person1.Name.ToString() } };
            _period = new DateOnlyPeriod(2009,2,2,2009,2,2);

            IActivity mainActivity = new Activity("Phone");
            IActivity lunchActivity = new Activity("Lunch") {ReportLevelDetail = ReportLevelDetail.Lunch};
            IActivity breakActivity = new Activity("Break")
                                          {
                                              ReportLevelDetail = ReportLevelDetail.ShortBreak
                                          };

            _scenario = new Scenario("default");
            _dateTimePeriod = new DateTimePeriod(2009, 2, 2, 2009, 2, 3);
            _parameters = new ScheduleParameters(_scenario, _person1, _dateTimePeriod);
            _parameters2 = new ScheduleParameters(_scenario, _person2, _dateTimePeriod);
			var authorization = new FullPermission();
			var permissionChecker = new PersistableScheduleDataPermissionChecker(authorization);

			_scheduleRange = new ScheduleRange(_dic, _parameters, permissionChecker, authorization);
            _scheduleRange2 = new ScheduleRange(_dic, _parameters2, permissionChecker, authorization);

						IPersonAssignment ass = new PersonAssignment(_person1, _scenario, new DateOnly(2009, 2, 2));

					ass.SetShiftCategory(new ShiftCategory("Olas"));
					ass.AddActivity(mainActivity,new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),new DateTime(2009, 2, 2, 17, 0, 0, DateTimeKind.Utc)));
					ass.AddActivity(lunchActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc)));
					ass.AddActivity(breakActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 9, 45, 0, DateTimeKind.Utc)));
					ass.AddActivity(breakActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 15, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 15, 15, 0, DateTimeKind.Utc)));

					
					_scheduleRange.Add(ass);

						IPersonAssignment ass2 = new PersonAssignment(_person2, _scenario, new DateOnly(2009, 2, 2));
					ass2.SetShiftCategory(new ShiftCategory("Olas2"));
					ass2.AddActivity(mainActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 7, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 18, 0, 0, DateTimeKind.Utc)));
					ass2.AddActivity(lunchActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 10, 30, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 11, 30, 0, DateTimeKind.Utc)));
					ass2.AddActivity(breakActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 9, 15, 0, DateTimeKind.Utc)));
					ass2.AddActivity(breakActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 14, 15, 0, DateTimeKind.Utc)));
					ass2.AddActivity(breakActivity, new DateTimePeriod(new DateTime(2009, 2, 2, 17, 15, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 17, 30, 0, DateTimeKind.Utc)));
					_scheduleRange2.Add(ass2);
		}
		
        [Test]
        public void CanCreateChineseReport()
        {
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
                Expect.Call(_dic[_person1]).Return(_scheduleRange).Repeat.AtLeastOnce();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.AtLeastOnce();
                Expect.Call(_dic.Scenario).Return(_scenario).Repeat.Any();
            }

            PdfDocument ret = _target.Export(_timeZoneInfo, CultureInfo.GetCultureInfo("zh-CN"), _persons, _period, _stateHolder,
                          ScheduleReportDetail.All, false);

            Assert.IsNotNull(ret);
            
            //OpenDocument(ret, "C:\\temp\\tempZH.pdf");
        }
        
        [Test]
        public void CanCreateUnicodeReport()
        {
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
                Expect.Call(_dic[_person1]).Return(_scheduleRange).Repeat.AtLeastOnce();
                Expect.Call(_dic[_person2]).Return(_scheduleRange2).Repeat.AtLeastOnce();
                Expect.Call(_dic.Scenario).Return(_scenario).Repeat.Any();

            }

            PdfDocument ret = _target.Export(_timeZoneInfo, CultureInfo.GetCultureInfo("en-US"), _persons, _period, _stateHolder,
                           ScheduleReportDetail.All, false);

            Assert.IsNotNull(ret);

            //OpenDocument(ret, "C:\\temp\\tempUnicoce.pdf");
        }

        [Test]
        public void CanGetBoldFont()
        {
            _font = PdfFontManager.GetFont(10, PdfFontStyle.Bold, CultureInfo.GetCultureInfo("en-US"));
            Assert.AreEqual(PdfFontStyle.Bold, _font.Style);
        }

        //private static void OpenDocument(PdfDocument doc, string fullPath)
        //{
        //    bool success = false;
        //    try
        //    {
        //        doc.Save(fullPath);
        //        success = true;
        //    }
        //    catch (IOException ex)
        //    {
        //        Debug.Print(ex.Message);
        //    }

        //    if (!success)
        //        return;

        //    //Launching the PDF file using the default Application.[Acrobat Reader]
        //    Process.Start(fullPath);

        //}
    }

    
}
