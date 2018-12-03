using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class WorkflowControlSetModelTest
    {
        private WorkflowControlSetModel _target;
        private IWorkflowControlSet _domainEntity;

        [SetUp]
        public void Setup()
        {
            _domainEntity = new WorkflowControlSet("description");
            _target = new WorkflowControlSetModel(_domainEntity);
        }

        [Test]
        public void VerifyHasOriginalDomainObject()
        {
            Assert.IsNotNull(_target.OriginalDomainEntity);
            Assert.AreSame(_domainEntity,_target.OriginalDomainEntity);
            Assert.AreNotSame(_domainEntity, _target.DomainEntity);
        }

        [Test]
        public void VerifyIdAndIsNew()
        {
            // No Id
            Assert.IsFalse(_target.Id.HasValue);
            Assert.IsTrue(_target.IsNew);

            // Id
            _domainEntity.SetId(Guid.NewGuid());
            _target = new WorkflowControlSetModel(_domainEntity);
            Assert.IsTrue(_target.Id.HasValue);
            Assert.IsFalse(_target.IsNew);
            Assert.AreEqual(_domainEntity.Id, _target.Id);
        }

        [Test]
        public void VerifyName()
        {
            Assert.AreEqual(_domainEntity.Name, _target.Name);
            _target.Name = "new desc";
            Assert.AreEqual("new desc", _target.Name);
        }

        [Test]
        public void VerifyUpdatedInfo()
        {
            // Existing entity
            _domainEntity.SetId(Guid.NewGuid());

            var updatedDate = new DateTime(2010, 2, 2, 9, 12, 0);
            var person = PersonFactory.CreatePerson("kalle","kula");
            ReflectionHelper.SetUpdatedBy(_domainEntity, person);
            ReflectionHelper.SetUpdatedOn(_domainEntity, updatedDate);

            _target = new WorkflowControlSetModel(_domainEntity);
            Assert.AreEqual(getUpdateInfo(_domainEntity), _target.UpdatedInfo);

            // Newly created entity
            _domainEntity = new WorkflowControlSet("new name");
            _target = new WorkflowControlSetModel(_domainEntity);
            Assert.IsEmpty(_target.UpdatedInfo);
        }

        [Test]
        public void VerifyToBeDeleted()
        {
            Assert.IsFalse(_target.ToBeDeleted);
            _target.ToBeDeleted = true;
            Assert.IsTrue(_target.ToBeDeleted);
        }        
		 
		 [Test]
        public void VerifyAnonymousTradingSet()
        {
            Assert.IsFalse(_target.AnonymousTrading);
				_target.AnonymousTrading = true;
				Assert.IsTrue(_target.AnonymousTrading);
        }		 
		 
		 [Test]
        public void VerifyLockTradingSet()
        {
            Assert.IsFalse(_target.LockTrading);
				_target.LockTrading = true;
				Assert.IsTrue(_target.LockTrading);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCanGetDefaultInstancesForType()
        {
            Assert.AreEqual(2, WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters.Count);
            Assert.IsTrue(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item is AbsenceRequestOpenDatePeriod);
            Assert.AreEqual(Resources.FromTo, WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].DisplayText);
            Assert.IsTrue(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item is AbsenceRequestOpenRollingPeriod);
            Assert.AreEqual(Resources.Rolling, WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].DisplayText);
            Assert.IsTrue(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item is AbsenceRequestOpenDatePeriod);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyDefaultValuesForAbsenceRequestPeriods()
        {
	        var datePeriod =
		        (AbsenceRequestOpenDatePeriod) WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item;
	        var rollingDatePeriod =
		        (AbsenceRequestOpenRollingPeriod) WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item;

            var todayDateOnly = DateOnly.Today;
            var inNextMonthDateOnly = new DateOnly(todayDateOnly.Date.AddMonths(1));

            var startDateOnly = new DateOnly(inNextMonthDateOnly.Year, inNextMonthDateOnly.Month, 1);
            var endDateOnly = new DateOnly(startDateOnly.Date.AddMonths(1).AddDays(-1));
            var requestPeriod = new DateOnlyPeriod(startDateOnly, endDateOnly);
            // Default values for date period of requests is next month
            Assert.AreEqual(requestPeriod, datePeriod.Period);

            // Default values for rolling period of requests is between 2 to 15 days
            Assert.AreEqual(new MinMax<int>(2, 15), rollingDatePeriod.BetweenDays);

            startDateOnly = new DateOnly(todayDateOnly.Year, todayDateOnly.Month, 1);
            endDateOnly = new DateOnly(startDateOnly.Date.AddMonths(1).AddDays(-1));
            var openForRequestPeriod = new DateOnlyPeriod(startDateOnly, endDateOnly);
            // Default values for period to make requests is current month
            Assert.AreEqual(openForRequestPeriod, datePeriod.OpenForRequestsPeriod);
            Assert.AreEqual(openForRequestPeriod, rollingDatePeriod.OpenForRequestsPeriod);

            // Default value for check staffing is Intraday
            Assert.AreEqual(new StaffingThresholdValidator(), datePeriod.StaffingThresholdValidator);
            Assert.AreEqual(new StaffingThresholdValidator(), rollingDatePeriod.StaffingThresholdValidator);
        }

	    [Test]
	    public void VerifyCanGetAbsenceRequestPeriodList()
	    {
		    _target.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod());
		    _target.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod());

		    var targetPeriodModels = _target.AbsenceRequestPeriodModels;
		    var periodAdapters = WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters;

		    Assert.AreEqual(2, _target.AbsenceRequestPeriodModels.Count);
		    Assert.IsTrue(targetPeriodModels[0].PeriodType.Equals(periodAdapters[0]));
		    Assert.IsTrue(targetPeriodModels[1].PeriodType.Equals(periodAdapters[1]));
		    Assert.IsFalse(targetPeriodModels[0].PeriodType.Equals(periodAdapters[1]));
		    Assert.IsFalse(targetPeriodModels[1].PeriodType.Equals(periodAdapters[0]));
		    Assert.AreEqual(Resources.FromTo, _target.AbsenceRequestPeriodModels[0].PeriodType.DisplayText);
	    }

	    [Test]
        public void VerifyCanGetAndSetPeriodFromModel()
        {
            _target.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
            {
	            Period = new DateOnlyPeriod(2010,6,1,2010,8,31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2010,3,1,2010,3,31)
            });
            _target.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
            {
	            BetweenDays = new MinMax<int>(2,14),
				OpenForRequestsPeriod = new DateOnlyPeriod(2010,1,1,2010,12,31)
            });

            var absenceRequestPeriodList = _target.AbsenceRequestPeriodModels;
            var datePeriod = absenceRequestPeriodList[0];
            var rollingPeriod = absenceRequestPeriodList[1];

            Assert.IsFalse(datePeriod.RollingStart.HasValue);
            Assert.IsFalse(datePeriod.RollingEnd.HasValue);
            Assert.AreEqual(new DateOnly(2010,6,1),datePeriod.PeriodStartDate);
            Assert.AreEqual(new DateOnly(2010, 8, 31), datePeriod.PeriodEndDate);
            Assert.AreEqual(new DateOnly(2010, 3, 1), datePeriod.OpenStartDate);
            Assert.AreEqual(new DateOnly(2010, 3, 31), datePeriod.OpenEndDate);

            Assert.IsFalse(rollingPeriod.PeriodStartDate.HasValue);
            Assert.IsFalse(rollingPeriod.PeriodEndDate.HasValue);
            Assert.AreEqual(2, rollingPeriod.RollingStart);
            Assert.AreEqual(14, rollingPeriod.RollingEnd);
            Assert.AreEqual(new DateOnly(2010, 1, 1), rollingPeriod.OpenStartDate);
            Assert.AreEqual(new DateOnly(2010, 12, 31), rollingPeriod.OpenEndDate);

            datePeriod.PeriodStartDate = datePeriod.PeriodStartDate.Value.AddDays(365);
            Assert.AreEqual(new DateOnly(2011, 6, 1), datePeriod.PeriodStartDate);
            Assert.AreEqual(new DateOnly(2011, 6, 1), datePeriod.PeriodEndDate);

            datePeriod.PeriodEndDate = datePeriod.PeriodStartDate.Value.AddDays(-365);
            Assert.AreEqual(new DateOnly(2010, 6, 1), datePeriod.PeriodStartDate);
            Assert.AreEqual(new DateOnly(2010, 6, 1), datePeriod.PeriodEndDate);

            rollingPeriod.RollingStart = 16;
            Assert.AreEqual(16,rollingPeriod.RollingStart);
            Assert.AreEqual(16, rollingPeriod.RollingEnd);

            rollingPeriod.RollingEnd = 3;
            Assert.AreEqual(3, rollingPeriod.RollingStart);
            Assert.AreEqual(3, rollingPeriod.RollingEnd);

            datePeriod.OpenStartDate = datePeriod.OpenStartDate.AddDays(365);
            Assert.AreEqual(new DateOnly(2011, 3, 1), datePeriod.OpenStartDate);
            Assert.AreEqual(new DateOnly(2011, 3, 1), datePeriod.OpenEndDate);

            datePeriod.OpenEndDate = datePeriod.OpenStartDate.AddDays(-365);
            Assert.AreEqual(new DateOnly(2010, 3, 1), datePeriod.OpenStartDate);
            Assert.AreEqual(new DateOnly(2010, 3, 1), datePeriod.OpenEndDate);
        }

        [Test]
        public void VerifyOtherPropertiesOnPeriod()
        {
            _target.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod());
            Assert.AreEqual(_target.DomainEntity.AbsenceRequestOpenPeriods[0].AbsenceRequestProcessList.Count,
				_target.AbsenceRequestPeriodModels[0].AbsenceRequestProcessList.Count);
            Assert.AreEqual(_target.DomainEntity.AbsenceRequestOpenPeriods[0].PersonAccountValidatorList.Count,
				_target.AbsenceRequestPeriodModels[0].PersonAccountValidatorList.Count);
            Assert.AreEqual(_target.DomainEntity.AbsenceRequestOpenPeriods[0].StaffingThresholdValidatorList.Count,
				_target.AbsenceRequestPeriodModels[0].StaffingThresholdValidatorList.Count);
            Assert.AreEqual(new PendingAbsenceRequest(),_target.AbsenceRequestPeriodModels[0].AbsenceRequestProcess);
            _target.AbsenceRequestPeriodModels[0].AbsenceRequestProcess = new GrantAbsenceRequest();
            Assert.AreEqual(new GrantAbsenceRequest(), _target.AbsenceRequestPeriodModels[0].AbsenceRequestProcess);
            Assert.AreEqual(new AbsenceRequestNoneValidator(), _target.AbsenceRequestPeriodModels[0].PersonAccountValidator);
            _target.AbsenceRequestPeriodModels[0].PersonAccountValidator = new PersonAccountBalanceValidator();
            Assert.AreEqual(new PersonAccountBalanceValidator(), _target.AbsenceRequestPeriodModels[0].PersonAccountValidator);
            Assert.AreEqual(new AbsenceRequestNoneValidator(), _target.AbsenceRequestPeriodModels[0].StaffingThresholdValidator);
            _target.AbsenceRequestPeriodModels[0].StaffingThresholdValidator = new StaffingThresholdValidator();
            Assert.AreEqual(new StaffingThresholdValidator(), _target.AbsenceRequestPeriodModels[0].StaffingThresholdValidator);

            var newAbsence = AbsenceFactory.CreateAbsence("Holiday - Hourly");
            _target.AbsenceRequestPeriodModels[0].Absence = newAbsence;
            Assert.AreEqual(newAbsence,_target.AbsenceRequestPeriodModels[0].Absence);
        }

        [Test]
        public void VerifyAllowedPreferenceActivityIsNullDefault()
        {
            Assert.IsNull(_target.AllowedPreferenceActivity);
        }

        [Test]
        public void VerifyAllowedPreferenceActivityCanBeSet()
        {
            var activity = ActivityFactory.CreateActivity("Lunch");
            _target.AllowedPreferenceActivity = activity;
            Assert.AreEqual(activity, _target.AllowedPreferenceActivity);
        }

        [Test]
        public void VerifyPreferenceInputPeriod()
        {
            var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
            _target.PreferenceInputPeriod = insertPeriod;
            Assert.AreEqual(insertPeriod, _target.PreferenceInputPeriod);
        }
        [Test]
        public void VerifyPreferencePeriod()
        {
            var preferencetPeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);
            _target.PreferencePeriod = preferencetPeriod;
            Assert.AreEqual(preferencetPeriod, _target.PreferencePeriod);
        }
        
        [Test]
        public void VerifyStudentAvailabilityInputPeriod()
        {
            var insertPeriod = new DateOnlyPeriod(2008, 4, 1, 2009, 4, 30);
            _target.StudentAvailabilityInputPeriod = insertPeriod;
            Assert.AreEqual(insertPeriod, _target.StudentAvailabilityInputPeriod);
        }
        [Test]
        public void VerifyStudentAvailabilityPeriod()
        {
            var studentAvailabilityPeriod = new DateOnlyPeriod(2008, 5, 1, 2009, 5, 31);
            _target.StudentAvailabilityPeriod = studentAvailabilityPeriod;
            Assert.AreEqual(studentAvailabilityPeriod, _target.StudentAvailabilityPeriod);
        }

        [Test]
        public void VerifyPublishedDateToCanBeSet()
        {
            DateTime? dateTime = DateTime.Now;
            _target.SchedulePublishedToDate = dateTime;
            Assert.AreEqual(dateTime, _target.SchedulePublishedToDate);
            Assert.AreEqual(dateTime, _target.DomainEntity.SchedulePublishedToDate);
            _target.SchedulePublishedToDate = null;
            Assert.IsNull(_target.SchedulePublishedToDate);
        }

        [Test]
        public void VerifyShiftTradePeriodCanBeSet()
        {
            var periodDays = new MinMax<int>(-10, 20);
            _target.ShiftTradeOpenPeriodDays = periodDays;

            Assert.AreEqual(periodDays, _target.ShiftTradeOpenPeriodDays);
        }

        [Test]
        public void CanAddAndRemoveAllowedShiftCategories()
        {
            var shiftCat = new ShiftCategory("cat");

            Assert.AreEqual(0, _target.AllowedPreferenceShiftCategories.Count());
            _target.AddAllowedPreferenceShiftCategory(shiftCat);
            Assert.AreEqual(1, _target.AllowedPreferenceShiftCategories.Count());
            _target.RemoveAllowedPreferenceShiftCategory(shiftCat);
            Assert.AreEqual(0, _target.AllowedPreferenceShiftCategories.Count());
        }

        [Test]
        public void CanAddAndRemoveAllowedAbsences()
        {
            var absence = new Absence();

            Assert.AreEqual(0, _target.AllowedPreferenceAbsences.Count());
            _target.AddAllowedPreferenceAbsence(absence);
            Assert.AreEqual(1, _target.AllowedPreferenceAbsences.Count());
            _target.RemoveAllowedPreferenceAbsence(absence);
            Assert.AreEqual(0, _target.AllowedPreferenceAbsences.Count());
        }

		[Test]
		public void CanAddAndRemoveAllowedAbsencesForReport()
		{
			var absence = new Absence();

			Assert.AreEqual(0, _target.AllowedAbsencesForReport.Count());
			_target.AddAllowedAbsenceForReport(absence);
			Assert.AreEqual(1, _target.AllowedAbsencesForReport.Count());
			_target.RemoveAllowedAbsenceForReport(absence);
			Assert.AreEqual(0, _target.AllowedAbsencesForReport.Count());
		}

        [Test]
        public void CanAddAndRemoveAllowedDayOffs()
        {
            var template = new DayOffTemplate(new Description("temp"));
            Assert.AreEqual(0, _target.AllowedPreferenceDayOffs.Count());
            _target.AddAllowedPreferenceDayOff(template);
            Assert.AreEqual(1, _target.AllowedPreferenceDayOffs.Count());
            _target.RemoveAllowedPreferenceDayOff(template);
            Assert.AreEqual(0, _target.AllowedPreferenceDayOffs.Count());
        }

        [Test]
        public void VerifyShiftTradeTargetTimeFlexibility()
        {
            var flexibility = new TimeSpan(0, 12, 0);

            _target.ShiftTradeTargetTimeFlexibility = flexibility;
            Assert.AreEqual(flexibility, _target.ShiftTradeTargetTimeFlexibility);
        }

        private static string getUpdateInfo(IChangeInfo domainEntity)
        {
            var localizer = new LocalizedUpdateInfo();
            var changed = localizer.UpdatedByText(domainEntity, Resources.UpdatedByColon);
            
            return changed;
        }
    }
}
