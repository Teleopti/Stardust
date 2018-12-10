using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestWithStaticDependenciesDONOTUSE]
	public class EffectiveRestrictionTest
    {
        private EffectiveRestriction _target;
        private StartTimeLimitation _startTimeLimitation;
        private EndTimeLimitation _endTimeLimitation;
        private WorkTimeLimitation _workTimeLimitation;
        private IShiftCategory _shiftCategory;
		private IEditableShift _commonMainShift;

        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IWorkShift _workShift4;
        private IWorkShift _workShift5;

		private IWorkShiftProjection _info1;
		private IWorkShiftProjection _info2;
		private IWorkShiftProjection _info3;
		private IWorkShiftProjection _info4;
		private IWorkShiftProjection _info5;

        private IActivity _activity;
		private CommonActivity _commonActivity;

		[SetUp]
        public void Setup()
        {
            _startTimeLimitation = new StartTimeLimitation();
            _endTimeLimitation = new EndTimeLimitation();
            _workTimeLimitation = new WorkTimeLimitation();
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Test");
			_shiftCategory.SetId(Guid.NewGuid());

            _activity = ActivityFactory.CreateActivity("Test");
			_activity.SetId(Guid.NewGuid());
            _activity.InContractTime = true;
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			_commonMainShift = EditableShiftFactory.CreateEditorShift(_activity, period, _shiftCategory);
			_commonActivity = new CommonActivity {Activity = _activity, Periods = new List<DateTimePeriod> {period}};
			//15h
            _workShift1 = WorkShiftFactory.CreateWorkShift(
                TimeSpan.FromHours(6),
                TimeSpan.FromHours(21),
                _activity,
                _shiftCategory);
            //14h
            _workShift2 = WorkShiftFactory.CreateWorkShift(
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(22),
                _activity,
                _shiftCategory);
            //16h
            _workShift3 = WorkShiftFactory.CreateWorkShift(
                TimeSpan.FromHours(9),
                TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1)),
                _activity,
                _shiftCategory);
            //22:30h
            _workShift4 = WorkShiftFactory.CreateWorkShift(
                TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45)),
                TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(15))),
                _activity,
                _shiftCategory);
            //13h
            _workShift5 = WorkShiftFactory.CreateWorkShift(
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(21),
                _activity,
                _shiftCategory);

			_info1 = WorkShiftProjection.FromWorkShift(_workShift1);
			_info2 = WorkShiftProjection.FromWorkShift(_workShift2);
			_info3 = WorkShiftProjection.FromWorkShift(_workShift3);
			_info4 = WorkShiftProjection.FromWorkShift(_workShift4);
			_info5 = WorkShiftProjection.FromWorkShift(_workShift5);

        }

        [Test]
        public void VerifyValidateWorkShiftStartEndTime()
        {
			  _startTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
			  _endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)));

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info4));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info5));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShiftWork"), Test]
        public void VerifyValidateWorkShiftWorkTime()
        {
            _workTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(17));

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info4));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info5));

        }

        [Test]
        public void VerifyValidateWorkShiftShiftCategory()
        {
            IShiftCategory notValidCategory = ShiftCategoryFactory.CreateShiftCategory("NotValid");
			notValidCategory.SetId(Guid.NewGuid());

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                _shiftCategory,
                null, null, new List<IActivityRestriction>());
			
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info4));

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                notValidCategory,
                null, null, new List<IActivityRestriction>());

            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info4));
        }

        [Test]
        public void VerifyValidateWorkShiftAbsence()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info4));

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null,
                AbsenceFactory.CreateAbsence("vacation"),
                new List<IActivityRestriction>());

            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info4));
        }

        [Test]
        public void VerifyValidateWorkShiftDayOff()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsTrue(_target.ValidateWorkShiftInfo(_info4));

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                DayOffFactory.CreateDayOff(new Description("öjf")),
                null,
                new List<IActivityRestriction>());

            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info1));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info2));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info3));
            Assert.IsFalse(_target.ValidateWorkShiftInfo(_info4));
        }

        [Test]
        public void VerifyCombineAbsence()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction result = _target.Combine(other);
            Assert.IsNull(result.Absence);

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null,
                AbsenceFactory.CreateAbsence("vacation"),
                new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.IsNotNull(result.Absence);
            result = other.Combine(_target);
            Assert.IsNotNull(result.Absence);

            IAbsence otherAbsence = AbsenceFactory.CreateAbsence("sick leave");
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, otherAbsence, new List<IActivityRestriction>());

            result = _target.Combine(other);
			Assert.IsNull(result);

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, otherAbsence, new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.AreEqual(otherAbsence, result.Absence);
        }

        [Test]
        public void VerifyCombineDayOff()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction result = _target.Combine(other);
            Assert.IsNull(result.DayOffTemplate);

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                DayOffFactory.CreateDayOff(new Description("öjf")),
                null,
                new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.IsNotNull(result.DayOffTemplate);
            result = other.Combine(_target);
            Assert.IsNotNull(result.DayOffTemplate);

            IDayOffTemplate otherDayOff = DayOffFactory.CreateDayOff(new Description("öjf"));
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                otherDayOff, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
			Assert.IsNull(result);

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                otherDayOff, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.AreEqual(otherDayOff, result.DayOffTemplate);
        }

        [Test]
        public void VerifyCombineCategory()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction result = _target.Combine(other);
            Assert.IsNull(result.ShiftCategory);

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                ShiftCategoryFactory.CreateShiftCategory("hej"),
                null, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.AreEqual("hej", result.ShiftCategory.Description.Name);
            result = other.Combine(_target);
            Assert.AreEqual("hej", result.ShiftCategory.Description.Name);

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                ShiftCategoryFactory.CreateShiftCategory("igen"),
                null, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
			Assert.IsNull(result);

            IShiftCategory cat = ShiftCategoryFactory.CreateShiftCategory("same");
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                cat,
                null, null, new List<IActivityRestriction>());

            other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                _workTimeLimitation,
                cat,
                null, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.AreEqual(cat, result.ShiftCategory);
        }

        [Test]
        public void VerifyCombineEndTimeLimitation()
        {
            _target = new EffectiveRestriction(
                _startTimeLimitation,
                new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(20)), 
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            var other = new EffectiveRestriction(
                _startTimeLimitation,
                new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17)),
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            var result = _target.Combine(other);
			Assert.IsNull(result);
        }

        [Test]
        public void VerifyCombineStartTimeLimitation()
        {
            var other = new EffectiveRestriction(
                new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(10)),
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            _target = new EffectiveRestriction(
                new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(6)),
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            var result = _target.Combine(other);
			Assert.IsNull(result);
        }

        [Test]
        public void VerifyCombineWorkTimeLimitation()
        {
            IEffectiveRestriction other = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), 
                null,
                null, null, new List<IActivityRestriction>());

            _target = new EffectiveRestriction(
                _startTimeLimitation,
                _endTimeLimitation,
                new WorkTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)),
                null,
                null, null, new List<IActivityRestriction>());

            var result = _target.Combine(other);
			Assert.IsNull(result);
        }

		[Test]
		public void ShouldCombineCommonMainShiftWhenTargetHasNoCommonMainShift()
		{
			_target = new EffectiveRestriction(
			  _startTimeLimitation,
			  _endTimeLimitation,
			  _workTimeLimitation,
			  _shiftCategory,
			  null, null, new List<IActivityRestriction>());
			
			IEffectiveRestriction other = new EffectiveRestriction(
			_startTimeLimitation,
			_endTimeLimitation,
			new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
			null,
			null, null, new List<IActivityRestriction>()) { CommonMainShift = _commonMainShift };
			
			var result = _target.Combine(other);
			Assert.That(result.CommonMainShift, Is.EqualTo(_commonMainShift));
		}
	
		[Test]
		public void ShouldCombineCommonMainShiftWhenTargetHasDifferentCommonMainShift()
		{
			_target = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				_shiftCategory,
				null, null, new List<IActivityRestriction>()) {CommonMainShift = _commonMainShift};

			var activity = ActivityFactory.CreateActivity("Test1");
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 7, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var commonMainShift = EditableShiftFactory.CreateEditorShift(activity, period, _shiftCategory);
			var other = new EffectiveRestriction(
			_startTimeLimitation,
			_endTimeLimitation,
			new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
			null,
			null, null, new List<IActivityRestriction>()) { CommonMainShift = commonMainShift };

			var result = _target.Combine(other);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldCombineCommonActivityWhenTargetHasNoCommonActivity()
		{
			_target = new EffectiveRestriction(
			  _startTimeLimitation,
			  _endTimeLimitation,
			  _workTimeLimitation,
			  _shiftCategory,
			  null, null, new List<IActivityRestriction>());
			
			IEffectiveRestriction other = new EffectiveRestriction(
			_startTimeLimitation,
			_endTimeLimitation,
			new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
			null,
			null, null, new List<IActivityRestriction>()) { CommonActivity = _commonActivity };
			
			var result = _target.Combine(other);
			Assert.That(result.CommonActivity, Is.EqualTo(_commonActivity));
		}
	
		[Test]
		public void ShouldCombineCommonActivityWhenTargetHasDifferentCommonActivities()
		{
			_target = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>()) {CommonActivity = _commonActivity};

			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var commonActivity = new CommonActivity { Activity = ActivityFactory.CreateActivity("new"), Periods = new List<DateTimePeriod> { period } };

			var other = new EffectiveRestriction(
			_startTimeLimitation,
			_endTimeLimitation,
			new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
			null,
			null, null, new List<IActivityRestriction>()) { CommonActivity = commonActivity };

			var result = _target.Combine(other);
			Assert.IsNull(result);
		}

        [Test]
        public void VerifyCombineActivityRestriction()
        {
            IActivityRestriction actRestriction = new ActivityRestriction(ActivityFactory.CreateActivity("hej"));
            IList<IActivityRestriction> actRestrictions1 = new List<IActivityRestriction>{actRestriction};
            IList<IActivityRestriction> actRestrictions2 = new List<IActivityRestriction> { actRestriction };
            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null, actRestrictions1);

            IEffectiveRestriction other = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

            Assert.AreEqual(1, _target.Combine(other).ActivityRestrictionCollection.Count);

            other = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null, actRestrictions2);

            Assert.AreEqual(2, _target.Combine(other).ActivityRestrictionCollection.Count);
        }

        [Test]
        public void VerifyConflictingEndTimeAndStartTimeLimitations()
        {
            _target = new EffectiveRestriction(
                new StartTimeLimitation(TimeSpan.FromHours(20), TimeSpan.FromHours(22)),
                _endTimeLimitation,
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            //  the whole end is before the start, we can't use it
            IEffectiveRestriction other = new EffectiveRestriction(
                _startTimeLimitation,
                new EndTimeLimitation(TimeSpan.FromHours(19), TimeSpan.FromHours(19)),
                _workTimeLimitation,
                null,
                null, null, new List<IActivityRestriction>());

            IEffectiveRestriction result = _target.Combine(other);
			Assert.IsNull(result);

            //  we must move the end start forward but the start end does not need a move because we do not have a limit on end.end
            other = new EffectiveRestriction(
               _startTimeLimitation,
               new EndTimeLimitation(TimeSpan.FromHours(16), null),
               _workTimeLimitation,
               null,
               null, null, new List<IActivityRestriction>());

            result = _target.Combine(other);
            Assert.AreEqual(TimeSpan.FromHours(20), result.EndTimeLimitation.StartTime);
        }

        [Test]
        public void VerifyValidateWithActivityRestriction()
        {
            TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
            TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13));
            _workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
            IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			lunchActivity.SetId(Guid.NewGuid());
        	var info = WorkShiftProjection.FromWorkShift(_workShift1);
            IActivityRestriction activityRestriction = new ActivityRestriction(ActivityFactory.CreateActivity("notInShift"));
            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction> {activityRestriction});

            //Activity must be in shift
            Assert.IsFalse(_target.ValidateWorkShiftInfo(info));

            activityRestriction = new ActivityRestriction(lunchActivity);
            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction> { activityRestriction });
            Assert.IsTrue(_target.ValidateWorkShiftInfo(info));
        }

		[Test]
		public void ShouldReturnFalseIfLunchStartsToLate()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.StartTimeLimitation = new StartTimeLimitation(null, TimeSpan.FromHours(11));
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));

		}

		[Test]
		public void ShouldCombineNotAvailable()
		{
			_target = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>());

			var other = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>());
			other.NotAvailable = true;
			_target = _target.Combine(other) as EffectiveRestriction;
			Assert.IsTrue(_target.NotAvailable);

			other = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>());

			_target = _target.Combine(other) as EffectiveRestriction;
			Assert.IsTrue(_target.NotAvailable);
		}

		[Test]
		public void ShouldNotCombineNullValue()
		{
			_target = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>());
			var result = _target.Combine(null);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnFalseIfSpecificStartAndLunchStartsToEarly()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(11), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(12), TimeSpan.FromHours(12));
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));
		}

		[Test]
		public void ShouldReturnFalseIfSpecificEndAndLunchStartsToLate()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(11), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(12), TimeSpan.FromHours(12));
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));
		}

		[Test]
		public void ShouldReturnFalseIfLunchStartsToEarly()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(13), null);
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));

		}

		[Test]
		public void ShouldReturnFalseIfLunchEndsToLate()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(14), null);
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));

		}

		[Test]
		public void ShouldReturnFalseIfLunchEndsToEarly()
		{
			TimePeriod fullPeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			TimePeriod lunchPeriod = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13));
			_workShift1 = WorkShiftFactory.CreateWithLunch(fullPeriod, lunchPeriod);
			IActivity lunchActivity = _workShift1.LayerCollection[1].Payload;
			var info = WorkShiftProjection.FromWorkShift(_workShift1);
			IActivityRestriction activityRestriction = new ActivityRestriction(lunchActivity);
			activityRestriction.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(12));
			_target = new EffectiveRestriction(new StartTimeLimitation(null, null), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction> { activityRestriction });

			Assert.IsFalse(_target.ValidateWorkShiftInfo(info));

		}

        [Test]
        public void CanCheckIfVisualLayerCollectionSatisfiesActivityRestriction()
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            IActivity activity = ActivityFactory.CreateActivity("lunch");
			activity.SetId(Guid.NewGuid());
            IActivity activity2 = ActivityFactory.CreateActivity("another one");
			activity2.SetId(Guid.NewGuid());
            var activityRestriction = new ActivityRestriction(activity);
            activityRestriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(11, 0, 0), null);
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(12, 0, 0));
            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction> { activityRestriction });

            DateOnly dateOnly = new DateOnly(2009, 2, 2);
            var factory = new VisualLayerFactory();
            var layer1 = factory.CreateShiftSetupLayer(activity2,
                                         new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
                                                            new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc)));
            var layerLunch = factory.CreateShiftSetupLayer(activity,
                                         new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
                                                            new DateTime(2009, 2, 2, 13, 0, 0, DateTimeKind.Utc)));

			var layerCollection = new WorkShiftProjectionLayer[] { };
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

        	layerCollection = new[]
        	                  	{
        	                  		new WorkShiftProjectionLayer
        	                  			{
        	                  				Period = layer1.Period,
        	                  				ActivityId = layer1.Payload.Id.Value
        	                  			},
        	                  		new WorkShiftProjectionLayer
        	                  			{
        	                  				Period = layerLunch.Period,
        	                  				ActivityId = layerLunch.Payload.Id.Value
        	                  			}
        	                  	};
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(12, 0, 0), null);
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(13, 0, 0));
            Assert.IsTrue(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0));
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(13, 0, 0));
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.StartTimeLimitation = new StartTimeLimitation(null, null);
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(13, 30, 0), new TimeSpan(15, 0, 0));
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.StartTimeLimitation = new StartTimeLimitation(null, null);
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(null, null);
            activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(1, 30, 0), null);
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(0, 30, 0));
            Assert.IsFalse(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));

            activityRestriction.WorkTimeLimitation = new WorkTimeLimitation();
            Assert.IsTrue(_target.VisualLayerCollectionSatisfiesActivityRestriction(dateOnly, timeZoneInfo, layerCollection));
        }

        [Test]
        public void VerifyIsLimitedWorkday()
        {
            IActivity activity = ActivityFactory.CreateActivity("lunch");
            var activityRestriction = new ActivityRestriction(activity);
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(9);

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, new DayOffTemplate(new Description()), null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), ShiftCategoryFactory.CreateShiftCategory("hej"), null, null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(!(_target.ShiftCategory != null || _target.NotAvailable));

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction> { activityRestriction });
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

            _target = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(startTime, endTime),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(startTime, endTime), null, null, null,
                                               new List<IActivityRestriction>());
            Assert.IsFalse(_target.ShiftCategory != null || _target.NotAvailable);

        }

		[Test]
		public void ShouldMayMatchWithShifts()
		{
			_target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
											   new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction>());
			_target.MayMatchWithShifts().Should().Be.True();
		}

		[Test]
		public void ShouldMayMatchBlacklistedShifts()
		{
			_target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
											   new WorkTimeLimitation(), null, null, null,
											   new List<IActivityRestriction>());
			_target.MayMatchBlacklistedShifts().Should().Be.False();
		}

		[Test]
		public void ShouldMatchShiftCategory()
		{
			_target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
											   new WorkTimeLimitation(), new ShiftCategory("Late"), null, null,
											   new List<IActivityRestriction>());
			_target.Match(new ShiftCategory("Late")).Should().Be.False();
		}

		[Test]
		public void ShouldMatchWorkShiftProjection()
		{
			_startTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
			_endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)));

			_target = new EffectiveRestriction(
				_startTimeLimitation,
				_endTimeLimitation,
				_workTimeLimitation,
				null,
				null, null, new List<IActivityRestriction>());

			Assert.IsFalse(_target.Match(_info1));
		}

        [Test]
        public void VerifyHashCode()
        {
            IActivity activity = ActivityFactory.CreateActivity("lunch").WithId();
            var activityRestriction = new ActivityRestriction(activity).WithId();
			var otherActivityRestriction = new ActivityRestriction(activity).WithId();
			var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(9);
            var cat = ShiftCategoryFactory.CreateShiftCategory("katt").WithId();
            _target = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), cat, null, null,
                                               new List<IActivityRestriction>());

            
           var  other = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                              new WorkTimeLimitation(), cat, null, null,
                                              new List<IActivityRestriction>());
        	other.NotAvailable = true;
			Assert.AreNotEqual(_target.GetHashCode(), other.GetHashCode());
        	other.NotAvailable = false;
			Assert.AreEqual(_target.GetHashCode(), other.GetHashCode());
        	other.IsAvailabilityDay = true;
			Assert.AreNotEqual(_target.GetHashCode(), other.GetHashCode());
			other.IsAvailabilityDay = false;


            Assert.AreEqual(_target.GetHashCode(), other.GetHashCode());

            _target = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction>{activityRestriction});

            other = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(),
                                               new WorkTimeLimitation(), null, null, null,
                                               new List<IActivityRestriction>{otherActivityRestriction});

            Assert.AreEqual(_target.GetHashCode(), other.GetHashCode());

            Assert.IsTrue(_target.Equals(other));
        }
    }
}
