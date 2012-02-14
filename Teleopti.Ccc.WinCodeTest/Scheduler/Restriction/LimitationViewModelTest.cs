using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class LimitationViewModelTest
    {
        private ILimitationViewModel _target;
        private TimeSpan _startStart;
        private TimeSpan _startEnd;
        private StartTimeLimitation _startTimeLimitation;
        private SimpleConverter _converter;

        [SetUp]
        public void Setup()
        {
            _startStart = TimeSpan.FromHours(8);
            _startEnd = TimeSpan.FromHours(12);  
            _startTimeLimitation = new StartTimeLimitation(_startStart, _startEnd);
            _target = new LimitationViewModel(_startTimeLimitation);
            _converter = new SimpleConverter();
        }
	
        [Test]
        public void VerifyCanCreateAndPropertiesAreSet()
        {
            Assert.AreEqual(_target.Limitation, _startTimeLimitation);
            Assert.AreEqual(_target.StartTime, _startTimeLimitation.StartTimeString);
            Assert.AreEqual(_target.EndTime, _startTimeLimitation.EndTimeString);
            Assert.IsTrue(_target.Enabled);
            Assert.IsTrue(_target.Editable);
            _target.Enabled = false;
            Assert.IsFalse(_target.Enabled);
        }

        [Test]
        public void VerifyIsEditable()
        {
          
            ILimitationViewModel model1 = new LimitationViewModel(new StartTimeLimitation(TimeSpan.FromHours(14), TimeSpan.FromHours(15)));
            ILimitationViewModel model2 = new LimitationViewModel(new StartTimeLimitation(TimeSpan.FromHours(21), TimeSpan.FromHours(23)));
			_target.EndTime = model1.EndTime;
			_target.StartTime = model1.StartTime;
            Assert.AreEqual(_target.StartTime, model1.StartTime);
            Assert.AreEqual(_target.EndTime, model1.EndTime);
            _target.Editable = false;
			_target.EndTime = model2.EndTime;
            _target.StartTime = model2.EndTime;
            Assert.AreNotEqual(_target.StartTime,model2.EndTime);
            Assert.AreNotEqual(_target.EndTime,model2.EndTime);
        }

        [Test]
        public void VerifyKeepsValueIfIsNotString()
        {
            string value = _target.StartTime;
            _target.StartTime = "not a valid TimeSpan";
            _target.EndTime = "not a valid TimeSpan";
            Assert.AreEqual(value,_target.StartTime);
        }

        [Test]
        public void VerifySimpleConverter()
        {
            object o = new object();
           
            Assert.AreSame(o,_converter.Convert(o,typeof(object),null,CultureInfo.CurrentCulture));
            Assert.AreSame(o,_converter.ConvertBack(o,typeof(object),null,CultureInfo.CurrentCulture));
        }

        [Test]
        public void VerifyNotifyPropertyChanged()
        {
            Stack<string> properties = new Stack<string>();
            _target.PropertyChanged += ((sender, e) => properties.Push(e.PropertyName));
            ILimitationViewModel model =
                new LimitationViewModel(new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(15)));

            _target.Enabled = !_target.Enabled;
            Assert.AreEqual("Enabled", properties.Pop());
            _target.StartTime = model.StartTime;
            Assert.AreEqual("StartTime", properties.Pop());
            
            _target.EndTime = model.EndTime;    //Should not trigger starttime
            Assert.AreEqual("EndTime", properties.Pop()); 
           
            _target.Editable = !_target.Editable;
            _target.Editable = true;
            Assert.AreEqual("Editable", properties.Pop());
            Assert.AreEqual("Editable", properties.Pop());


            //Test that it doesnt fire if values are same
            properties.Clear();
            _target.Editable = _target.Editable;
            _target.Enabled = _target.Enabled;
            _target.StartTime = _target.StartTime;
            _target.EndTime = _target.EndTime;
            Assert.IsEmpty(properties);

        }

        [Test]
        public void VerifyEditableProperties()
        {
            Assert.IsTrue(_target.Editable);
            Assert.IsTrue(_target.EditableStartTime); //Check default
            Assert.IsTrue(_target.EditableEndTime);
            _target.Editable = false;
            Assert.IsFalse(_target.EditableStartTime); //Check based on editable
            Assert.IsFalse(_target.EditableEndTime);
            
            _target = new LimitationViewModel(_startTimeLimitation,false,false); //Check ctor settings
            Assert.IsFalse(_target.EditableStartTime);
            Assert.IsFalse(_target.EditableEndTime);
        }

        #region IsValidState
        [Test]
        public void VerifyPossibleToSetToInvalidStateWhenInvalidStatePossibleIsTrue()
        {
        
            string originEnd = _target.EndTime;
            TimeSpan startTimeGreaterThanEndTime = _startEnd.Add(TimeSpan.FromHours(1));
            string startStringGreaterThanoriginEnd = new StartTimeLimitation(_startStart, startTimeGreaterThanEndTime).EndTimeString; //Uses a limitation to create a correct timestring:
            _target.InvalidStatePossible = true;
            _target.StartTime = startStringGreaterThanoriginEnd;
            Assert.AreEqual(startStringGreaterThanoriginEnd,_target.StartTime,"StartTime set to startStringGreaterThanoriginEnd");
            Assert.AreEqual(originEnd,_target.EndTime,"EndTime not changed, because the model can have invalid state");
            Assert.IsTrue(_target.Invalid,"Model is now invalid, EndTime < StartTime");
        }

        [Test]
        public void VerifyReturnsTheLastValidIfSetInvalidState()
        {
            //PropertyChanged:
            IList<string> propertyChanges = new List<string>();
            _target.PropertyChanged += ((sender, e) => propertyChanges.Add(e.PropertyName));

            string originStart = _target.StartTime;
            string originEnd = _target.EndTime;
            _target.InvalidStatePossible = true;
           
            TimeSpan EndTimeLowerThanStartTime = _startStart.Subtract(TimeSpan.FromHours(1));
            _target.EndTime = _startTimeLimitation.StringFromTimeSpan(EndTimeLowerThanStartTime);
            Assert.IsTrue(_target.Invalid);
            Assert.IsTrue(propertyChanges.Contains("Invalid"));

            propertyChanges.Clear();
            _target.InvalidStatePossible = false;
         
            Assert.AreEqual(originStart, _target.StartTime);
            Assert.AreEqual(originEnd, _target.EndTime);
            Assert.IsTrue(propertyChanges.Contains("Invalid"));
        }

        [Test]
        public void VerifySetsTheStartEndIfSetToInvalidStatePossibleFalseValidState()
        {
            //PropertyChanged:
            IList<string> propertyChanges = new List<string>();
            _target.PropertyChanged += ((sender, e) => propertyChanges.Add(e.PropertyName));

            string originStart = _target.StartTime;
         
            _target.InvalidStatePossible = true;

            TimeSpan validEndTime = _startEnd.Add(TimeSpan.FromMinutes(5));
            _target.EndTime = _startTimeLimitation.StringFromTimeSpan(validEndTime);
            string validEndTimeString = _target.EndTime;
            Assert.IsFalse(_target.Invalid);
           
            _target.InvalidStatePossible = false;

            Assert.AreEqual(originStart, _target.StartTime);
            Assert.AreEqual(validEndTimeString, _target.EndTime);
            Assert.IsFalse(propertyChanges.Contains("Invalid"),"We have never been in invalid state"); 
        }


        #endregion
       

    }

}
