using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	public class ApproveRequestsWithValidatorsEventHandlerTest
	{
		private IPersonRequestRepository _personRequestRepository;
		private IPersonRepository _personRepository;
		private IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private FakeQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;

		private IPerson _person;
		private IPersonRequest _personRequest;
		private ApproveRequestsWithValidatorsEvent _event;
		private ApproveRequestsWithValidatorsEventHandler _target;
		private IAbsence _absence;

		private readonly DateTime _startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateTime _endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

		private void prepareTestData(bool scheduleIsProtected, bool requestIsPending)
		{
			_personRepository = new FakePersonRepository(new FakeStorage());
			_writeProtectedScheduleCommandValidator = MockRepository.GenerateMock<IWriteProtectedScheduleCommandValidator>();
			_writeProtectedScheduleCommandValidator.Stub(x => x.ValidateCommand(new DateTime(),
				new Person(), new ApproveBatchRequestsCommand())).IgnoreArguments().Return(scheduleIsProtected);
			_personRequestRepository = new FakePersonRequestRepository();
			_queuedAbsenceRequestRepository = new FakeQueuedAbsenceRequestRepository();

			_absence = AbsenceFactory.CreateAbsence("Holiday");
			_person = createAndSetupPerson();
			_personRequest = createAbsenceRequest(_person, _absence, new DateTimePeriod(_startDateTime, _endDateTime),
				requestIsPending);

			_event = new ApproveRequestsWithValidatorsEvent
			{
				Validator = RequestValidatorsFlag.BudgetAllotmentValidator | RequestValidatorsFlag.IntradayValidator,
				PersonRequestIdList = new[] { _personRequest.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = new Guid() }
			};

			_target = new ApproveRequestsWithValidatorsEventHandler(_personRequestRepository, _writeProtectedScheduleCommandValidator,
				_queuedAbsenceRequestRepository, new FakeCurrentUnitOfWorkFactory(null));
		}

		[Test]
		public void ShouldDoNothingIfScheduleForAbsenceRequestIsWriteProtected()
		{
			prepareTestData(false, true);

			_target.Handle(_event);

			var queuedRequests = _queuedAbsenceRequestRepository.LoadAll();
			queuedRequests.Any().Should().Be.False();
		}

		[Test]
		public void ShouldDoNothingIfAbsenceRequestIsNull()
		{
			prepareTestData(true, true);

			var eventWithPersonRequestIdNotExist = new ApproveRequestsWithValidatorsEvent
			{
				Validator = RequestValidatorsFlag.BudgetAllotmentValidator,
				PersonRequestIdList = new[] { Guid.NewGuid() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = new Guid() }
			};

			_target.Handle(eventWithPersonRequestIdNotExist);

			var queuedRequests = _queuedAbsenceRequestRepository.LoadAll();
			queuedRequests.Any().Should().Be.False();
		}

		[Test]
		public void ShouldDoNothingIfAbsenceRequestIsNotPending()
		{
			prepareTestData(true, false);

			_target.Handle(_event);

			var queuedRequests = _queuedAbsenceRequestRepository.LoadAll();
			queuedRequests.Any().Should().Be.False();
		}

		[Test]
		public void ShouldQueuedIfAbsenceRequestIsSatisfied()
		{
			prepareTestData(true, true);

			_target.Handle(_event);

			var queuedRequests = _queuedAbsenceRequestRepository.LoadAll();
			queuedRequests.Count().Should().Be(1);

			var queuedRequest = queuedRequests.First();
			queuedRequest.Created.Should().Be.EqualTo(_personRequest.CreatedOn.GetValueOrDefault());
			queuedRequest.StartDateTime.Should().Be.EqualTo(_personRequest.Request.Period.StartDateTime);
			queuedRequest.EndDateTime.Should().Be.EqualTo(_personRequest.Request.Period.EndDateTime);
			queuedRequest.MandatoryValidators.Should().Be.EqualTo(_event.Validator);
		}

		[Test]
		public void ShouldUpdateQueuedRequestIfAbsenceRequestHasAlreadyQueued()
		{
			prepareTestData(true, true);
			_queuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = _personRequest.Id.GetValueOrDefault(),
				Created = DateTime.Now,
				StartDateTime = DateTime.Now,
				EndDateTime = DateTime.Now,
				MandatoryValidators = RequestValidatorsFlag.None
			});

			var newEvent = new ApproveRequestsWithValidatorsEvent
			{
				Validator = RequestValidatorsFlag.BudgetAllotmentValidator,
				PersonRequestIdList = new[] { _personRequest.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = new Guid() }
			};

			_target.Handle(newEvent);

			var queuedRequests = _queuedAbsenceRequestRepository.LoadAll();
			queuedRequests.Count().Should().Be(1);

			var queuedRequest = queuedRequests.First();
			queuedRequest.Created.Should().Be.EqualTo(_personRequest.CreatedOn.GetValueOrDefault());
			queuedRequest.StartDateTime.Should().Be.EqualTo(_personRequest.Request.Period.StartDateTime);
			queuedRequest.EndDateTime.Should().Be.EqualTo(_personRequest.Request.Period.EndDateTime);

			queuedRequest.MandatoryValidators.Should().Be.EqualTo(newEvent.Validator);
		}

		private IPerson createAndSetupPerson()
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			return person;
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod,
			bool isPending)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod)).WithId();
			if (isPending)
			{
				personRequest.ForcePending();
			}

			_personRequestRepository.Add(personRequest);

			return personRequest;
		}
	}
}