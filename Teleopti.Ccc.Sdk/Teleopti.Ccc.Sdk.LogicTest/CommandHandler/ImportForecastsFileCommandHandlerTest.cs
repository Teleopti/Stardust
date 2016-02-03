using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ImportForecastsFileCommandHandlerTest
    {
        private MockRepository _mock;
		private IMessagePopulatingServiceBusSender _busSender;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IJobResultRepository _jobResultRepository;
        private ImportForecastsFileCommandHandler _target;
        private IPerson _person;
        private ISkill _targetSkill;
        private Guid _fileId;
        private ImportForecastsFileCommandDto _importForecastsFileCommandDto;
        private IJobResult _jobResult;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IMessagePopulatingServiceBusSender>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
            _target = new ImportForecastsFileCommandHandler(_busSender,_currentUnitOfWorkFactory,_jobResultRepository);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _targetSkill = SkillFactory.CreateSkill("Test Skills");
            _targetSkill.SetId(Guid.NewGuid());

            _fileId = Guid.NewGuid();
            _importForecastsFileCommandDto = new ImportForecastsFileCommandDto
                                                 {
                                                     ImportForecastsMode =
                                                         ImportForecastsOptionsDto.ImportWorkloadAndStaffing,
                                                     TargetSkillId = _targetSkill.Id.GetValueOrDefault(),
                                                     UploadedFileId = _fileId
                                                 };
            _jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                            _person, DateTime.UtcNow);
        }

        [Test]
        public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
         
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(()=>_jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(()=>unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(()=>_busSender.Send(new ImportForecastsFileToSkill(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_importForecastsFileCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowFaultExceptionIfCommandIsNull()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.Send(new ImportForecastsFileToSkill(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(null);
            }
        }

        [Test]
        public void ShouldHandleImportForecastFileCommandSuccessfully()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.Send(new ImportForecastsFileToSkill(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_importForecastsFileCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfNotPermitted()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
            }
            using (_mock.Playback())
            {
                using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
                {
                    _target.Handle(_importForecastsFileCommandDto);
                }
            }
        }
    }
}
