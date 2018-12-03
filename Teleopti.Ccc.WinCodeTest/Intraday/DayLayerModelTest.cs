using System;
using System.Drawing;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class DayLayerModelTest
    {
        private DateTimePeriod _period;
        private MockRepository _mocks;
        private IPerson _person;
        private ITeam _team;
        private DayLayerModel _target;
	    private LayerViewModelCollection _layerViewModelCollection;
	    private CommonNameDescriptionSetting _commonNameDescriptionSetting;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 8, 0, 0, 0, DateTimeKind.Utc), 0);
            _person = PersonFactory.CreatePerson("Kalle", "Kula");
            _person.SetEmploymentNumber("10");
            _team = _mocks.StrictMock<ITeam>();
			_layerViewModelCollection = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission());
            _commonNameDescriptionSetting = new CommonNameDescriptionSetting { AliasFormat = CommonNameDescriptionSetting.LastName };
            _target = new DayLayerModel(_person, _period, _team, _layerViewModelCollection, _commonNameDescriptionSetting);
        }

        [Test]
        public void APinnedAdapterIsPinned()
        {
            bool isPinnedSet = false;
            _target.PropertyChanged += (sender, e) => isPinnedSet = true;
            _target.IsPinned = true;
        	var targetPinned = _target.IsPinned;
            Assert.IsTrue(isPinnedSet);
			Assert.IsTrue(targetPinned);
        }

		[Test]
        public void ShouldGetCommonNameDescription()
        {
            Assert.AreEqual("Kula", _target.CommonNameDescription);
        }

		[Test]
		public void ShouldGetTeam()
		{
			Assert.AreEqual(_team, _target.Team);
		}

		[Test]
		public void ShouldReturnColorValue_WhenAlarmExists()
		{
			var eventIstriggered = false;
			_target.PropertyChanged += (s, e) => eventIstriggered = true;
			_target.HasAlarm = true;
			_target.ColorValue = 255;
			_target.ColorValue = 255;
			Assert.That(_target.ColorValue, Is.EqualTo(255));
			Assert.That(_target.Color, Is.EqualTo(Color.FromArgb(255))); 
			Assert.IsTrue(eventIstriggered);
		}

		[Test]
		public void ShouldReturnAlarmDescription()
		{
			var eventTrigger = false;
			_target.PropertyChanged += (s, e) => eventTrigger = true;
			_target.AlarmDescription = "alarm";
			_target.AlarmDescription = "alarm";
			Assert.That(_target.AlarmDescription, Is.EqualTo("alarm"));
			Assert.IsTrue(eventTrigger);
		}

		[Test]
		public void ShouldReturnRuleStartTime()
		{
			var dateTime = DateTime.Now;
			_target.RuleStartTime = dateTime;
			Assert.That(_target.RuleStartTime, Is.EqualTo(dateTime));
		}

		[Test]
		public void ShouldReturnNextActivityDescription()
		{
			var eventTrigger = false;
			_target.PropertyChanged += (s, e) => eventTrigger = true;
			_target.NextActivityDescription = "nextActivity";
			_target.NextActivityDescription = "nextActivity";
			Assert.That(_target.NextActivityDescription, Is.EqualTo("nextActivity"));
			Assert.IsTrue(eventTrigger);
		}

		[Test]
		public void ShouldReturnNextActivityStartDate()
		{
			var dateTime = DateTime.Now;
			var eventTrigger = false;
			_target.PropertyChanged += (s, e) => eventTrigger = true;
			_target.NextActivityStartDateTime = dateTime;
			_target.NextActivityStartDateTime = dateTime;
			Assert.That(_target.NextActivityStartDateTime, Is.EqualTo(dateTime));
			Assert.IsTrue(eventTrigger);
		}

		[Test]
		public void ShouldReturnScheduleStartDateTime()
		{
			var dateTime = new DateTime(2013, 01, 25);
			var evenTrigger = false;
			_target.PropertyChanged += (s, e) => evenTrigger = true;
			_target.ScheduleStartDateTime = dateTime;
			_target.ScheduleStartDateTime = dateTime;
			Assert.That(_target.ScheduleStartDateTime, Is.EqualTo(dateTime));
			Assert.IsTrue(evenTrigger);
		}

    	[Test]
    	public void ShouldReturnCurrentActivityDescription()
    	{
    		var eventTrigger = false;
    		_target.PropertyChanged += (s, e) => eventTrigger = true;
    		_target.CurrentActivityDescription = "activity";
    		_target.CurrentActivityDescription = "activity";
			Assert.That(_target.CurrentActivityDescription, Is.EqualTo("activity"));
			Assert.IsTrue(eventTrigger);
    	}

		[Test]
		public void ShouldReturnCurrentStateDescription()
		{
			var eventTrigger = false;
			_target.PropertyChanged += (s, e) => eventTrigger = true;
			_target.CurrentStateDescription = "state";
			_target.CurrentStateDescription = "state";
			Assert.That(_target.CurrentStateDescription, Is.EqualTo("state"));
			Assert.IsTrue(eventTrigger);
		}

		[Test]
		public void ShouldReturnEnteredState()
		{
			var dateTime = DateTime.Now;
			var eventTriggered = false;
			_target.PropertyChanged += (s, e) => eventTriggered = true;
			_target.EnteredCurrentState = dateTime;
			_target.EnteredCurrentState = dateTime;
			Assert.That(_target.EnteredCurrentState, Is.EqualTo(dateTime));
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void ShouldReturnStaffingEffect()
		{
			var eventTriggered = false;
			_target.PropertyChanged += (s, e) => eventTriggered = true;
			_target.StaffingEffect = 10D;
			_target.StaffingEffect = 10D;
			Assert.That(_target.StaffingEffect, Is.EqualTo(10D));
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void ShouldReturnShowNextActivityLayer()
		{
			_target.ShowNextActivity = false;
			Assert.IsFalse(_target.ShowNextActivity);
		}

		[Test]
		public void IsInEditMode_SameValue()
		{
			_target.IsInEditMode = true;
			_target.IsInEditMode = true;
			_target.IsInEditMode.Should().Be.True();
		}

		[Test]
		public void CancelEditMode_VerifyTransactionCopy()
		{
			_target.NextActivityDescription = "OldValue";
			_target.BeginEdit();
			_target.NextActivityDescription = "NewValue";
			_target.CancelEdit();
			_target.IsInEditMode.Should().Be.False();
		}

		[Test]
		public void TimeInCurrentState_WhenCompared_ShouldBeOrderedByEnteredCurrentState()
		{
			var target1 = new DayLayerModel(_person, _period, _team, _layerViewModelCollection, _commonNameDescriptionSetting);
			var target2 = new DayLayerModel(_person, _period, _team, _layerViewModelCollection, _commonNameDescriptionSetting);

			target1.EnteredCurrentState = DateTime.UtcNow.AddHours(1);
			target2.EnteredCurrentState = target1.EnteredCurrentState.AddHours(1);

			Assert.That((TimeSpan)target1.TimeInCurrentState > (TimeSpan)target2.TimeInCurrentState);
		}

		[Test]
		public void TimeInCurrentState_WhenEnterCurrentStateChange_ShouldNotifypropertyChanged()
		{
			var propertyChangedListener = new PropertyChangedListener();
			propertyChangedListener.ListenTo(_target);

			_target.EnteredCurrentState = new DateTime();

			Assert.That(propertyChangedListener.HasFired("TimeInCurrentState"));
		}

	    [Test]
	    public void ChangingHasAlarm_ShouldNotifyPropertyChanged()
	    {
			var propertyChangedListener = new PropertyChangedListener();
			propertyChangedListener.ListenTo(_target);

			_target.HasAlarm = !_target.HasAlarm;

			Assert.That(propertyChangedListener.HasFired("HasAlarm"));
	    }

	    [Test]
	    public void Color_WhenNoAlarmColorIsConnectedToTheAlarm_ShouldBeNoAlarmColor()
	    {
			var expected = Color.FromArgb(DayLayerModel.NoAlarmColorValue);

		    _target.HasAlarm = false;
		    _target.ColorValue = 0;

			Assert.That(_target.Color, Is.EqualTo(expected));

	    }

		[Test]
	    public void Color_WhenThereIsAnAlarmColorConnectedToTheAlarm_ShouldBeNoAlarmColor()
		 {
			 _target.ColorValue = 0;

			 var expected = Color.FromArgb(0);

			 _target.HasAlarm = true;

			 Assert.That(_target.Color, Is.EqualTo(expected));

		 }


		
	}
}