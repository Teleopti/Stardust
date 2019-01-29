using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
	/// Tests TeamGamificationSettingRepository
	///</summary>
	[TestFixture]
	[Category("BucketB")]
	public class TeamGamificationSettingRepositoryTest : RepositoryTest<ITeamGamificationSetting>
	{
		private ITeamGamificationSetting _teamGamificationSetting;
		private IGamificationSetting _gamificationSetting;
		private ISite _site;
		private ITeam _team;


		protected override void ConcreteSetup()
		{
			_site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(_site);

			_team = TeamFactory.CreateSimpleTeam("team");
			_site.AddTeam(_team);

			PersistAndRemoveFromUnitOfWork(_team);

			_gamificationSetting = new GamificationSetting("gamificationsetting");
			PersistAndRemoveFromUnitOfWork(_gamificationSetting);
		}
		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override ITeamGamificationSetting CreateAggregateWithCorrectBusinessUnit()
		{
			var team = TeamFactory.CreateSimpleTeam("team");
			_site.AddTeam(team);
			PersistAndRemoveFromUnitOfWork(team);

			ITeamGamificationSetting teamGamificationSetting = new TeamGamificationSetting { Team = team, GamificationSetting = _gamificationSetting };
			return teamGamificationSetting;
		}
		
		[Test]
		public void VerifyCanPersistProperties()
		{
			_teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			
			PersistAndRemoveFromUnitOfWork(_teamGamificationSetting);
			IList<ITeamGamificationSetting> loadedTeamGamificationSettings = new TeamGamificationSettingRepository(CurrUnitOfWork).LoadAll().ToList();
			Assert.AreEqual(1, loadedTeamGamificationSettings.Count);
			Assert.AreEqual("team", loadedTeamGamificationSettings[0].Team.Description.Name);
			Assert.AreEqual("gamificationsetting", loadedTeamGamificationSettings[0].GamificationSetting.Description.Name);
		}
		
		[Test]
		public void VerifyCanLoadTeamGamificationSettings()
		{
			_teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			PersistAndRemoveFromUnitOfWork(_teamGamificationSetting);
			IEnumerable<ITeamGamificationSetting> loadedGamificationSettings =
				new TeamGamificationSettingRepository(CurrUnitOfWork).FindAllTeamGamificationSettingsSortedByTeam();
			Assert.AreEqual(1, loadedGamificationSettings.Count());
		}
		[Test]
		public void VerifyCanLoadTeamGamificationSettingByTeam()
		{
			_teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			PersistAndRemoveFromUnitOfWork(_teamGamificationSetting);
			ITeamGamificationSetting loadedMyGamificationSetting =
				new TeamGamificationSettingRepository(CurrUnitOfWork).FindTeamGamificationSettingsByTeam(_team);
			Assert.AreEqual("team" , loadedMyGamificationSetting.Team.Description.Name);
		}

		[Test]
		public void ShouldFetchTeamGamificationSettings()
		{
			_teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			PersistAndRemoveFromUnitOfWork(_teamGamificationSetting);

			var loadedMyGamificationSettings =
				new TeamGamificationSettingRepository(CurrUnitOfWork).FetchTeamGamificationSettings(_gamificationSetting.Id.GetValueOrDefault());
			loadedMyGamificationSettings.First().Id.Should().Be.EqualTo(_teamGamificationSetting.Id);
		}
		protected override void VerifyAggregateGraphProperties(ITeamGamificationSetting loadedAggregateFromDatabase)
		{
			ITeamGamificationSetting org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.GamificationSetting, loadedAggregateFromDatabase.GamificationSetting);
		}

		protected override Repository<ITeamGamificationSetting> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new TeamGamificationSettingRepository(currentUnitOfWork);
		}
	}
}
