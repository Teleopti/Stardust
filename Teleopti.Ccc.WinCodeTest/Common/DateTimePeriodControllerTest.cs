using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{


    [TestFixture]
    public class DateTimePeriodControllerTest
    {

        private DateTimePeriodController _target;
        private DateTimePeriod _defaultDateTimePeriod;
        private TimeSpan _interval;
      
        [SetUp]
        public void Setup()
        {
            _interval = TimeSpan.FromMinutes(15);
            _defaultDateTimePeriod = new DateTimePeriod(2008,07,05,2008,07,08);
            _target = new DateTimePeriodController(_defaultDateTimePeriod, _interval);
        }
	
        [Test]
        public void VerifyCanGetDateTimePeriodFromController()
        {
            Assert.IsNotNull(_target.DateTimePeriod);
        }

        [Test]
        public void VerifyCanGetAndSetStartDateTime()
        {
            DateTime theDateTime = new DateTime(2001,1,1,1,1,0,DateTimeKind.Utc);
            Assert.IsNotNull(_target.StartDateTime);
            _target.StartDateTime= theDateTime;
            Assert.AreEqual(theDateTime, _target.StartDateTime);
        }

        [Test]
        public void VerifyIntervalIsAddedWhenSettingHighStartDateTime()
        {
            DateTime laterThanEndDateTime = _defaultDateTimePeriod.EndDateTime.AddDays(1);
            _target.StartDateTime = laterThanEndDateTime;
            Assert.AreEqual(_target.StartDateTime, 
                laterThanEndDateTime,
                "EndTime should change to startTime+interval when setting StartTime+interval > EndTime");     
        }

        [Test]
        public void VerifyIntervalIsSubtractedWhenSettingLowEndDateTime()
        {
            DateTime earlierThanStartDateTime = _defaultDateTimePeriod.StartDateTime.Subtract(TimeSpan.FromDays(1));
            _target.EndDateTime = earlierThanStartDateTime;
            Assert.AreEqual(earlierThanStartDateTime, _target.EndDateTime);
            Assert.AreEqual(_target.StartDateTime,
                earlierThanStartDateTime.Subtract(_interval)
                ,"StartTime should change to EndTime-interval when setting low EndTime");
        }

        [Test]
        public void VerifyExtendEndTimeIfIntervalIsLongerThanPeriod()
        {
            DateTimePeriod thePeriod = new DateTimePeriod(2001, 2, 2, 2001, 2, 3);
            DateTimePeriodController controller = new DateTimePeriodController(thePeriod,TimeSpan.FromDays(4));
            Assert.AreEqual(thePeriod.StartDateTime, controller.StartDateTime,"StartDateTiem Should be intact");
            Assert.AreEqual(thePeriod.StartDateTime.AddDays(4), controller.EndDateTime,"EndDateTime should be minimum startdatetime+interval");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OnlyUtc()
        {
            _target.StartDateTime= new DateTime(2002,1,1,1,1,0,DateTimeKind.Local);
        }

        /// <summary>
        /// Test for checking that Notifications for Databinding are correct in order an in number
        /// </summary> 
        [Test]
        public void VerifyNotifyOnChange()
        {
            //Setup for testing the PropertyChanged Event
            #region setup
            int i = 0;
            string [] propertiesFiredInOrder = new string[]  
            {   
                "DateTimePeriod",   //1. StartDateTime changed
                "StartDateTime",     
                "DateTimePeriod",   //2. EndDateTime Changed
                "EndDateTime",       
                "Interval",         //3. Interval Changed
                "Interval",         //4. Interval Changed to longer than EndDateTime forces EndDateTime to change
                "DateTimePeriod",
                "EndDateTime",      
                "DateTimePeriod",    //5. Changing StartDateTime > EndDateTime changes DateTimePeriod and EndDateTime
                "EndDateTime", 
                "StartDateTime" 
            };

            PropertyChangedEventHandler ControlHandler = new PropertyChangedEventHandler(
                    delegate(object sender, PropertyChangedEventArgs e)
                    {
                        Assert.AreEqual(propertiesFiredInOrder[i], e.PropertyName,"Wrong PropertyChanged Fired");
                        Debug.WriteLine(e.PropertyName);
                        i++;
                    });

            _target.PropertyChanged += ControlHandler;
            #endregion //setup

            _target.StartDateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc); //Fires Start -> Period
            _target.EndDateTime = new DateTime(2000, 1, 2, 1, 1, 1, DateTimeKind.Utc);  //Fires End -> Period
            _target.Interval = TimeSpan.FromHours(1);   //Fires Interval
            _target.Interval = TimeSpan.FromDays(5);    //Fires Interval -> Period -> End
            _target.StartDateTime = _target.EndDateTime.AddDays(1); //Fires Start -> End -> Period

            //Check that all "properties" has been fired (not more or less):
            Assert.AreEqual(propertiesFiredInOrder.Length,i,"PropertyChanged fired more/less times than expected");
        
        
        }

        [Test]
        public void VerifyGetTheCorrectNumberOfStartAndEndDateTimes()
        {
          
            _target.StartInterval = TimeSpan.FromHours(1);
            _target.EndInterval = TimeSpan.FromHours(1);
            _target.Interval = TimeSpan.FromMinutes(15);

            Assert.AreEqual(5, _target.StartTimeSelection.Count,"StartTime+intervals");
            Assert.AreEqual(5, _target.EndTimeSelection.Count,"EndTime + intervals");

        }

        [Test]
	    public void VerifyCorrectStartEndDateTimes()
	    {

            _target.StartInterval = TimeSpan.FromHours(1);
            _target.EndInterval = TimeSpan.FromHours(1);
            _target.Interval = TimeSpan.FromMinutes(15);

            Assert.AreEqual(_target.EndTimeSelection[0], _target.EndDateTime);
            Assert.AreEqual(_target.EndTimeSelection[1], _target.EndDateTime.Add(TimeSpan.FromMinutes(15)));
            Assert.AreEqual(_target.EndTimeSelection[2], _target.EndDateTime.Add(TimeSpan.FromMinutes(30)));
            Assert.AreEqual(_target.EndTimeSelection[3], _target.EndDateTime.Add(TimeSpan.FromMinutes(45)));
            Assert.AreEqual(_target.EndTimeSelection[4], _target.EndDateTime.Add(TimeSpan.FromMinutes(60)));

            Assert.AreEqual(_target.StartTimeSelection[0], _target.StartDateTime);
            Assert.AreEqual(_target.StartTimeSelection[1], _target.StartDateTime.Subtract(TimeSpan.FromMinutes(15)));
            Assert.AreEqual(_target.StartTimeSelection[2], _target.StartDateTime.Subtract(TimeSpan.FromMinutes(30)));
            Assert.AreEqual(_target.StartTimeSelection[3], _target.StartDateTime.Subtract(TimeSpan.FromMinutes(45)));
            Assert.AreEqual(_target.StartTimeSelection[4], _target.StartDateTime.Subtract(TimeSpan.FromMinutes(60)));
        }
              
        [Test]
        public void VerifyDefaultStartEndInterval()
        {
            TimeSpan notDefaultTimeSpan = TimeSpan.FromHours(8);
            DateTimePeriodController target2 = new DateTimePeriodController(_target.DateTimePeriod, TimeSpan.FromHours(1), notDefaultTimeSpan, notDefaultTimeSpan);
            Assert.AreEqual(TimeSpan.FromHours(12), _target.StartInterval);
            Assert.AreEqual(TimeSpan.FromHours(12), _target.EndInterval);
            Assert.AreEqual(notDefaultTimeSpan, target2.StartInterval);
            Assert.AreEqual(notDefaultTimeSpan, target2.EndInterval);
        }


        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyIntervalIsPositive()
        {
            _target.Interval = TimeSpan.FromMinutes(-1);
        } 

        


    }

   
}
