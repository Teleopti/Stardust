using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class IsEditablePredicateTest
    {
        private IsEditablePredicate _target;
        private DateOnlyPeriod _thisMonth;
        private DateOnlyPeriod _nextMonth;
        private DateOnlyPeriod _nextNextMonth;

        [SetUp]
        public void Setup()
        {
            _target = new IsEditablePredicate(() => new DateTime(2010, 5, 27));
            _thisMonth = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);
            _nextMonth = new DateOnlyPeriod(2010, 6, 1, 2010, 6, 30);
            _nextNextMonth = new DateOnlyPeriod(2010, 7, 1, 2010, 7, 31);
        }

        [Test]
        public void ShouldReturnTrueWhenPersonHasAWorkflowControlSetWithPreferencePeriodsInRange()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.PreferencePeriod = _nextMonth;
            workflowControlSet.PreferenceInputPeriod = _thisMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsPreferenceEditable(_nextMonth.StartDate, person), Is.True);
            Assert.That(_target.IsPreferenceEditable(_nextMonth.StartDate.AddDays(15), person), Is.True);
            Assert.That(_target.IsPreferenceEditable(_nextMonth.EndDate, person), Is.True);
        }
        
        [Test]
        public void ShouldReturnTrueWhenPersonHasAWorkflowControlSetWithStudentAvailabilityPeriodsInRange()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.StudentAvailabilityPeriod = _nextMonth;
            workflowControlSet.StudentAvailabilityInputPeriod = _thisMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsStudentAvailabilityEditable(_nextMonth.StartDate, person), Is.True);
            Assert.That(_target.IsStudentAvailabilityEditable(_nextMonth.StartDate.AddDays(15), person), Is.True);
            Assert.That(_target.IsStudentAvailabilityEditable(_nextMonth.EndDate, person), Is.True);
        }

        [Test]
        public void ShouldReturnFalseWhenPersonHasNoWorkflowControlSet()
        {
            var person = PersonFactory.CreatePerson("name");
            person.WorkflowControlSet = null;

            Assert.That(_target.IsPreferenceEditable(_nextMonth.StartDate, person), Is.False);
            Assert.That(_target.IsStudentAvailabilityEditable(_nextMonth.StartDate, person), Is.False);
        }

        [Test]
        public void ShouldReturnFalseWhenPreferenceInputPeriodIsInTheFuture()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.PreferencePeriod = _nextNextMonth;
            workflowControlSet.PreferenceInputPeriod = _nextMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsPreferenceEditable(_nextNextMonth.StartDate, person), Is.False);
        }
        
        [Test]
        public void ShouldReturnFalseWhenStudentAvailabilityInputPeriodIsInTheFuture()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.StudentAvailabilityPeriod = _nextNextMonth;
            workflowControlSet.StudentAvailabilityInputPeriod = _nextMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsStudentAvailabilityEditable(_nextNextMonth.StartDate, person), Is.False);
        }

        [Test]
        public void ShouldReturnFalseWhenPreferencePeriodIsClosed()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.PreferencePeriod = _nextNextMonth;
            workflowControlSet.PreferenceInputPeriod = _thisMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsPreferenceEditable(_nextMonth.StartDate, person), Is.False);
        }
        
        [Test]
        public void ShouldReturnFalseWhenStudentAvailabilityPeriodIsClosed()
        {
            var person = PersonFactory.CreatePerson("name");
            var workflowControlSet = new WorkflowControlSet("description");
            workflowControlSet.StudentAvailabilityPeriod = _nextNextMonth;
            workflowControlSet.StudentAvailabilityInputPeriod = _thisMonth;
            person.WorkflowControlSet = workflowControlSet;

            Assert.That(_target.IsStudentAvailabilityEditable(_nextMonth.StartDate, person), Is.False);
        }
    }
}