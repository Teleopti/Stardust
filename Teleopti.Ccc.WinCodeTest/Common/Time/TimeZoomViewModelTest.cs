using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class TimeZoomViewModelTest
    {
        private TimeZoomViewModel _target;
        private DateTimePeriod _period;
        private DateTimePeriod _utcNowPeriod; //simple checking against now, no need to check exact.
        private MinMax<double> _inTheMiddleWithTolerance; //simple checking , no need to check exact.

        [SetUp]
        public void Setup()
        {
            _inTheMiddleWithTolerance=new MinMax<double>(0.45,0.55);
            DateTime baseDateTime = new DateTime(2001,1,1,0,0,0,DateTimeKind.Utc);

            _period = new DateTimePeriod(baseDateTime,baseDateTime.AddDays(1));
            _utcNowPeriod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),DateTime.UtcNow.AddHours(1));
            _target = new TimeZoomViewModel(_period);
        }

        [Test]
        public void VerifyDefaultValues()
        {
            double width = _target.Period.ElapsedTime().TotalMinutes*_target.MinuteWidth;
            Assert.AreEqual(_period,_target.Period);
            Assert.AreEqual(width,_target.PanelWidth);
            Assert.AreEqual(0d, _target.ScrollPosition);
            CheckUtcNow(_target.ScrollDateTime);
        }

        [Test]
        public void VerifyDefaultZoomLevels()
        {
            Assert.AreEqual(3,_target.ZoomLevels.Count);
            Assert.AreEqual(0.5,_target.ZoomLevels[0].MinuteWidth,"Width of firstZoomLevel");
            Assert.AreEqual(1,_target.ZoomLevels[1].MinuteWidth,"Width of second ZoomLevel");
            Assert.AreEqual(2,_target.ZoomLevels[2].MinuteWidth,"Width of third ZoomLevel");
            Assert.AreEqual(_target.ZoomLevels[1],_target.ZoomLevel,"Second zoomlevel is default");
            Assert.AreEqual(_target.ZoomLevel.MinuteWidth, _target.MinuteWidth, "MinuteWidth is from ZoomLevel");
        }

        [Test]
        public void VerifyZoomInCommand()
        {
            TesterForCommandModels models = new TesterForCommandModels();
            Assert.IsTrue(models.CanExecute(_target.ZoomInCommand));
            models.ExecuteCommandModel(_target.ZoomInCommand);
            Assert.AreEqual(_target.ZoomLevels[0],_target.ZoomLevel);
            Assert.IsFalse(models.CanExecute(_target.ZoomInCommand));
        }

        [Test]
        public void VerifyZoomOutCommand()
        {
            TesterForCommandModels models = new TesterForCommandModels();
            Assert.IsTrue(models.CanExecute(_target.ZoomOutCommand));
            models.ExecuteCommandModel(_target.ZoomOutCommand);
            Assert.AreEqual(_target.ZoomLevels[2],_target.ZoomLevel);
            Assert.IsFalse(models.CanExecute(_target.ZoomOutCommand));
        }

        [Test]
        public void VerifyScrollToDateTimeCommand()
        {
           
            //By setting the ScrollDateTime to StartDateTime, we should get 0, EndDateTime = 1, and in between = 0.5.
            TesterForCommandModels models = new TesterForCommandModels();
            _target.ScrollDateTime = _target.Period.EndDateTime.AddDays(1);
            Assert.IsFalse(models.CanExecute(_target.ScrollToDateTimeCommand), "ScrollDateTime is outside the Period");

            //MAX
            _target.ScrollDateTime = _target.Period.EndDateTime;
            Assert.IsTrue(models.CanExecute(_target.ScrollToDateTimeCommand), "Should be able to scroll to EndDateTime");
            models.ExecuteCommandModel(_target.ScrollToDateTimeCommand);
            Assert.AreEqual(1d, _target.ScrollPosition, "Position scrolled to max");

            //MIN
            _target.ScrollDateTime = _target.Period.StartDateTime;
            Assert.IsTrue(models.CanExecute(_target.ScrollToDateTimeCommand),
                          "Should be able to scroll to StartDateTime");
            models.ExecuteCommandModel(_target.ScrollToDateTimeCommand);
            Assert.AreEqual(0d, _target.ScrollPosition, "Position scrolled to min");

            //MIDDLE
            double minutes = _target.Period.ElapsedTime().TotalMinutes/2;
            DateTime inTheMiddle = _target.Period.StartDateTime.AddMinutes(minutes);
            _target.ScrollDateTime = inTheMiddle;
            Assert.IsTrue(models.CanExecute(_target.ScrollToDateTimeCommand));
            models.ExecuteCommandModel(_target.ScrollToDateTimeCommand);
           CheckInTheMiddle(_target.ScrollPosition);
        }

        [Test]
        public void VerifyScrollToUtcNow()
        {
            TesterForCommandModels models = new TesterForCommandModels();
            Assert.IsFalse(models.CanExecute(_target.ScrollToNowCommand));
            _target.Period = _utcNowPeriod;
            Assert.IsTrue(models.CanExecute(_target.ScrollToNowCommand));
            models.ExecuteCommandModel(_target.ScrollToNowCommand);
            CheckUtcNow(_target.ScrollDateTime);
           
            models.ExecuteCommandModel(_target.ScrollToNowCommand);
            CheckUtcNow(_target.ScrollDateTime);
            CheckInTheMiddle(_target.ScrollPosition);

        }


        [Test]
        public void VerifyPanelScrollPosition()
        {
            //Changing the scrollposition or panelwidth should change the panelscrollposition
            _target.PanelWidth = 1000;
            _target.ScrollPosition = 0.5;
            Assert.AreEqual(1000*0.5,_target.PanelScrollPosition);
            
            //Check that propertychange fires to the PanelScrollPosition, since thats what we want to bind to
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.ScrollPosition = 0.3;
            Assert.IsTrue(listener.HasFired("PanelScrollPosition"));
            listener.Clear();
            _target.PanelWidth = 500;
            Assert.IsTrue(listener.HasFired("PanelScrollPosition"));
        }

        #region helpers
        private void CheckUtcNow(DateTime toCheck)
        {
            Assert.IsTrue(_utcNowPeriod.Contains(toCheck));
        }

        private void CheckInTheMiddle(double toCheck)
        {
            Assert.IsTrue(_inTheMiddleWithTolerance.Contains(toCheck));
        }


        #endregion

    }
}
