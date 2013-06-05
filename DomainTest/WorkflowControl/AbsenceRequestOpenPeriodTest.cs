using System;
using System.Linq;
using NUnit.Framework;
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
            Assert.IsTrue(typeof(AbsenceRequestNoneValidator).IsInstanceOfType(Target.PersonAccountValidatorList[0]));
            Assert.IsTrue(typeof(PersonAccountBalanceValidator).IsInstanceOfType(Target.PersonAccountValidatorList[1]));
        }

        [Test]
        public void VerifyStaffingThresholdValidations()
        {
            Assert.AreEqual(3, Target.StaffingThresholdValidatorList.Count);
            Assert.IsTrue(typeof(AbsenceRequestNoneValidator).IsInstanceOfType(Target.StaffingThresholdValidatorList[0]));
            Assert.IsTrue(typeof(StaffingThresholdValidator).IsInstanceOfType(Target.StaffingThresholdValidatorList[1]));
            Assert.IsTrue(typeof(BudgetGroupAllowanceValidator).IsInstanceOfType(Target.StaffingThresholdValidatorList[2]));
        }

        [Test]
        public void VerifyProcessAbsenceRequestList()
        {
            Assert.AreEqual(3, Target.AbsenceRequestProcessList.Count);
            Assert.IsTrue(typeof(PendingAbsenceRequest).IsInstanceOfType(Target.AbsenceRequestProcessList[0]));
            Assert.IsTrue(typeof(GrantAbsenceRequest).IsInstanceOfType(Target.AbsenceRequestProcessList[1]));
            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(Target.AbsenceRequestProcessList[2]));
        }

        [Test]
        public void VerifyCanChangeStaffingThresholdValidation()
        {
            Target.StaffingThresholdValidator = Target.StaffingThresholdValidatorList[1]; //Automatically creates a new instance of the validator when set!
            Assert.AreNotSame(Target.StaffingThresholdValidatorList[1],Target.StaffingThresholdValidator);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotSetNullAsStaffingThresholdValidator()
        {
            Target.StaffingThresholdValidator = null;
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

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotSetNullAsPersonAccountValidator()
        {
            Target.PersonAccountValidator = null;
        }


        [Test]
        public void VerifyCanChangeAbsenceRequestProcess()
        {
            Target.AbsenceRequestProcess = Target.AbsenceRequestProcessList[1]; //Automatically creates a new instance of the validator when set!
            Assert.AreNotSame(Target.AbsenceRequestProcessList[1], Target.AbsenceRequestProcess);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotSetNullAsAbsenceRequestProcess()
        {
            Target.AbsenceRequestProcess = null;
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
