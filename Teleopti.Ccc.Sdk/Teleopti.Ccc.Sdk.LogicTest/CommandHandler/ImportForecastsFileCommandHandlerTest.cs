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

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _busSender = _mock.StrictMock<IServiceBusSender>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
            _target = new ImportForecastsFileCommandHandler(_busSender,_unitOfWorkFactory,_jobResultRepository);
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var targetSkill = SkillFactory.CreateSkill("Test Skills");
            targetSkill.SetId(Guid.NewGuid());

            var fileId = Guid.NewGuid();
           
            var importForecastsFileCommandDto = new ImportForecastsFileCommandDto();
            importForecastsFileCommandDto.ImportForecastsMode = ImportForecastsOptionsDto.ImportWorkloadAndStaffing;
            importForecastsFileCommandDto.TargetSkillId = targetSkill.Id.GetValueOrDefault();
            importForecastsFileCommandDto.UploadedFileId = fileId;
            var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                            person, DateTime.UtcNow);


            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(()=>_jobResultRepository.Add(jobResult)).IgnoreArguments();
                Expect.Call(()=>unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(()=>_busSender.NotifyServiceBus(new ImportForecastsFileToSkill())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(importForecastsFileCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowFaultExceptionIfCommandIsNull()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var targetSkill = SkillFactory.CreateSkill("Test Skills");
            targetSkill.SetId(Guid.NewGuid());
            
            var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                            person, DateTime.UtcNow);


            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _jobResultRepository.Add(jobResult)).IgnoreArguments();
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
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var targetSkill = SkillFactory.CreateSkill("Test Skills");
            targetSkill.SetId(Guid.NewGuid());

            var fileId = Guid.NewGuid();

            var importForecastsFileCommandDto = new ImportForecastsFileCommandDto();
            importForecastsFileCommandDto.ImportForecastsMode = ImportForecastsOptionsDto.ImportWorkloadAndStaffing;
            importForecastsFileCommandDto.TargetSkillId = targetSkill.Id.GetValueOrDefault();
            importForecastsFileCommandDto.UploadedFileId = fileId;
            
            var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(new DateOnly(DateTime.Now), new DateOnly(DateTime.Now)),
                                            person, DateTime.UtcNow);


            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _jobResultRepository.Add(jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _busSender.NotifyServiceBus(new ImportForecastsFileToSkill())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(importForecastsFileCommandDto);
            }
        }   
    }
}
