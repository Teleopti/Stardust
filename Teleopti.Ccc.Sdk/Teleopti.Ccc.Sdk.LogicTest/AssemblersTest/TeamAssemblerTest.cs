using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class TeamAssemblerTest
    {
        private TeamAssembler _target;
        private ITeam _teamDomain;
        private TeamDto _teamDto;
        private MockRepository _mocks;
        private ITeamRepository _teamRep;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _teamRep = _mocks.StrictMock<ITeamRepository>();
            _target = new TeamAssembler(_teamRep);

            // Create domain object
            _teamDomain = new Team
                              {
                                  Description = new Description("Team Stockholm", "TS"),
                                  Site = new Site("Europe")
                                 };
            _teamDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _teamDto = new TeamDto(_teamDomain);
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            TeamDto teamDto = _target.DomainEntityToDto(_teamDomain);

            Assert.AreEqual(_teamDomain.Id, teamDto.Id);
            Assert.AreEqual(_teamDomain.Description.Name, teamDto.Description);
            Assert.AreEqual(_teamDomain.SiteAndTeam, teamDto.SiteAndTeam);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_teamRep.Get(_teamDto.Id.Value)).Return(_teamDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                ITeam teamDomain = _target.DtoToDomainEntity(_teamDto);
                Assert.IsNotNull(teamDomain);
            }
        }
    }
}
