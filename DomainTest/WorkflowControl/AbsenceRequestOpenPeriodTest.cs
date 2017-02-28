using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    public abstract class AbsenceRequestOpenPeriodTest
    {
        protected IAbsenceRequestOpenPeriod Target { get; private set; }

        [SetUp]
        public void Setup()
        {
            Target = CreateInstance();
        }

        protected abstract IAbsenceRequestOpenPeriod CreateInstance();


        [Test]
        public void VerifyPersonAccountValidations()
        {
            Assert.AreEqual(2, Target.PersonAccountValidatorList.Count);
            Assert.IsTrue(Target.PersonAccountValidatorList[0] is AbsenceRequestNoneValidator);
            Assert.IsTrue(Target.PersonAccountValidatorList[1] is PersonAccountBalanceValidator);
        }

        [Test]
        public void VerifyStaffingThresholdValidations()
        {
            Assert.AreEqual(5, Target.StaffingThresholdValidatorList.Count);
            Assert.IsTrue(Target.StaffingThresholdValidatorList[0] is AbsenceRequestNoneValidator);
            Assert.IsTrue(Target.StaffingThresholdValidatorList[1] is StaffingThresholdValidator);
            Assert.IsTrue(Target.StaffingThresholdValidatorList[2] is BudgetGroupAllowanceValidator);
            Assert.IsTrue(Target.StaffingThresholdValidatorList[3] is BudgetGroupHeadCountValidator);
            Assert.IsTrue(Target.StaffingThresholdValidatorList[4] is StaffingThresholdWithShrinkageValidator);
        }

        [Test]
        public void VerifyProcessAbsenceRequestList()
        {
            Assert.AreEqual(3, Target.AbsenceRequestProcessList.Count);
            Assert.IsTrue(Target.AbsenceRequestProcessList[0] is PendingAbsenceRequest);
            Assert.IsTrue(Target.AbsenceRequestProcessList[1] is GrantAbsenceRequest);
            Assert.IsTrue(Target.AbsenceRequestProcessList[2] is DenyAbsenceRequest);
        }

        [Test]
        public void VerifyCanChangeStaffingThresholdValidation()
        {
            Target.StaffingThresholdValidator = Target.StaffingThresholdValidatorList[1]; //Automatically creates a new instance of the validator when set!
            Assert.AreNotSame(Target.StaffingThresholdValidatorList[1],Target.StaffingThresholdValidator);
        }

        [Test]
        public void VerifyCannotSetNullAsStaffingThresholdValidator()
        {
            Assert.Throws<ArgumentNullException>(() => Target.StaffingThresholdValidator = null);
        }

        [Test]
        public void VerifyDefaultValueIsNotNull()
        {
            Assert.IsNotNull(Target.PersonAccountValidator);
            Assert.IsNotNull(Target.StaffingThresholdValidator);
            Assert.IsNotNull(Target.AbsenceRequestProcess);
        }

        [Test]
        public void VerifyCanChangePersonAccountValidation()
        {
            Target.PersonAccountValidator = Target.PersonAccountValidatorList[1]; //Automatically creates a new instance of the validator when set!
            Assert.AreNotSame(Target.PersonAccountValidatorList[1], Target.PersonAccountValidator);
        }

        [Test]
        public void VerifyCannotSetNullAsPersonAccountValidator()
        {
            Assert.Throws<ArgumentNullException>(() => Target.PersonAccountValidator = null);
        }


        [Test]
        public void VerifyCanChangeAbsenceRequestProcess()
        {
            Target.AbsenceRequestProcess = Target.AbsenceRequestProcessList[1]; //Automatically creates a new instance of the validator when set!
            Assert.AreNotSame(Target.AbsenceRequestProcessList[1], Target.AbsenceRequestProcess);
        }

        [Test]
        public void VerifyCannotSetNullAsAbsenceRequestProcess()
        {
            Assert.Throws<ArgumentNullException>(() => Target.AbsenceRequestProcess = null);
        }

        [Test]
        public void VerifyOrderIndex()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("MySet");
            IAbsenceRequestOpenPeriod target2 = CreateInstance();
            workflowControlSet.AddOpenAbsenceRequestPeriod(Target);
            workflowControlSet.AddOpenAbsenceRequestPeriod(target2);

            Assert.AreEqual(0, Target.OrderIndex);
            Assert.AreEqual(1, target2.OrderIndex);
        }

        [Test]
        public void CanGetSelectedValidatorList()
        {
            var absenceRequestValidators =
                Target.GetSelectedValidatorList();
            Assert.AreEqual(2, absenceRequestValidators.Count());
        }

        [Test]
        public void VerifyGetSelectedProcess()
        {
            IProcessAbsenceRequest processAbsenceRequest = Target.AbsenceRequestProcess;
            Assert.AreEqual(Target.AbsenceRequestProcess,processAbsenceRequest);
        }

        [Test]
        public void CanClone()
        {
            Target.SetId(Guid.NewGuid());
            IAbsenceRequestOpenPeriod clone = (IAbsenceRequestOpenPeriod) Target.Clone();
            Assert.IsFalse(clone.Id.HasValue);
            Assert.AreNotSame(Target.AbsenceRequestProcess, clone.AbsenceRequestProcess);
            Assert.AreNotSame(Target.PersonAccountValidator, clone.PersonAccountValidator);
            Assert.AreNotSame(Target.StaffingThresholdValidator, clone.StaffingThresholdValidator);
            clone = Target.NoneEntityClone();
            Assert.IsFalse(clone.Id.HasValue);
            Assert.AreNotSame(Target.AbsenceRequestProcess, clone.AbsenceRequestProcess);
            Assert.AreNotSame(Target.PersonAccountValidator, clone.PersonAccountValidator);
            Assert.AreNotSame(Target.StaffingThresholdValidator, clone.StaffingThresholdValidator);
            clone = Target.EntityClone();
            Assert.IsTrue(clone.Id.HasValue);
            Assert.AreNotSame(Target.AbsenceRequestProcess, clone.AbsenceRequestProcess);
            Assert.AreNotSame(Target.PersonAccountValidator, clone.PersonAccountValidator);
            Assert.AreNotSame(Target.StaffingThresholdValidator, clone.StaffingThresholdValidator);
        }
    }
}
