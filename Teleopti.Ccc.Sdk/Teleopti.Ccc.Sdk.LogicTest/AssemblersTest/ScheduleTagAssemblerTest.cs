using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ScheduleTagAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var scheduleTagRepository = new FakeScheduleTagRepository();
			var target = new ScheduleTagAssembler(scheduleTagRepository);
			var scheduleTag = new ScheduleTag {Description = "test"}.WithId();

            ScheduleTagDto scheduleTagDto = target.DomainEntityToDto(scheduleTag);
            Assert.AreEqual(scheduleTagDto.Description , "test");
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var scheduleTagRepository = new FakeScheduleTagRepository();
		    var target = new ScheduleTagAssembler(scheduleTagRepository);
		    var scheduleTag = new ScheduleTag {Description = "test"}.WithId();
		    scheduleTagRepository.Add(scheduleTag);

		    var scheduleTagDto = new ScheduleTagDto {Id = scheduleTag.Id, Description = "test"};

		    var result = target.DtoToDomainEntity(scheduleTagDto);
		    Assert.AreEqual(result.Description, "test");
	    }

	    [Test]
		public void ShouldReturnNullInstanceOnNullDto()
		{
			var scheduleTagRepository = new FakeScheduleTagRepository();
			var target = new ScheduleTagAssembler(scheduleTagRepository);
			var result = target.DtoToDomainEntity(null);
			Assert.That(result, Is.EqualTo(KeepOriginalScheduleTag.Instance));
		}
    }
}
