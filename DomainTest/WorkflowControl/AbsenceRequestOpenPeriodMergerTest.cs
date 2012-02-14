using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class AbsenceRequestOpenPeriodMergerTest
    {
        private IAbsenceRequestOpenPeriodMerger _target;

        [SetUp]
        public void Setup()
        {
            _target = new AbsenceRequestOpenPeriodMerger();
        }

        [Test]
        public void VerifyCanMergeProcess()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //Make pending
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { AbsenceRequestProcess = period1.AbsenceRequestProcessList[1]}; //Grant

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> {period1, period2});

            Assert.IsTrue(typeof(PendingAbsenceRequest).IsInstanceOfType(mergedPeriod.AbsenceRequestProcess));
        }

        [Test]
        public void VerifyCanMergeProcessWithDeny()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //Make pending
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { AbsenceRequestProcess = period1.AbsenceRequestProcessList[2] }; //Deny

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(mergedPeriod.AbsenceRequestProcess));
        }

        [Test]
        public void VerifyCanMergePersonAccountValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { PersonAccountValidator= period1.PersonAccountValidatorList[1] }; //Check person account

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(typeof(PersonAccountBalanceValidator).IsInstanceOfType(mergedPeriod.PersonAccountValidator));
        }

        [Test]
        public void VerifyCanMergeStaffingThresholdValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { StaffingThresholdValidator = period1.StaffingThresholdValidatorList[1] }; //Check staffing thresholds

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(typeof(StaffingThresholdValidator).IsInstanceOfType(mergedPeriod.StaffingThresholdValidator));
        }

        [Test]
        public void VerifyCanMergeBudgetGroupAllowanceValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { StaffingThresholdValidator = period1.StaffingThresholdValidatorList[2] }; //Check staffing thresholds

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(typeof(BudgetGroupAllowanceValidator).IsInstanceOfType(mergedPeriod.StaffingThresholdValidator));
        }

        [Test]
        public void VerifyCanMergeThreePeriods()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            period1.StaffingThresholdValidator = period1.StaffingThresholdValidatorList[1];
            period1.PersonAccountValidator = period1.PersonAccountValidatorList[1];
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { StaffingThresholdValidator = period1.StaffingThresholdValidatorList[1] }; //Check staffing thresholds
            IAbsenceRequestOpenPeriod period3 = new AbsenceRequestOpenDatePeriod();
            period3.AbsenceRequestProcess = period3.AbsenceRequestProcessList[1];


            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2, period3 });

            Assert.IsTrue(typeof(StaffingThresholdValidator).IsInstanceOfType(mergedPeriod.StaffingThresholdValidator));
            Assert.IsTrue(typeof(PersonAccountBalanceValidator).IsInstanceOfType(mergedPeriod.PersonAccountValidator));
            Assert.IsFalse(typeof(GrantAbsenceRequest).IsInstanceOfType(mergedPeriod.AbsenceRequestProcess));
        }

        [Test]
        public void VerifyWhenPeriodListEmpty()
        {
            IAbsenceRequestOpenPeriod mergedPeriod = _target.Merge(new List<IAbsenceRequestOpenPeriod>());
            Assert.IsTrue(typeof(AbsenceRequestNoneValidator).IsInstanceOfType(mergedPeriod.StaffingThresholdValidator));
            Assert.IsTrue(typeof(AbsenceRequestNoneValidator).IsInstanceOfType(mergedPeriod.PersonAccountValidator));
            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(mergedPeriod.AbsenceRequestProcess));
        }
    }
}
