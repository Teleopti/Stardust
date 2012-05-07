using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ApproveRequestCommandHandlerTest
    {
        private MockRepository _mock;
        private IScheduleRepository _scheduleRepository;
        private IScheduleDictionarySaver _scheduleDictionarySaver;
        private IScenarioProvider _scenarioProvider;
        private IPersonRequestCheckAuthorization _authorization;
        private ISwapAndModifyService _swapAndModifyService;
        private IPersonRequestRepository _personRequestRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private ApproveRequestCommandHandler _target;
        private IScheduleDictionaryModifiedCallback _scheduleDictionaryModifiedCallback;
        private IRequestApprovalService _requestApprovalService;


        [SetUp]
        public  void Setup()
        {
            _mock = new MockRepository();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _scheduleDictionarySaver = _mock.StrictMock<IScheduleDictionarySaver>();
            _scenarioProvider = _mock.StrictMock<IScenarioProvider>();
            _authorization = _mock.StrictMock<IPersonRequestCheckAuthorization>();
            _swapAndModifyService = _mock.StrictMock<ISwapAndModifyService>();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _scheduleDictionaryModifiedCallback = _mock.StrictMock<IScheduleDictionaryModifiedCallback>();
            _requestApprovalService = _mock.StrictMock<IRequestApprovalService>();
            _target = new ApproveRequestCommandHandler(_scheduleRepository, _scheduleDictionarySaver, _scenarioProvider,
                                                       _authorization, _swapAndModifyService, _personRequestRepository,
                                                       _unitOfWorkFactory, _messageBrokerEnablerFactory,
                                                       _scheduleDictionaryModifiedCallback);
        }
            
        [Test]
        public void ApproveRequestShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
            
            IPerson person = PersonFactory.CreatePerson("Test Peson");
            person.SetId(Guid.NewGuid());
            var approveRequestCommandDto = new ApproveRequestCommandDto { PersonRequestId = Guid.NewGuid() };
            var personRequestFactory = new PersonRequestFactory();
            var absenceRequest = personRequestFactory.CreateAbsenceRequest(
                AbsenceFactory.CreateAbsence("test absence"), new DateTimePeriod());

            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var period = new DateTimePeriod(startDate, startDate.AddDays(1));
            var dictionary = new ReadOnlyScheduleDictionary(scenario, new ScheduleDateTimePeriod(period),
                new DifferenceEntityCollectionService<IPersistableScheduleData>());

            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(request.Request).Return(absenceRequest).Repeat.Times(3);
                Expect.Call(_scenarioProvider.DefaultScenario()).Return(scenario).Repeat.Twice();
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), period, null)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(_personRequestRepository.Get(approveRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(request.Approve(null, _authorization)).IgnoreArguments().Return(null);
                Expect.Call(_scheduleDictionarySaver.MarkForPersist(unitOfWork, _scheduleRepository, null)).
                    IgnoreArguments().Return(new ScheduleDictionaryPersisterResult());
                Expect.Call(()=>_scheduleDictionaryModifiedCallback.Callback(dictionary, null, null, null)).IgnoreArguments();
            }
            
            using (_mock.Playback())
            {
                _target.Handle(approveRequestCommandDto);
            }
        }
    }
}
