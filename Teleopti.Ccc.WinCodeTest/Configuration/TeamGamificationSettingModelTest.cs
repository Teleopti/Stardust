﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class TeamGamificationSettingModelTest
    {
        
		private TeamGamificationSettingModel _target;

        [SetUp]
        public void Setup()
        {
			_target = new TeamGamificationSettingModel(new TeamGamificationSetting());
        }

        [Test]
        public void VerifyGamificationSetting()
        {
            var setting = new GamificationSetting("setting");
            _target.GamificationSetting = setting;
            Assert.AreEqual(setting, _target.GamificationSetting);
        } 
		
		[Test]
        public void VerifyEmptyGamificationSettingInsteadOfNull()
        {
			Assert.AreEqual(GamificationSettingProvider.NullGamificationSetting,_target.GamificationSetting);
        }

		[Test]
        public void VerifySettingStatusEmpty()
		{
			_target.GamificationSetting = GamificationSettingProvider.NullGamificationSetting;
			Assert.AreEqual(" ",_target.SettingStatus);
        }
		
        [Test]
        public void VerifyCanGetSiteAndTeam()
        {
	        var team = TeamFactory.CreateSimpleTeam("team");
	        _target.Team = team;
            team.Site = SiteFactory.CreateSimpleSite("Site");
            Assert.AreEqual(team.SiteAndTeam,_target.SiteAndTeam);
        }
		
	}
}
