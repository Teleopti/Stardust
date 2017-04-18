using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport;

namespace Teleopti.Ccc.WinCodeTest.FileImport
{
    [TestFixture]
    public class DimQueueTableTest
    {
        private DimQueueTable _target;
        private TimeZoneInfo _timeZoneInfoUtc;
        private MockRepository _mocker;
     
        [SetUp]
        public void Setup()
        {
            _mocker= new MockRepository();
            _timeZoneInfoUtc = TimeZoneInfo.Utc;
            _target = new DimQueueTable(_timeZoneInfoUtc);      
        }

        [Test]
        public  void VerifyFill()
        {
            DataTable dt = _target.CreateEmptyDataTable();
            IList<ImportFileDo> list = new List<ImportFileDo>();
            list.Add(ImportFileDo.Create("24;20090220;06:00;6;Queue 14;3;0;0;0;3;11;0;562;0;7;0;0;3;0;0;0", ";", 1));
            _target.Fill(dt, list);
            Assert.AreEqual(dt.Rows.Count, 1);
        }

        [Test]
        public void VerifyUsesFileImportParserToConvertDateTime()
        {
            IFileImportDateTimeParser parser = _mocker.StrictMock<IFileImportDateTimeParser>();
           
            DateTime utcDateTime = new DateTime(2008, 01, 25, 12, 0, 0, DateTimeKind.Utc);

            string utcTimeString = "12:21";
            string dateString = "20090220";
            string timeString = "06:00";
            //--------------------------------

            DataTable dtUtc = _target.CreateEmptyDataTable();

            IList<ImportFileDo> list = new List<ImportFileDo>();
            list.Add(ImportFileDo.Create("24;" + dateString + ";" + timeString + ";6;Queue 14;3;0;0;0;3;11;0;562;0;7;0;0;3;0;0;0", ";", 1));
            
            using(_mocker.Record())
            {
                Expect.Call(()=>parser.TimeZone(_timeZoneInfoUtc)); //The timezone is passed from the DimQueTable
                Expect.Call(parser.DateTimeIsValid(dateString, timeString)).Return(true).Repeat.Any();
                Expect.Call(parser.UtcDateTime(dateString, timeString)).Return(utcDateTime).Repeat.Any();
                Expect.Call(parser.UtcTime(dateString, timeString)).Return(utcTimeString).Repeat.Any();
            }

            using(_mocker.Playback())
            {
                _target.Fill(dtUtc, list, parser);
                DateTime dateTime = (DateTime) dtUtc.Rows[0]["datetime"];
                string interval = (string) dtUtc.Rows[0]["interval"];

                Assert.AreEqual(utcDateTime.Day,dateTime.Day,"Day Is the parsed");
                Assert.AreEqual(utcDateTime.Month, dateTime.Month, "Month is parsed");
                Assert.AreEqual(utcDateTime.Year,dateTime.Year,"Year is parsed");

                Assert.AreEqual(utcTimeString,interval,"Timestring is converted by parser");   
            }  
        }

        [Test]
        public void VerifyDateFieldIsWithoutTimeValue()
        {
            DataTable dt = _target.CreateEmptyDataTable();
            IList<ImportFileDo> list = new List<ImportFileDo>();
            list.Add(ImportFileDo.Create("24;20090220;06:00;6;Queue 14;3;0;0;0;3;11;0;562;0;7;0;0;3;0;0;0", ";", 1));
            _target.Fill(dt, list);
            DateTime dateTime = (DateTime)dt.Rows[0]["datetime"];
            Assert.AreEqual(TimeSpan.Zero, dateTime.TimeOfDay);
        }

    }
}
