using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
    [TestFixture]
    public class GetAllScheduleTagsQueryHandlerTest
    {
        private MockRepository mocks;
        private IScheduleTagRepository scheduleTagRepository;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private GetAllScheduleTagsQueryHandler target;
        private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
        private IAssembler<IScheduleTag, ScheduleTagDto> assembler;
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            scheduleTagRepository = mocks.DynamicMock<IScheduleTagRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
            assembler = mocks.DynamicMock<IAssembler<IScheduleTag, ScheduleTagDto>>();
            target = new GetAllScheduleTagsQueryHandler(assembler, scheduleTagRepository, currentUnitOfWorkFactory);
        }

        [Test]
        public void ShouldGetAllScenariosAvailable()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var scheduleTagList = new List<IScheduleTag>();
            var scheduleTag = new ScheduleTag() {Description = "test"};
            scheduleTagList.Add(scheduleTag);

            var scheduleTagDtoList = new List<ScheduleTagDto>();
            var scheduleTagDto = new ScheduleTagDto() {Id = Guid.NewGuid(), Description = "test"};
            scheduleTagDtoList.Add(scheduleTagDto);

            using (mocks.Record())
            {
                Expect.Call(scheduleTagRepository.FindAllScheduleTags()).Return(scheduleTagList);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(assembler.DomainEntitiesToDtos(scheduleTagList)).IgnoreArguments().Return(scheduleTagDtoList);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
            }
            using (mocks.Playback())
            {
                var result = target.Handle(new GetAllScheduleTagsDto());
                result.First().Description.Should().Be.EqualTo("test");
            }
        }
    }
}
