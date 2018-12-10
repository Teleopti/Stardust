using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[DatabaseTest]
	[ExtendScope(typeof(PersonAssociationChangedEventPublisher))]
	[ExtendScope(typeof(AgentStateReadModelMaintainer))]
	[ExtendScope(typeof(UpdateGroupingReadModelHandler))]
	public class OrganizationReaderSkillsTest
	{
		public IOrganizationReader Target;
		public Database Database;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldReadTeamWithSkill()
		{
			Database
				.WithSite()
				.WithTeam()
				.WithAgent()
				.WithTeam()
				.WithAgent()
				.WithSkill("phone")
				;
			var team2 = Database.CurrentTeamId();
			var phone = Database.SkillIdFor("phone");
	
			var result = UnitOfWork.Get(() => Target.Read(phone.AsArray()));

			result.Single().Teams.Single().TeamId.Should().Be(team2);
		}

	}
	

}