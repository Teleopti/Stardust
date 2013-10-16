using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

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
            var innerSpecification = (UpdatedAfterSpecification)_target.InnerSpecification; 
            Assert.IsTrue(withinRange(DateTime.UtcNow.Subtract(_timeSpan),innerSpecification.DateTime));
        }

        [Test]
        public void VerifyCallsInnerSpecification()
        {
            var inner = _mocks.StrictMock<ISpecification<IChangeInfo>>();
            _target = new PersonRequestViewModelFilter(inner);
           
            var requestFactory = new PersonRequestFactory();
            var request = requestFactory.CreateApprovedPersonRequest();

            var model = new PersonRequestViewModel(request, new ShiftTradeRequestStatusCheckerForTestDoesNothing(),null,null, null);

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
            var longTimeAgo = new DateTime(1812,1,1,1,1,1,DateTimeKind.Utc);

			var shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var factory = new PersonRequestFactory();
			var personRequestToOldButPending = factory.CreatePersonRequest();
			var personRequestToOld = factory.CreateApprovedPersonRequest();

			ReflectionHelper.SetUpdatedOn(personRequestToOldButPending, longTimeAgo);
            ReflectionHelper.SetUpdatedOn(personRequestToOld, longTimeAgo);

            Assert.IsFalse(personRequestToOld.IsPending,"Make sure this isnt pending");
            Assert.IsTrue(personRequestToOldButPending.IsPending, "Make sure its pending");

			var modelThatShouldBeFiltered = new PersonRequestViewModel(personRequestToOld,
                                                                       shiftTradeRequestStatusChecker, null,null, null);

			var modelThatShouldNotBeFiltered = new PersonRequestViewModel(personRequestToOldButPending,
                                                                       shiftTradeRequestStatusChecker, null, null, null);

            Assert.IsTrue(_target.IsSatisfiedBy(modelThatShouldBeFiltered));
            Assert.IsFalse(_target.IsSatisfiedBy(modelThatShouldNotBeFiltered));
        }

		[Test]
		public void Verify_FutureShiftTradePending_DoesNotGetFiltered()
		{
			
		}

        private static bool withinRange(DateTime dateTime,DateTime dateTimeToCompare)
        {
			var period = new DateTimePeriod(dateTime.Subtract(TimeSpan.FromDays(1)), dateTime.Add(TimeSpan.FromDays(1)));
            return period.Contains(dateTimeToCompare);
        }

       
    }
}
