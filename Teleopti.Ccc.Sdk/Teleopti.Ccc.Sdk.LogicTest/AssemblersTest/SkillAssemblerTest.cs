using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class SkillAssemblerTest
    {
        private SkillAssembler _target;
        private ISkill _skillDomain;
        private SkillDto _skillDto;
        private MockRepository _mocks;
        private ISkillRepository _skillRep;
        private IAssembler<IActivity, ActivityDto> _activityAssembler;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _skillRep = _mocks.StrictMock<ISkillRepository>();
            _activityAssembler = _mocks.StrictMock<IAssembler<IActivity, ActivityDto>>();
            _target = new SkillAssembler(_skillRep,_activityAssembler);

            // Create domain object
            _skillDomain = SkillFactory.CreateSkill("test");
            _skillDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _skillDto = new SkillDto {Id = _skillDomain.Id};
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            using(_mocks.Record())
            {
                Expect.Call(_activityAssembler.DomainEntityToDto(_skillDomain.Activity)).Return(new ActivityDto());
            }
            using (_mocks.Playback())
            {
                SkillDto skillDto = _target.DomainEntityToDto(_skillDomain);

                Assert.AreEqual(_skillDomain.Id, skillDto.Id);
                Assert.AreEqual(_skillDomain.Name, skillDto.Name);
                Assert.AreEqual(_skillDomain.Description, skillDto.Description);
                Assert.AreEqual(_skillDomain.DefaultResolution, skillDto.Resolution);
                Assert.AreEqual(_skillDomain.SkillType.Description.Name, skillDto.SkillType);
                Assert.AreEqual(_skillDomain.DisplayColor.ToArgb(), skillDto.DisplayColor.ToColor().ToArgb());
                Assert.IsNotNull(skillDto.Activity);
            }
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_skillRep.Get(_skillDto.Id.Value)).Return(_skillDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                ISkill skillDomain = _target.DtoToDomainEntity(_skillDto);
                Assert.IsNotNull(skillDomain);
            }
        }
    }
}
