using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class PersonRequestViewModelFilterTest
    {
        private TimeSpan _timeSpan;
        private PersonRequestViewModelFilter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _timeSpan = TimeSpan.FromDays(40);
            _mocks=new MockRepository();
        }

        [Test]
        public void VerifyInnerSpecificationDateTime()
        {
            _target = new PersonRequestViewModelFilter(_timeSpan);
            UpdatedAfterSpecification innerSpecification = (UpdatedAfterSpecification)_target.InnerSpecification; 
            Assert.IsTrue(WithinRange(DateTime.UtcNow.Subtract(_timeSpan),innerSpecification.DateTime));
        }

        [Test]
        public void VerifyCallsInnerSpecification()
        {
            ISpecification<IChangeInfo> inner = _mocks.StrictMock<ISpecification<IChangeInfo>>();
            _target = new PersonRequestViewModelFilter(inner);
           
            PersonRequestFactory requestFactory = new PersonRequestFactory();
            IPersonRequest request = requestFactory.CreateApprovedPersonRequest();

            PersonRequestViewModel model = new PersonRequestViewModel(request, new ShiftTradeRequestStatusCheckerForTestDoesNothing(),null,null, null);

            using(_mocks.Record())
            {
                Expect.Call(inner.IsSatisfiedBy(request)).Return(true);
                Expect.Call(inner.IsSatisfiedBy(request)).Return(false);
            }
            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(model),"Returns false if inner specification returns true, because we use it for filtering OUT");
                Assert.IsTrue(_target.IsSatisfiedBy(model), "Returns true if inner specification returns false, because we use it for filtering OUT");
            }
        }

        [Test]
        public void VerifyPendingDoesNotGetFiltered()
        {
            _target = new PersonRequestViewModelFilter(TimeSpan.FromDays(1));
            DateTime longTimeAgo = new DateTime(1812,1,1,1,1,1,DateTimeKind.Utc);
           
            IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
            PersonRequestFactory factory = new PersonRequestFactory();
            IPersonRequest personRequestToOldButPending = factory.CreatePersonRequest();
            IPersonRequest personRequestToOld = factory.CreateApprovedPersonRequest();


            ReflectionHelper.SetUpdatedOn(personRequestToOldButPending, longTimeAgo);
            ReflectionHelper.SetUpdatedOn(personRequestToOld, longTimeAgo);

            Assert.IsFalse(personRequestToOld.IsPending,"Make sure this isnt pending");
            Assert.IsTrue(personRequestToOldButPending.IsPending, "Make sure its pending");

            PersonRequestViewModel modelThatShouldBeFiltered = new PersonRequestViewModel(personRequestToOld,
                                                                       shiftTradeRequestStatusChecker, null,null, null);

            PersonRequestViewModel modelThatShouldNotBeFiltered = new PersonRequestViewModel(personRequestToOldButPending,
                                                                       shiftTradeRequestStatusChecker, null, null, null);


            Assert.IsTrue(_target.IsSatisfiedBy(modelThatShouldBeFiltered));
            Assert.IsFalse(_target.IsSatisfiedBy(modelThatShouldNotBeFiltered));
            


            
        }

        private static bool WithinRange(DateTime dateTime,DateTime dateTimeToCompare)
        {
            DateTimePeriod period = new DateTimePeriod(dateTime.Subtract(TimeSpan.FromDays(1)),dateTime.Add(TimeSpan.FromDays(1)));
            return period.Contains(dateTimeToCompare);
        }

       
    }
}
