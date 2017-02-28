﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class AbsenceRequestOpenPeriodMergerTest
    {
        private AbsenceRequestOpenPeriodMerger _target;

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

            Assert.IsTrue(mergedPeriod.AbsenceRequestProcess is PendingAbsenceRequest);
        }

        [Test]
        public void VerifyCanMergeProcessWithDeny()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //Make pending
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { AbsenceRequestProcess = period1.AbsenceRequestProcessList[2] }; //Deny

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(mergedPeriod.AbsenceRequestProcess is DenyAbsenceRequest);
        }

        [Test]
        public void VerifyCanMergePersonAccountValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { PersonAccountValidator= period1.PersonAccountValidatorList[1] }; //Check person account

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(mergedPeriod.PersonAccountValidator is PersonAccountBalanceValidator);
        }

        [Test]
        public void VerifyCanMergeStaffingThresholdValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { StaffingThresholdValidator = period1.StaffingThresholdValidatorList[1] }; //Check staffing thresholds

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(mergedPeriod.StaffingThresholdValidator is StaffingThresholdValidator);
        }

        [Test]
        public void VerifyCanMergeBudgetGroupAllowanceValidator()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod(); //No check
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod { StaffingThresholdValidator = period1.StaffingThresholdValidatorList[2] }; //Check staffing thresholds

            IAbsenceRequestOpenPeriod mergedPeriod =
                _target.Merge(new List<IAbsenceRequestOpenPeriod> { period1, period2 });

            Assert.IsTrue(mergedPeriod.StaffingThresholdValidator is BudgetGroupAllowanceValidator);
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

            Assert.IsTrue(mergedPeriod.StaffingThresholdValidator is StaffingThresholdValidator);
            Assert.IsTrue(mergedPeriod.PersonAccountValidator is PersonAccountBalanceValidator);
            Assert.IsFalse(mergedPeriod.AbsenceRequestProcess is GrantAbsenceRequest);
        }

        [Test]
        public void VerifyWhenPeriodListEmpty()
        {
            IAbsenceRequestOpenPeriod mergedPeriod = _target.Merge(new List<IAbsenceRequestOpenPeriod>());
            Assert.IsTrue(mergedPeriod.StaffingThresholdValidator is AbsenceRequestNoneValidator);
            Assert.IsTrue(mergedPeriod.PersonAccountValidator is AbsenceRequestNoneValidator);
            Assert.IsTrue(mergedPeriod.AbsenceRequestProcess is DenyAbsenceRequest);
        }
    }
}
