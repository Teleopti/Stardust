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
    public class ExportMultisiteSkillToSkillCommandHandlerTest
    {
        private MockRepository _mock;
        private IServiceBusSender _busSender;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IJobResultRepository _jobResultRepository;
        private ExportMultisiteSkillToSkillCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _busSender = _mock.StrictMock<IServiceBusSender>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
            _target = new ExportMultisiteSkillToSkillCommandHandler(_busSender,_unitOfWorkFactory,_jobResultRepository);
        }

        [Test]
        public void ShouldExportMultisiteSkillSuccessfully()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var periodDto = new DateOnlyPeriodDto
                                {
                                    Id = Guid.NewGuid(),
                                    StartDate =
                                        new DateOnlyDto(new DateOnly(new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
                                    EndDate =
                                        new DateOnlyDto(new DateOnly(new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)))
                                };


            var exportMultisiteSkillToSkillCommandDto = new ExportMultisiteSkillToSkillCommandDto {Period = periodDto};

            var jobResult = new JobResult(JobCategory.MultisiteExport, new DateOnlyPeriod(new DateOnly(DateTime.Now), new DateOnly(DateTime.Now)),
                                            person, DateTime.UtcNow);


            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_busSender.EnsureBus()).Return(true);
                Expect.Call(() => _jobResultRepository.Add(jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.NotifyServiceBus(new ExportMultisiteSkillsToSkill())).IgnoreArguments();
            }

            using(_mock.Playback())
            {
                _target.Handle(exportMultisiteSkillToSkillCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var periodDto = new DateOnlyPeriodDto
                                {
                                    Id = Guid.NewGuid(),
                                    StartDate =
                                        new DateOnlyDto(new DateOnly(new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
                                    EndDate =
                                        new DateOnlyDto(new DateOnly(new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)))
                                };

            var exportMultisiteSkillToSkillCommandDto = new ExportMultisiteSkillToSkillCommandDto {Period = periodDto};
            var jobResult = new JobResult(JobCategory.MultisiteExport, new DateOnlyPeriod(new DateOnly(DateTime.Now), new DateOnly(DateTime.Now)),
                                            person, DateTime.UtcNow);

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_busSender.EnsureBus()).Return(false);
                Expect.Call(() => _jobResultRepository.Add(jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.NotifyServiceBus(new ExportMultisiteSkillsToSkill())).IgnoreArguments();
            }

            using (_mock.Playback())
            {
                _target.Handle(exportMultisiteSkillToSkillCommandDto);
            }
        }


    }
}
