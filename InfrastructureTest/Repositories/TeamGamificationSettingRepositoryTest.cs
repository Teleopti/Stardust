﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
	/// Tests TeamGamificationSettingRepository
	///</summary>
	[TestFixture]
	[Category("LongRunning")]
	public class TeamGamificationSettingRepositoryTest : RepositoryTest<ITeamGamificationSetting>
	{
		private ITeamGamificationSetting _teamGamificationSetting;
		private IGamificationSetting _gamificationSetting;
		private ISite _site;
		private ITeam _team;


		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override ITeamGamificationSetting CreateAggregateWithCorrectBusinessUnit()
		{
			ITeamGamificationSetting teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			return teamGamificationSetting;
		}

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
		
		[Test]
		public void VerifyCanPersistProperties()
		{
			_teamGamificationSetting = new TeamGamificationSetting { Team = _team, GamificationSetting = _gamificationSetting };
			
			PersistAndRemoveFromUnitOfWork(_teamGamificationSetting);
			IList<ITeamGamificationSetting> loadedTeamGamificationSettings = new TeamGamificationSettingRepository(UnitOfWork).LoadAll();
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
				new TeamGamificationSettingRepository(UnitOfWork).FindAllTeamGamificationSettingsSortedByTeam();
			Assert.AreEqual(1, loadedGamificationSettings.Count());
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(ITeamGamificationSetting loadedAggregateFromDatabase)
		{
			ITeamGamificationSetting org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Team, loadedAggregateFromDatabase.Team);
		}

		protected override Repository<ITeamGamificationSetting> TestRepository(IUnitOfWork unitOfWork)
		{
			return new TeamGamificationSettingRepository(unitOfWork);
		}
	}
}
