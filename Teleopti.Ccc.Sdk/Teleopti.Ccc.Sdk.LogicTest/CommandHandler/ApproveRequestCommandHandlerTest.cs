using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ApproveRequestCommandHandlerTest
    {
        private MockRepository _mock;
        private IScheduleStorage _scheduleStorage;
				private IScheduleDifferenceSaver _scheduleDictionarySaver;
        private ICurrentScenario _scenarioRepository;
        private IPersonRequestCheckAuthorization _authorization;
        private ISwapAndModifyService _swapAndModifyService;
        private IPersonRequestRepository _personRequestRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ApproveRequestCommandHandler _target;
        private IPerson _person;
        private ApproveRequestCommandDto _approveRequestCommandDto;
        private PersonRequestFactory _personRequestFactory;
        private IAbsenceRequest _absenceRequest;
        private IScenario _scenario;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTimePeriod _period;
        private ShiftTradeRequest _shiftTradeRequest;
        private IPerson _personTo;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private IGlobalSettingDataRepository _globalSettingDataRepository;

        [SetUp]
        public  void Setup()
        {
            _mock = new MockRepository();
            _scheduleStorage = _mock.StrictMock<IScheduleStorage>();
						_scheduleDictionarySaver = _mock.StrictMock<IScheduleDifferenceSaver>();
			_scenarioRepository = _mock.StrictMock<ICurrentScenario>();
            _authorization = _mock.StrictMock<IPersonRequestCheckAuthorization>();
            _swapAndModifyService = _mock.StrictMock<ISwapAndModifyService>();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _globalSettingDataRepository = _mock.StrictMock<IGlobalSettingDataRepository>();
            _target = new ApproveRequestCommandHandler(_scheduleStorage, _scheduleDictionarySaver, _scenarioRepository,
                                                       _authorization, _swapAndModifyService, _personRequestRepository,
                                                       _currentUnitOfWorkFactory,
													   new DifferenceEntityCollectionService<IPersistableScheduleData>(),
                                                       _globalSettingDataRepository, null, new DoNothingScheduleDayChangeCallBack(), new FakeTimeZoneGuard());

            _person = PersonFactory.CreatePerson("Test Peson");
            _person.SetId(Guid.NewGuid());
            _approveRequestCommandDto = new ApproveRequestCommandDto { PersonRequestId = Guid.NewGuid() };
            _personRequestFactory = new PersonRequestFactory();
            _absenceRequest = _personRequestFactory.CreateAbsenceRequest(
                AbsenceFactory.CreateAbsence("test absence"), new DateTimePeriod(2000,1,1,2000,1,2));

            _personTo = PersonFactory.CreatePerson("Test Peson");
            _personTo.SetId(Guid.NewGuid());

            var shiftTradeSwapDetilList = new List<IShiftTradeSwapDetail>();
            var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, _personTo, new DateOnly(_startDate),  new DateOnly(_startDate));
            shiftTradeSwapDetilList.Add(shiftTradeSwapDetail);
            _shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetilList);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        }
            
        [Test]
        public void ApproveAbsenceRequestShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
	        var dictionary = _mock.DynamicMock<IReadOnlyScheduleDictionary>();

            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(request.Request).Return(_absenceRequest).Repeat.Times(4);
                Expect.Call(_scenarioRepository.Current()).Return(_scenario).Repeat.Twice();
				Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { _person }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(), null)).
                    IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary.Values).Return(new List<IScheduleRange>{_mock.DynamicMultiMock<IScheduleRange>(typeof(IUnvalidatedScheduleRangeUpdate))});
                Expect.Call(_personRequestRepository.Get(_approveRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(request.Approve(null, _authorization)).IgnoreArguments().Return(null);
                Expect.Call(() => _scheduleDictionarySaver.SaveChanges(null, null)).IgnoreArguments();
            }
            
            using (_mock.Playback())
            {
                _target.Handle(_approveRequestCommandDto);
            }
        }

        [Test]
        public void ApproveShiftTradeRequestShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
						var dictionary = _mock.DynamicMock<IReadOnlyScheduleDictionary>();


            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(request.Request).Return(_shiftTradeRequest).Repeat.Times(4);
                Expect.Call(_scenarioRepository.Current()).Return(_scenario).Repeat.Twice();
				Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { _person }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(), null)).
                    IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary.Values).Return(new List<IScheduleRange> { _mock.DynamicMultiMock<IScheduleRange>(typeof(IUnvalidatedScheduleRangeUpdate)) });
                Expect.Call(_personRequestRepository.Get(_approveRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(request.Approve(null, _authorization)).IgnoreArguments().Return(null);
                Expect.Call(() => _scheduleDictionarySaver.SaveChanges(null, null)).IgnoreArguments();
            }

            using (_mock.Playback())
            {
                _target.Handle(_approveRequestCommandDto);
            }
        }

        [Test]
        public void ShouldThrowExceptionIfRequestIsInvalid()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
			var dictionary = new ReadOnlyScheduleDictionary(_scenario, new ScheduleDateTimePeriod(_period),
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				CurrentAuthorization.Make());

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(request.Request).Return(_absenceRequest).Repeat.Times(4);
                Expect.Call(_scenarioRepository.Current()).Return(_scenario).Repeat.Twice();
				Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
										new[] { _person }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(), null)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(_personRequestRepository.Get(_approveRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(request.Approve(null, _authorization)).IgnoreArguments().Throw(new InvalidRequestStateTransitionException());
                Expect.Call(() => _scheduleDictionarySaver.SaveChanges(null, null)).IgnoreArguments();
            }
	        Assert.Throws<FaultException>(() =>
	        {
				using (_mock.Playback())
				{
					_target.Handle(_approveRequestCommandDto);
				}
	        });
        }
    }
}
