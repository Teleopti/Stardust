using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("BucketB")]
    public class PersonRequestReadOnlyRepositoryTest : DatabaseTest
    {
        private IRequestHistoryReadOnlyRepository _target;

        [Test]
        public void ShouldLoadRequestWithoutCrash()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();

            _target = new RequestHistoryReadOnlyRepository(UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork());
            var result = _target.LoadOnPerson(new Guid(),1, 10 );
            Assert.That(result.Count, Is.EqualTo(0));
        }

		[Test]
		public void ShouldLoadCancelledAbsenceRequestsForRequestHistory()
		{
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);

			var absence = AbsenceFactory.CreateAbsence("holiday");
			PersistAndRemoveFromUnitOfWork(absence);

			var startDateTime = new DateTime(2017, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2017, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var absenceRequest =
				new PersonRequestFactory {Person = person}.CreateAbsenceRequest(absence,
					new DateTimePeriod(startDateTime, endDateTime));
			var personRequest = absenceRequest.Parent as IPersonRequest;
			personRequest?.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			personRequest?.Cancel(new PersonRequestAuthorizationCheckerForTest());
			PersistAndRemoveFromUnitOfWork(personRequest);

			Assert.That(personRequest?.StatusText, Is.EqualTo(Resources.Cancelled));

			Session.Transaction.Commit();
			Session.Disconnect();

			_target = new RequestHistoryReadOnlyRepository(UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork());
			var result = _target.LoadOnPerson(person.Id.Value, 1, 10);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.FirstOrDefault()?.RequestStatusText, Is.EqualTo(Resources.Cancelled));

			CleanUpAfterTest();
		}
    }
}