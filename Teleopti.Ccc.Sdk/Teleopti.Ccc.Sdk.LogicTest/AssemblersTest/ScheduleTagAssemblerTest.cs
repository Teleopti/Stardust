using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ScheduleTagAssemblerTest
    {

        private ScheduleTagAssembler _target;
        private MockRepository _mocks;
        private IScheduleTagRepository _scheduleTagRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleTagRepository = _mocks.StrictMock<IScheduleTagRepository>();
            _target = new ScheduleTagAssembler(_scheduleTagRepository);
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            var scheduleTag = new ScheduleTag() {Description = "test"};

            ScheduleTagDto scheduleTagDto = _target.DomainEntityToDto(scheduleTag);
            Assert.AreEqual(scheduleTagDto.Description , "test");
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            var scheduleTag = new ScheduleTag() {Description = "test"};
            var scheduleTagDto = new ScheduleTagDto() { Id = Guid.NewGuid(), Description = "test" };

            using (_mocks.Record())
            {
                Expect.Call(_scheduleTagRepository.Get(Guid.NewGuid())).IgnoreArguments().Return(scheduleTag);
            }
            using (_mocks.Playback())
            {
                IScheduleTag result = _target.DtoToDomainEntity(scheduleTagDto);
                Assert.AreEqual(result.Description, "test");
            }
            
            Assert.AreEqual(scheduleTag.Description , "test");
        }

    }
}
