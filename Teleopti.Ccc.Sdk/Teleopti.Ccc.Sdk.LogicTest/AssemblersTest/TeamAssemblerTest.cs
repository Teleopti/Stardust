using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class TeamAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var teamRep = new FakeTeamRepository();
			var target = new TeamAssembler(teamRep);

			var teamDomain = new Team
			{
				Description = new Description("Team Stockholm", "TS"),
				Site = new Site("Europe")
			}.WithId();

			var teamDto = target.DomainEntityToDto(teamDomain);

            Assert.AreEqual(teamDomain.Id, teamDto.Id);
            Assert.AreEqual(teamDomain.Description.Name, teamDto.Description);
            Assert.AreEqual(teamDomain.SiteAndTeam, teamDto.SiteAndTeam);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var teamRep = new FakeTeamRepository();
		    var target = new TeamAssembler(teamRep);

		    var teamDomain = new Team
		    {
			    Description = new Description("Team Stockholm", "TS"),
			    Site = new Site("Europe")
		    }.WithId();
		    teamRep.Add(teamDomain);

		    var teamDto = new TeamDto {Description = teamDomain.Description.Name, Id = teamDomain.Id};

		    var result = target.DtoToDomainEntity(teamDto);
		    Assert.IsNotNull(result);
	    }
    }
}
