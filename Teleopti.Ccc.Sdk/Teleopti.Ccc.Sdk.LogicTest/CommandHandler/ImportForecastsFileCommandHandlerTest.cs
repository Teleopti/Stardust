using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ImportForecastsFileCommandHandlerTest
    {
        private MockRepository _mock;
        private IServiceBusSender _busSender;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IJobResultRepository _jobResultRepository;
        private ImportForecastsFileCommandHandler _target;
        private IPerson _person;
        private ISkill _targetSkill;
        private Guid _fileId;
        private ImportForecastsFileCommandDto _importForecastsFileCommandDto;
        private IJobResult _jobResult;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _busSender = _mock.StrictMock<IServiceBusSender>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
            _target = new ImportForecastsFileCommandHandler(_busSender,_unitOfWorkFactory,_jobResultRepository);

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
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
         
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(()=>_jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(()=>unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(()=>_busSender.NotifyServiceBus(new ImportForecastsFileToSkill())).IgnoreArguments();
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
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _busSender.NotifyServiceBus(new ImportForecastsFileToSkill())).IgnoreArguments();
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
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _busSender.NotifyServiceBus(new ImportForecastsFileToSkill())).IgnoreArguments();
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
