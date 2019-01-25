using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DateTimePeriodViewModelTest
    {
        private DateTimePeriodViewModel _target;
        private TesterForCommandModels _commandTester;
       

        [SetUp]
        public void Setup()
        {
            _target = new DateTimePeriodViewModel();
            _commandTester = new TesterForCommandModels();
        }

        #region DependencyProperties
        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(DateTime.SpecifyKind(DateTime.MaxValue,DateTimeKind.Utc), _target.Max, "Maximum is set to utc max");
            Assert.AreEqual(DateTime.SpecifyKind(DateTime.MinValue,DateTimeKind.Utc), _target.Min, "Minimum is set to utc min");
            Assert.AreEqual(TimeSpan.Zero,_target.Interval);
        }

        [Test]
        public void VerifyDependencyProperties()
        {
            DateTime baseDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Min = baseDate;
            _target.Max = baseDate.AddDays(3);
            _target.Start = baseDate.AddDays(1);
            _target.End = baseDate.AddDays(2);
            Assert.AreEqual(baseDate, _target.Min, "verify Min");
            Assert.AreEqual(baseDate.AddDays(1), _target.Start, "verify Start");
            Assert.AreEqual(baseDate.AddDays(2), _target.End, "verify End");
            Assert.AreEqual(baseDate.AddDays(3), _target.Max, "verify Max");
            Assert.AreEqual(new DateTimePeriod(baseDate.AddDays(1), baseDate.AddDays(2)), _target.DateTimePeriod, "verify DateTimeperiod");
            Assert.AreEqual(new MinMax<DateTime>(baseDate, baseDate.AddDays(3)), _target.MinMax, "verify MinMax");
        }

        [Test]
        public void VerifySettingMinHigherThanStartChangesStart()
        {
            DateTime baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Start = baseDateTime;
            _target.Min = baseDateTime.AddDays(1);
            Assert.AreEqual(baseDateTime.AddDays(1), _target.Start, "StartDateTime coerced by min");
        }

        [Test]
        public void VerifyIntervalIsAddedToValue()
        {
            DateTime baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Min = baseDateTime;
            _target.Max = baseDateTime.AddDays(5);
            _target.End = baseDateTime.AddDays(3);
            _target.Start = baseDateTime.AddDays(3);
            Assert.AreEqual(TimeSpan.Zero, _target.Start.Subtract(_target.End));
            _target.Interval = TimeSpan.FromMinutes(15);
            Assert.AreEqual(_target.Interval, _target.End.Subtract(_target.Start));
            _target.End = baseDateTime.AddDays(3);
            Assert.AreEqual(baseDateTime.AddDays(3).Subtract(_target.Interval),_target.Start);
        }

        #endregion //DependencyProperties

        [Test]
        public void VerifyDateTimeViewModels()
        {
            Assert.AreEqual(_target.DateTimePeriod.StartDateTime, _target.StartDateTimeViewModel.DateTime);
            Assert.AreEqual(_target.DateTimePeriod.EndDateTime, _target.EndDateTimeViewModel.DateTime);
        }

        [Test]
        public void VerifyIsSameDateUtc()
        {
            DateTime start = new DateTime(2001,1,1,5,0,0,DateTimeKind.Utc);
            DateTime end = start.Add(TimeSpan.FromHours(18)); //Same Date UTC
            _target.Start = start;
            _target.End = end;
            Assert.IsTrue(_target.SameDateUtc);
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.End = end.AddHours(2);
            Assert.IsFalse(_target.SameDateUtc);
            Assert.IsTrue(listener.HasFired("SameDateUtc"));
        }

        [Test]
        public void VerifySameDateLocal()
        {
            DateTime start = new DateTime(2001, 1, 1, 5, 0, 0, DateTimeKind.Local);
            DateTime end = start.Add(TimeSpan.FromHours(18)); //Same Date UTC
            _target.StartDateTimeAsLocal = start;
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.EndDateTimeAsLocal = end;
            Assert.IsTrue(_target.SameDateLocal);
            _target.EndDateTimeAsLocal = end.AddHours(2);
            Assert.IsFalse(_target.SameDateLocal);
            Assert.IsTrue(listener.HasFired("SameDateLocal"));
        }

        [Test]
        public void VerifyMoveStartDateTimeCommands()
        {
            DateTime start = new DateTime(2001,1,1,0,0,0,DateTimeKind.Utc);
            DateTime end = start.AddHours(10);

            _target.Interval = TimeSpan.FromMinutes(15);
            _target.Start = start;
            _target.End = end;

            //Verify the RoutedCommands:
            Assert.AreEqual(_target.MoveStartTimeEarlierCommand.Command,CommonRoutedCommands.MoveStartOneIntervalEarlier);
            Assert.AreEqual(_target.MoveStartTimeLaterCommand.Command,CommonRoutedCommands.MoveStartOneIntervalLater);

            Assert.IsTrue(_commandTester.CanExecute(_target.MoveStartTimeEarlierCommand),"No limits for now, so it should be able to execute");
            Assert.IsTrue(_commandTester.CanExecute(_target.MoveStartTimeLaterCommand), "No limits for now, so it should be able to execute");


            _commandTester.ExecuteCommandModel(_target.MoveStartTimeEarlierCommand);
            Assert.AreEqual(_target.Start,start.Subtract(_target.Interval),"The starttime should have been moved by the intervallength");

            _commandTester.ExecuteCommandModel(_target.MoveStartTimeLaterCommand);
            Assert.AreEqual(_target.Start, start, "The starttime should have been moved forward by the intervallength");
        }

        [Test]
        public void VerifyMoveEndTimeCommands()
        {
            DateTime start = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = start.AddHours(10);

            _target.Interval = TimeSpan.FromMinutes(15);
            _target.Start = start;
            _target.End = end;

            //Verify the RoutedCommands:
            Assert.AreEqual(_target.MoveEndTimeEarlierCommand.Command, CommonRoutedCommands.MoveEndOneIntervalEarlier);
            Assert.AreEqual(_target.MoveEndTimeLaterCommand.Command, CommonRoutedCommands.MoveEndOneIntervalLater);

            //Verify CanExecute
            Assert.IsTrue(_commandTester.CanExecute(_target.MoveEndTimeEarlierCommand),"CanExecute can always execute");
            Assert.IsTrue(_commandTester.CanExecute(_target.MoveEndTimeLaterCommand), "CanExecute can always execute");

            _commandTester.ExecuteCommandModel(_target.MoveEndTimeEarlierCommand);
            Assert.AreEqual(_target.End, end.Subtract(_target.Interval), "The endtime should have been moved by the intervallength");

            //Move back by executing the movelatercommand
            _commandTester.ExecuteCommandModel(_target.MoveEndTimeLaterCommand);
            Assert.AreEqual(_target.End, end, "The endtime should have been moved forward by the intervallength (back to the origin)");
        }

        [Test]
        public void VerifyMovePeriodCommands()
        {
            DateTime start = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = start.AddHours(10);

            _target.Interval = TimeSpan.FromMinutes(15);
            _target.Start = start;
            _target.End = end;

            //Verify the RoutedCommands:
            Assert.AreEqual(_target.MovePeriodEarlierCommand.Command, CommonRoutedCommands.MovePeriodOneIntervalEarlier);
            Assert.AreEqual(_target.MovePeriodLaterCommand.Command, CommonRoutedCommands.MovePeriodOneIntervalLater);

            //Verify CanExecute
            Assert.IsTrue(_commandTester.CanExecute(_target.MovePeriodEarlierCommand), "CanExecute can always execute");
            Assert.IsTrue(_commandTester.CanExecute(_target.MovePeriodLaterCommand), "CanExecute can always execute");


            _commandTester.ExecuteCommandModel(_target.MovePeriodEarlierCommand);
            Assert.AreEqual(_target.End, end.Subtract(_target.Interval), "The endtime should have been moved by the intervallength");
            Assert.AreEqual(_target.Start, start.Subtract(_target.Interval), "The starttime should have been moved by the intervallength");

            //Move back by executing the movelatercommand
            _commandTester.ExecuteCommandModel(_target.MovePeriodLaterCommand);
            Assert.AreEqual(_target.Start, start, "The starttime should have been moved forward by the intervallength (back to the origin)");
            Assert.AreEqual(_target.End, end, "The endtime should have been moved forward by the intervallength (back to the origin)");
        }


        [Test]
        public void VerifyToggleAutoUpdateCommand()
        {
            bool d = _target.AutoUpdate; //default value
            _commandTester.ExecuteCommandModel(_target.ToggleAutoUpdatePeriod);

            Assert.AreNotEqual(_target.AutoUpdate, d, "autoupdate toggled from default value");

            _commandTester.ExecuteCommandModel(_target.ToggleAutoUpdatePeriod);

            Assert.AreEqual(_target.AutoUpdate, d, "autoupdate toggled back to default");
        }

        [Test]
        public void VerifyToggleCommandCanExecute()
        {
            Assert.IsTrue(_commandTester.CanExecute(_target.ToggleAutoUpdatePeriod));
        }

        [Test]
        public void VerifyToggleRoutedCommand()
        {
            Assert.AreSame(_target.ToggleAutoUpdatePeriod.Command, CommonRoutedCommands.ToggleAutoUpdate);
        }

        #region selectables
        
        [Test]
        public void VerifySelectableTimeSpans()
        {
            _target.Interval = TimeSpan.FromMinutes(15);
            var dateTimeInterval = new DateTime(1901, 1, 1, _target.Interval.Hours, _target.Interval.Minutes,
                                    _target.Interval.Minutes);
            IList<String> selectables = _target.SelectableStartTimes.SourceCollection as IList<String>;  
            Assert.AreEqual(selectables[0],DateTime.MinValue.ToShortTimeString(),"should always start on 00:00");
            Assert.AreEqual(selectables[1], dateTimeInterval.ToShortTimeString(), "Step of intervals is the Interval");
        }

        [Test]
        public void VerifySelectableTimeSpansChangesIfIntervalIsChanged()
        {
            _target.Interval = TimeSpan.FromMinutes(15);
            var dateTimeInterval = new DateTime(1901, 1, 1, _target.Interval.Hours, _target.Interval.Minutes,
                                    _target.Interval.Minutes);
            IList<String> selectables = _target.SelectableStartTimes.SourceCollection as IList<String>;
            Assert.AreEqual(selectables[1], dateTimeInterval.ToShortTimeString(), "Step of intervals is the Interval");

            _target.Interval = TimeSpan.FromMinutes(10);
            var dateTimeShortInterval = new DateTime(1901, 1, 1, _target.Interval.Hours, _target.Interval.Minutes,
                                    _target.Interval.Minutes);
            selectables = _target.SelectableStartTimes.SourceCollection as IList<String>;
            Assert.AreEqual(selectables[1], dateTimeShortInterval.ToShortTimeString(), "The steps should have been recalculated to use 10 minutes intervals");
        }

        [Test]
        public void VerifySelectableTimeSpanMinimum()
        {
            _target.Interval = TimeSpan.FromMinutes(1);
            
            Assert.AreEqual(TimeSpan.FromMinutes(5), _target.SelectableTimes[2].Subtract(_target.SelectableTimes[1]),
                            "Minimum should be 5 minutes");
        }

        [Test]
        public void VerifyMaximumSelectableTimeSpan()
        {
            _target.Interval = TimeSpan.FromDays(1);
         
            Assert.AreEqual(TimeSpan.FromHours(1), _target.SelectableTimes[2].Subtract(_target.SelectableTimes[1]),
                            "Maximum should be 1 hour");
        }

        [Test]
        public void VerifyPreSelectableMinimum()
        {
            _target.Interval = TimeSpan.Zero;
            var dateTimeMinimumInterval = new DateTime(1901, 1, 1, DateTimePeriodViewModel.MinimumInterval.Hours, DateTimePeriodViewModel.MinimumInterval.Minutes,
                                    DateTimePeriodViewModel.MinimumInterval.Minutes);
            var selectables = _target.SelectableStartTimes.SourceCollection as IList<String>;
            if (selectables != null)
            {
                Assert.AreEqual(selectables[0], DateTime.MinValue.ToShortTimeString(), "should always start on 00:00");
                Assert.AreEqual(selectables[1], dateTimeMinimumInterval.ToShortTimeString(), "It should have automated minimum");
            }
        }

        #endregion

        #region ValidState
        [Test]
        public void VerifyInvalidState()
        {
            Assert.IsTrue(_target.IsValid,"Should be true default");
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            Assert.IsTrue(_target.AutoUpdate,"Should be on by default");

            _target.AutoUpdate = false;
            Assert.IsFalse(_target.AutoUpdate,"AutoUpdate should be set to false");
            Assert.IsTrue(listener.HasFired("AutoUpdate"),"AutoUpdate has triggered propertychanged");

            _target.Start = _target.End.AddDays(1);
            Assert.IsTrue(_target.Start>_target.End,"It should now be in illegalState");

            Assert.IsFalse(_target.IsValid,"Model should no longer be valid since the starttime is later than the endtime");
            Assert.IsTrue(listener.HasFired("IsValid"),"Model should have fired PropertyChanged for IsValid");
        }

        [Test]
        public void VerifyDataError()
        {
            //In valid state, it should return null;
            string anyProperty = "thisCouldBeAnyProperty";
            Assert.IsTrue(_target.IsValid);
            var result = ((IDataErrorInfo)_target)[anyProperty];
            Assert.IsNull(result,"Should always return null when model is valid");

            _target.AutoUpdate = false;
            _target.Start = _target.End.AddDays(1);
            Assert.IsFalse(_target.IsValid,"Should now be invalid");
            Assert.AreEqual(UserTexts.Resources.StartDateMustBeSmallerThanEndDate,((IDataErrorInfo)_target)[anyProperty]);
        }

        [Test]
        public void VerifyBackToLegalStateWhenAutoUpdateIsOn()
        {
            _target.AutoUpdate = false;
            _target.Start = _target.End.AddDays(1);

            _target.AutoUpdate = true;
            Assert.IsTrue(_target.Start<=_target.End);
        }


        #endregion //ValidState
        #region Utc
        [Test]
        public void VerifyMinUtc()
        {
            Assert.Throws<ArgumentException>(() => _target.Min = new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Local));
        }

        [Test]
        public void VerifyMaxUtc()
        {
            Assert.Throws<ArgumentException>(() => _target.Max = new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Local));
        }

        [Test]
        public void VerifyStartUtc()
        {
            Assert.Throws<ArgumentException>(() => _target.Start = new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Local));
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-09
        /// </remarks>
        [Test]
        public void VerifyEndUtc()
        {
            Assert.Throws<ArgumentException>(() => _target.End = new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Local));
        }
        #endregion

        #region DataBindings
        [Test]
        public void VerifyNotifyPropertyChangedMinMax()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.Min = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(listener.HasFired("MinMax"), "MinMax Fired when changing Min");
        
            listener.Clear();
            _target.Max = new DateTime(2001, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(listener.HasFired("DateTimePeriod"),  "DateTimePeriod Fired when changing max to less than actual");
            Assert.IsTrue(listener.HasFired("MinMax"), "MinMax Fired when changing max");
        }

        [Test]
        public void VerifyNotifyPropertyChangedStartEnd()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.Start = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(listener.HasFired("DateTimePeriod"), "StartEnd Fired when changing Start");

           listener.Clear();
            _target.End = new DateTime(2001, 1, 4, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(listener.HasFired("DateTimePeriod"), "StartEnd Fired when changing End");
        }

        [Test]
        public void VerifyStartIsCoercedToMax()
        {
            DateTime baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Min = baseDateTime;
            _target.Max = baseDateTime.AddDays(3);
            _target.Start = baseDateTime.AddDays(5);
            Assert.AreEqual(baseDateTime.AddDays(3),_target.Start);
        }

        [Test]
        public void VerifyEndValueIsCoercedToMin()
        {
            DateTime baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Min = baseDateTime.AddDays(3);
            _target.Max = baseDateTime.AddDays(5);
            _target.End = baseDateTime.AddDays(2);
            Assert.AreEqual(baseDateTime.AddDays(3), _target.End);
        }

        [Test]
        public void VerifyEndChangesIfStartIsSetToHigher()
        {
            DateTime baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.Min = baseDateTime;
            _target.Max = baseDateTime.AddDays(5);
            _target.End = baseDateTime.AddDays(2);
            _target.Start= baseDateTime.AddDays(3);
            Assert.AreEqual(baseDateTime.AddDays(3), _target.End);
        }

        #endregion //DataBindings

        #region for winformbindings
        [Test]
        public void VerifyCanGetAndSetLocalStartAndEndDateTimes()
        {
            int hours = 2;
            DateTime baseDate = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTime localBaseDate = TimeZoneHelper.ConvertFromUtc(baseDate, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            _target.Start = baseDate;
            _target.End = baseDate.AddHours(hours);
            Assert.AreEqual(localBaseDate, _target.StartDateTimeAsLocal,"Converts start to local");
            Assert.AreEqual(localBaseDate.AddHours(hours), _target.EndDateTimeAsLocal,"Converts end to local");
            _target.StartDateTimeAsLocal = localBaseDate;
            _target.EndDateTimeAsLocal= localBaseDate.AddHours(hours);
            Assert.AreEqual(localBaseDate, _target.StartDateTimeAsLocal, "Can set local start");
            Assert.AreEqual(localBaseDate.AddHours(hours), _target.EndDateTimeAsLocal, "Can set local end");
        }

        [Test]
        public void VerifyCanSetAndGetTimeAsTimeSpanForBindingToOutlookTimePicker()
        {
            int startHours = 8;
            int hours = 5;
            DateTime baseDate = new DateTime(2001, 1, 1, startHours, 0, 0, DateTimeKind.Local);
            _target.StartDateTimeAsLocal = baseDate;
            _target.EndDateTimeAsLocal = baseDate.AddHours(hours);
            Assert.AreEqual(TimeSpan.FromHours(startHours), _target.StartTimeAsLocalTimeSpan);
            Assert.AreEqual(TimeSpan.FromHours(startHours + hours), _target.EndTimeAsLocalTimeSpan);

            _target.EndTimeAsLocalTimeSpan = TimeSpan.FromHours(15);
            _target.StartTimeAsLocalTimeSpan = TimeSpan.FromHours(11);

            Assert.AreEqual(11,_target.StartTimeAsLocalTimeSpan.TotalHours);
            Assert.AreEqual(15,_target.EndTimeAsLocalTimeSpan.TotalHours);
        }

        #endregion //for winformbindings

        [Test]
        public void ShouldTreatTimeBeforeBoundaryStartAsInvalid()
        {
            _target.AutoUpdate = false;
            _target.Min = new DateTime(2001,1,1,5,0,0,DateTimeKind.Utc);
            _target.Start = new DateTime(2001, 1, 1, 5, 0, 0, DateTimeKind.Utc).AddHours(-1);
            Assert.IsFalse(_target.IsValid);
        }

        [Test]
        public void ShouldTreatTimeAfterBoundaryEndAsInvalid()
        {
            _target.AutoUpdate = false;
            _target.Max = new DateTime(2001, 1, 1, 5, 0, 0, DateTimeKind.Utc);
            _target.Start = _target.Max.AddHours(1);
            _target.End = _target.Max.AddHours(3);
            Assert.IsFalse(_target.IsValid);
        }
    }
}

