using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ExportMultisiteSkillToSkillCommandHandlerTest
    {
        private MockRepository _mock;
		private IEventPublisher _busSender;
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
	    private IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_busSender = _mock.StrictMock<IEventPublisher>();
            _unitOfWorkFactory = _mock.StrictMock<ICurrentUnitOfWorkFactory>();
            _jobResultRepository = _mock.StrictMock<IJobResultRepository>();
		    _eventInfrastructureInfoPopulator = _mock.StrictMock<IEventInfrastructureInfoPopulator>();
			_target = new ExportMultisiteSkillToSkillCommandHandler(_busSender,_unitOfWorkFactory,_jobResultRepository,_eventInfrastructureInfoPopulator);
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
	            Expect.Call(
		            () => _eventInfrastructureInfoPopulator.PopulateEventContext(new ExportMultisiteSkillsToSkillEvent()))
		            .IgnoreArguments();
                Expect.Call(() => _busSender.Publish(new ExportMultisiteSkillsToSkillEvent())).IgnoreArguments();
            }

            using(_mock.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					_target.Handle(_exportMultisiteSkillToSkillCommandDto);
				}
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
				Expect.Call(
				   () => _eventInfrastructureInfoPopulator.PopulateEventContext(new ExportMultisiteSkillsToSkillEvent()))
				   .IgnoreArguments();
				Expect.Call(() => _busSender.Publish(new ExportMultisiteSkillsToSkillEvent())).IgnoreArguments();
            }
            using (_mock.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					_target.Handle(_exportMultisiteSkillToSkillCommandDto);
				}
			}
        }

        [Test]
        public void ShouldThrowExceptionIfNotPermitted()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
            }
	        Assert.Throws<FaultException>(() =>
	        {
				using (_mock.Playback())
				{
					using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
					{
						_target.Handle(_exportMultisiteSkillToSkillCommandDto);
					}
				}
	        });
        }
    }
}
