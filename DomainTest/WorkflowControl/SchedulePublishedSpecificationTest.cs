using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class SchedulePublishedSpecificationTest
    {
        private MockRepository mocks;
        private IWorkflowControlSet workflowControlSet;
        private ScheduleVisibleReasons viewReason;
        private SchedulePublishedSpecification target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            workflowControlSet = mocks.StrictMock<IWorkflowControlSet>();
            viewReason = ScheduleVisibleReasons.Any;
        }

        [Test]
        public void VerifyNullWorkflowControlSetMeansNotPublished()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                target = new SchedulePublishedSpecification(null, viewReason);
                Assert.IsFalse(target.IsSatisfiedBy(DateOnly.Today));
            }
        }

        [Test]
        public void VerifyPublishedDateIsUsedWhenOutsidePreferenceInputPeriod()
        {
            using (mocks.Record())
            {
                ExpectRelativePreferencePeriod();
                Expect.Call(workflowControlSet.StudentAvailabilityInputPeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(-1),
                                                                                                DateOnly.Today.AddDays(6)));
                Expect.Call(workflowControlSet.StudentAvailabilityPeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(6),
                                                                                           DateOnly.Today.AddDays(9))).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target = new SchedulePublishedSpecification(workflowControlSet, viewReason);
                Assert.IsTrue(target.IsSatisfiedBy(DateOnly.Today.AddDays(-5)));
            }
        }

        [Test]
        public void VerifyPublishedDateInsidePreferenceInputPeriod()
        {
            using (mocks.Record())
            {
                ExpectRelativePreferencePeriod();
            }
            using (mocks.Playback())
            {
                target = new SchedulePublishedSpecification(workflowControlSet, viewReason);
                Assert.IsTrue(target.IsSatisfiedBy(DateOnly.Today.AddDays(7)));
            }
        }

        [Test]
        public void VerifyPublishedDateInsideStudentAvailabilityInputPeriod()
        {
            using (mocks.Record())
            {
                ExpectRelativePreferencePeriod();
                Expect.Call(workflowControlSet.StudentAvailabilityInputPeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(-1),
                                                                                                DateOnly.Today.AddDays(11)));
                Expect.Call(workflowControlSet.StudentAvailabilityPeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(11),
                                                                                           DateOnly.Today.AddDays(15))).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target = new SchedulePublishedSpecification(workflowControlSet, viewReason);
                Assert.IsTrue(target.IsSatisfiedBy(DateOnly.Today.AddDays(12)));
            }
        }

        [Test]
        public void VerifyPublishedDateNullIsNotPublished()
        {
            using (mocks.Record())
            {
                Expect.Call(workflowControlSet.SchedulePublishedToDate).Return(null);
            }
            using (mocks.Playback())
            {
                target = new SchedulePublishedSpecification(workflowControlSet, ScheduleVisibleReasons.Published);
                Assert.IsFalse(target.IsSatisfiedBy(DateOnly.Today));
            }
        }

        private void ExpectRelativePreferencePeriod()
        {
            Expect.Call(workflowControlSet.PreferenceInputPeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(-1),
                                                                                            DateOnly.Today.AddDays(6)));
            Expect.Call(workflowControlSet.PreferencePeriod).Return(new DateOnlyPeriod(DateOnly.Today.AddDays(6),
                                                                                       DateOnly.Today.AddDays(9))).Repeat.Any();
            Expect.Call(workflowControlSet.SchedulePublishedToDate).Return(DateTime.Today.AddDays(-4)).Repeat.Any();
        }
    }
}
