using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
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
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ExportMultisiteSkillToSkillCommandHandlerTest
    {
        private MockRepository _mock;
		private IMessagePopulatingServiceBusSender _busSender;
        private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private IJobResultRepository _jobResultRepository;
        private ExportMultisiteSkillToSkillCommandHandler _target;
        private IPerson _person;
        private readonly DateOnlyPeriodDto _periodDto = new DateOnlyPeriodDto
                                                        {
                                                            Id = Guid.NewGuid(),
															StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 1, 1) },
															EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 1, 2) }
                                                        };

        private ExportMultisiteSkillToSkillCommandDto _exportMultisiteSkillToSkillCommandDto;
        private IJobResult _jobResult;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IMessagePopulatingServiceBusSender>();
            _unitOfWorkFactory = _mock.StrictMock<ICurrentUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
            _target = new ExportMultisiteSkillToSkillCommandHandler(_busSender,_unitOfWorkFactory,_jobResultRepository);
            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
            _activity = ActivityFactory.CreateActivity("test activity");
            _activity.SetId(Guid.NewGuid());
            var activityDto  =new ActivityDto();
            activityDto.Id = Guid.NewGuid();
            
            var multiSiteSkillSelectionDto = new MultisiteSkillSelectionDto();
            var skillDto = new SkillDto();
            skillDto.Activity = activityDto;
            skillDto.Id = Guid.NewGuid();
            skillDto.Name = "source SkillDto";

            var targetSkillDto = new SkillDto();
            targetSkillDto.Activity = activityDto;
            targetSkillDto.Id = Guid.NewGuid();
            targetSkillDto.Name = "target SkillDto";

            var childSkillMappingDto = new ChildSkillMappingDto();
            childSkillMappingDto.SourceSkill = skillDto;
            childSkillMappingDto.TargetSkill = targetSkillDto;
            var childSkillMappingDtoList = new Collection<ChildSkillMappingDto>();
            childSkillMappingDtoList.Add(childSkillMappingDto);

            multiSiteSkillSelectionDto.MultisiteSkill = skillDto;
            multiSiteSkillSelectionDto.ChildSkillMapping = childSkillMappingDtoList;

            var multiSelectionSkillDtoList = new Collection<MultisiteSkillSelectionDto>();
            multiSelectionSkillDtoList.Add(multiSiteSkillSelectionDto);
            
            _exportMultisiteSkillToSkillCommandDto = new ExportMultisiteSkillToSkillCommandDto { Period = _periodDto, MultisiteSkillSelection = multiSelectionSkillDtoList};
            _jobResult = new JobResult(JobCategory.MultisiteExport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                            _person, DateTime.UtcNow);
        }

        [Test]
        public void ShouldExportMultisiteSkillSuccessfully()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
        
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.Send(new ExportMultisiteSkillsToSkill(), true)).IgnoreArguments();
            }

            using(_mock.Playback())
            {
                _target.Handle(_exportMultisiteSkillToSkillCommandDto);
            }
        }

        [Test]
        public void ShouldThrowFaultExceptionIfServiceBusIsNotAvailable()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _jobResultRepository.Add(_jobResult)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _busSender.Send(new ExportMultisiteSkillsToSkill(), true)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.Handle(_exportMultisiteSkillToSkillCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfNotPermitted()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
            }
            using (_mock.Playback())
            {
                using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
                {
                    _target.Handle(_exportMultisiteSkillToSkillCommandDto);
                }
            }
        }
    }
}
