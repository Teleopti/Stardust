using System;
using System.Threading;
using System.Windows.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common.Time.Timeline
{
    //  Note: Will be replaced with TimlineViewModel
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TimelineControlViewModelTest
    {
        private TimelineControlViewModel _target;
        private DateTimePeriod _nowPeriod;
        private DateTimePeriod _newPeriod;
        private PropertyChangedListener _listener;

        [SetUp]
        public void Setup()
        {
        	var nu = DateTime.UtcNow;
            _listener = new PropertyChangedListener();
            _target = new TimelineControlViewModel(null,new CreateLayerViewModelService());
            _nowPeriod = new DateTimePeriod(nu.Subtract(TimeSpan.FromHours(1)),nu.AddHours(1));
            _newPeriod = _nowPeriod.MovePeriod(TimeSpan.FromHours(1));
        }

        [Test]
        public void VerifyDefaultValuesAndProperties()
        {
            _listener.ListenTo(_target);
            Assert.IsNotNull(_target.TimeZoom,"Just make sure its not null");
            Assert.IsTrue(_nowPeriod.Contains(_target.NowPeriod),"Just make sure its about now");
            Assert.IsFalse(_target.ShowNowPeriod);
            Assert.IsFalse(_target.ShowNowTime);
            Assert.AreEqual(Dispatcher.CurrentDispatcher,_target.CurrentDispatcher);
            
            Assert.IsFalse(_target.ShowDate);
            _target.ShowDate = true;
            Assert.IsTrue(_target.ShowDate);
            
            Assert.AreEqual(_target.HoverWidth,TimeSpan.FromMinutes(1));
            Assert.IsFalse(_target.ShowSelectedPeriod);
            
            Assert.IsTrue(_target.ShowTickMark);
            _target.ShowTickMark = false;
            Assert.IsFalse(_target.ShowTickMark);
            
            Assert.AreEqual(_target.Resolution,TimeSpan.FromHours(1));
            _target.Resolution = TimeSpan.FromMinutes(20);
            Assert.AreEqual(_target.Resolution,TimeSpan.FromMinutes(20));

            Assert.AreEqual(_target.Interval,TimeSpan.FromMinutes(15));
            _target.Interval = TimeSpan.FromMinutes(20);
            Assert.AreEqual(_target.Interval,TimeSpan.FromMinutes(20));

            Assert.AreEqual(_target.TimeZone,TimeZoneInfo.Local);
            _target.TimeZone = TimeZoneInfo.CreateCustomTimeZone("id", TimeSpan.FromHours(1), "disp", "ddd");
            Assert.AreNotEqual(_target.TimeZone,TimeZoneInfo.Local);
            
            _listener.Clear();
            Assert.IsFalse(_target.ShowLayers);
            _target.ShowLayers = true;
            Assert.IsTrue(_target.ShowLayers);
            Assert.IsTrue(_listener.HasFired("ShowLayers"));

            _listener.Clear();
            Assert.IsFalse(_target.ShowHoverTime);
            _target.ShowHoverTime = true;
            Assert.IsTrue(_target.ShowHoverTime);
            Assert.IsTrue(_listener.HasFired("ShowHoverTime"));


            Assert.IsFalse(_target.ShowNowTime);
            //coverage:
            Assert.IsNotNull(_target.Layers);
        }

        [Test]
        public void VerifyTicMark()
        {
            //For every 00 hour in the current TimeZoneInfo, add one TickMarkDay
            _listener.ListenTo(_target);
            DateTime start = new DateTime(2001,1,1,0,0,0,DateTimeKind.Utc);
            int numberOfDays = 12;
            _target.Period = new DateTimePeriod(start,start.AddDays(numberOfDays));
            Assert.IsTrue(15 > _target.TickMarkDays.Count && _target.TickMarkDays.Count > 9);
            Assert.IsTrue(_listener.HasFired("TickMarks"));

            //Test the setters for NotifyPropertyChang
            _listener.Clear();
          

        }

        [Test]
        public void VerifySelected()
        {
            _listener.ListenTo(_target);
            _target.SelectedPeriod = _newPeriod;
            Assert.AreEqual(_target.SelectedPeriod,_newPeriod);
            Assert.IsTrue(_listener.HasFired("SelectedPeriod"));

            _target.ShowSelectedPeriod = true;
            Assert.IsTrue(_listener.HasFired("ShowSelectedPeriod"));

        }

        [Test]
        public void VerifyHover()
        {
            Assert.AreEqual(_target.HoverTime,_target.HoverPeriod.StartDateTime);
           
            _listener.ListenTo(_target);
            //By changing the HoverWidth, we are changing the HoverPeriod:
            _target.HoverWidth = TimeSpan.FromMinutes(15);
            Assert.IsTrue(_listener.HasFired("HoverPeriod"));
            _listener.Clear();
            _target.HoverTime = _target.HoverTime.AddHours(1);
            Assert.IsTrue(_listener.HasFired("HoverTime"));
            Assert.IsTrue(_listener.HasFired("HoverPeriod"));
        }

        #region nowperiod
        [Test]
        public void VerifyChangeNowTime()
        {
             DateTimePeriod NotUtcNowPeriod = _nowPeriod.MovePeriod(TimeSpan.FromDays(100));
            _target.Period = NotUtcNowPeriod;
            _target.ChangeNowTime();
            Assert.IsTrue(_nowPeriod.Contains(_target.NowPeriod),"Being outside from the targets period, nowperiod will be set to now + one minute");
            Assert.AreEqual(TimeSpan.FromMinutes(1),_target.NowPeriod.ElapsedTime());

            //If within the period, it will keep the startdateTime
            _target.Period = _nowPeriod;
            _target.ChangeNowTime();
	        var diff = Math.Abs(_target.NowPeriod.StartDateTime.Subtract(_nowPeriod.StartDateTime).TotalSeconds);
			Assert.Less(diff, 1);
            Assert.AreNotEqual(_target.NowPeriod,_nowPeriod,"Just check that it has changed the EndDateTime");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.IsFalse(System.Boolean,System.String)"), Test]
        public void VerifyNowTimer()
        {
            DateTimePeriod startPeriod = _target.NowPeriod;
            Assert.IsNull(_target.NowTimer);
         
            _target.ShowNowPeriod = true;
            //Verify properties:
            Assert.IsNotNull(_target.NowTimer,"Setting the ShowNowPeriod starts the timer");
            Assert.AreEqual(_target.NowTimer.Interval,TimeSpan.FromMinutes(1)); //We only need to update 1 time/minute
            Assert.AreEqual(_target.Dispatcher,_target.NowTimer.Dispatcher);//Make sure it operates on the "gui" thread

            Assert.IsFalse(_target.NowPeriod.Equals(startPeriod), "Should have changed the nowPeriod");
        }

       
        #endregion



    }
}
