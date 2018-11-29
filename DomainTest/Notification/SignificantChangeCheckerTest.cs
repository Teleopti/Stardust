using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class SignificantChangeCheckerTest
	{
		private MockRepository _mocks;
		private SignificantChangeChecker _target;
		private IPerson _person;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IScheduleDayReadModelComparer _scheduleDayReadModelComparer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();
			_scheduleDayReadModelComparer = _mocks.StrictMock<IScheduleDayReadModelComparer>();
			_target = new SignificantChangeChecker(_scheduleDayReadModelRepository, _scheduleDayReadModelComparer);
			_person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
		}

		[Test]
		public void ShouldReturnFalseIfPeriodAfterWithinFourteenDays()
		{
			var date = DateTime.Today;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(20)), new DateOnly(date.AddDays(30)) );

			Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, new ScheduleDayReadModel()).Subject, Is.EqualTo(""));
		}

		[Test]
		public void ShouldReturnFalseIfPeriodBeforeWithinFourteenDays()
		{
			var date = DateTime.Today;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(-1)));

			Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, new ScheduleDayReadModel()).Subject, Is.Empty);
		}

        [Test]
        public void ShouldReturnMessageIfChangeIsWithinFourteenDays()
        {
            var date = DateTime.Today;
            var period = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date.AddDays(2)));
            _person.WorkflowControlSet = new WorkflowControlSet("mm") { SchedulePublishedToDate = date.AddDays(7) };
            var newReadModel = new ScheduleDayReadModel
                {
                    StartDateTime = date.AddDays(1).AddHours(2),
                    EndDateTime = date.AddDays(1).AddHours(4),
                    PersonId = _person.Id.GetValueOrDefault(),
                    Workday = true,
                    Date = date.AddDays(1).Date
                };

            var oldReadModel = new ScheduleDayReadModel
                {
                    StartDateTime = date.AddDays(1).AddHours(1),
                    EndDateTime = date.AddDays(1).AddHours(3),
                    PersonId = _person.Id.GetValueOrDefault(),
                    Workday = true,
                    Date = date.AddDays(1).Date
                };

            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ForPerson(new DateOnly(date),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (oldReadModel);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, oldReadModel,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date))).Return(message);

            _mocks.ReplayAll();

			Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, newReadModel).Subject, Is.Not.Empty.And.Not.Null);
			_mocks.VerifyAll();
        }

        [Test]
		public void ShouldReturnNullIfNoOverlappingPeriod()
		{
            var date = DateTime.Today;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(15)), new DateOnly(date.AddDays(20)));
            
            Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, null).Subject, Is.Empty);
       }

        [Test]
        public void ShouldReturnMessageIfDayNotExistInOverlappingPeriod()
        {
            var date = DateTime.Today;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)));
            _person.WorkflowControlSet = new WorkflowControlSet("mm") { SchedulePublishedToDate = date.AddDays(7) };
            var newReadModel = new ScheduleDayReadModel
                {
                    StartDateTime = date.AddDays(-1).AddHours(2),
                    EndDateTime = date.AddDays(-1).AddHours(4),
                    PersonId = _person.Id.GetValueOrDefault(),
                    Workday = true,
                    Date = date.AddDays(-1).Date
                };

            var oldReadModel = new ScheduleDayReadModel
                {
                    StartDateTime = date.AddDays(1).AddHours(1),
                    EndDateTime = date.AddDays(1).AddHours(3),
                    PersonId = _person.Id.GetValueOrDefault(),
                    Workday = true,
                    Date = date.AddDays(1).Date
                };
            
            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ForPerson(new DateOnly(date.AddDays(1)),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (oldReadModel);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, oldReadModel,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(1)))).Return(message);
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, newReadModel).Subject, Is.Not.Empty.And.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnNotificationIfDayNotExistIncurrentReadModel()
        {
            var date = DateTime.Today;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)));
            _person.WorkflowControlSet = new WorkflowControlSet("mm") { SchedulePublishedToDate = date.AddDays(7) };
            var newReadModel = new ScheduleDayReadModel
                {
                    StartDateTime = date.AddDays(1).AddHours(2),
                    EndDateTime = date.AddDays(1).AddHours(4),
                    PersonId = _person.Id.GetValueOrDefault(),
                    Workday = true,
                    Date = date.AddDays(1).Date
                };

            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ForPerson(new DateOnly(date.AddDays(1)),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (null);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(1)))).Return(message);
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, newReadModel).Subject, Is.Not.Empty.And.Not.Null);
            _mocks.VerifyAll();

        }

		[Test]
		public void ShouldReturnNullIfNoPeriodIsNotWithinPublished()
		{
			var date = DateTime.Today;
			_person.WorkflowControlSet = new WorkflowControlSet("mm") { SchedulePublishedToDate = date.AddDays(7) };
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(11)), new DateOnly(date.AddDays(13)));
			
			Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, null).Subject, Is.Empty);
			
		}

		[Test]
		public void ShouldReturnNullIfPublishedToIsBeforeToday()
		{
			var date = DateTime.Today;
			_person.WorkflowControlSet = new WorkflowControlSet("mm") { SchedulePublishedToDate = date.AddDays(-7) };
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(11)), new DateOnly(date.AddDays(13)));

			Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, null).Subject, Is.Empty);

		}

        [Test]
        public void ShouldReturnNullIfNotPublished()
        {
            var date = DateTime.Today;
            _person.WorkflowControlSet = new WorkflowControlSet("mm");
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(0)), new DateOnly(date.AddDays(1)));
           
            Expect.Call(_scheduleDayReadModelRepository.ForPerson(new DateOnly(date), _person.Id.GetValueOrDefault())).Repeat.Never();

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(null, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(0)))).Repeat.Never();
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, null).Subject, Is.Empty);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnNullIfNoWorkflowControlSet()
        {
            var date = DateTime.Today;
            
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(0)), new DateOnly(date.AddDays(1)));

            Expect.Call(_scheduleDayReadModelRepository.ForPerson(new DateOnly(date), _person.Id.GetValueOrDefault())).Repeat.Never();

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(null, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(0)))).Repeat.Never();
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period.StartDate, _person, null).Subject, Is.Empty);

            _mocks.VerifyAll();
        }

	}

}