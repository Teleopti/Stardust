using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class TeamProviderTest
    {
	    [Test]
	    public void VerifyCanGetAllTeams()
	    {
		    var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
		    var target = new TeamProvider(teamRepository);
		    var result = new List<ITeam> {TeamFactory.CreateSimpleTeam()};

		    teamRepository.Stub(x => x.LoadAll()).Return(result);

		    Assert.AreEqual(result, target.GetTeams());
	    }

	    [Test]
        public void VerifyUpdate()
		{
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			var target = new TeamProvider(teamRepository);
            var oldTeam = TeamFactory.CreateSimpleTeam();
			oldTeam.SetId(Guid.NewGuid());
            var updatedTeam = TeamFactory.CreateSimpleTeam();
            updatedTeam.SetId(oldTeam.Id.GetValueOrDefault());
            
                teamRepository.Stub(x => x.LoadAll()).Return(new List<ITeam> { oldTeam });
                teamRepository.Stub(x => x.Get(updatedTeam.Id.GetValueOrDefault())).Return(updatedTeam);
            
                target.HandleMessageBrokerEvent(updatedTeam.Id.GetValueOrDefault(), DomainUpdateType.Update);
                Assert.AreEqual(1, target.GetTeams().Count());
                Assert.IsTrue(target.GetTeams().Contains(updatedTeam));
        }

	    [Test]
	    public void VerifyDelete()
	    {
		    var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
		    var target = new TeamProvider(teamRepository);
		    var oldTeam = TeamFactory.CreateSimpleTeam();
		    oldTeam.SetId(Guid.NewGuid());

		    teamRepository.Stub(x => x.LoadAll()).Return(new List<ITeam> {oldTeam});

		    target.HandleMessageBrokerEvent(oldTeam.Id.GetValueOrDefault(), DomainUpdateType.Delete);
		    Assert.AreEqual(0, target.GetTeams().Count());
		    Assert.IsFalse(target.GetTeams().Contains(oldTeam));
	    }

	    [Test]
	    public void VerifyInsert()
	    {
		    var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
		    var target = new TeamProvider(teamRepository);
		    var newTeam = TeamFactory.CreateSimpleTeam();
		    newTeam.SetId(Guid.NewGuid());

		    teamRepository.Stub(x => x.LoadAll()).Return(new List<ITeam>());
		    teamRepository.Stub(x => x.Get(newTeam.Id.GetValueOrDefault())).Return(newTeam);

		    target.HandleMessageBrokerEvent(newTeam.Id.GetValueOrDefault(), DomainUpdateType.Insert);
		    Assert.AreEqual(1, target.GetTeams().Count());
		    Assert.IsTrue(target.GetTeams().Contains(newTeam));
	    }
    }
}
