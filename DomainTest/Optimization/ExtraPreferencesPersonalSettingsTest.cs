using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ExtraPreferencesPersonalSettingsTest
	{
		private ExtraPreferencesPersonalSettings _target;
		private IList<GroupPageLight> _groupPages;
		private string _groupPage1Key;
		private string _groupPage2Key;
		private GroupPageLight _groupPage1;
		private IExtraPreferences _extraPreferencesSource;
		private IExtraPreferences _extraPreferencesTarget;
	    private IList<GroupPageLight> _groupPagesForTeamBlockPer;

	    [SetUp]
		public void Setup()
		{
			_groupPage1Key = "Key1";
			_groupPage2Key = "Key2";
			_groupPage1 = new GroupPageLight();
			
			_groupPages = new List<GroupPageLight> { _groupPage1 };

		    _groupPagesForTeamBlockPer = _groupPages;
		    var singleAgentTeamGP = GroupPageLight.SingleAgentGroup(string.Empty);

            _groupPagesForTeamBlockPer.Add(singleAgentTeamGP);
			_target = new ExtraPreferencesPersonalSettings();
			_extraPreferencesSource = new ExtraPreferences();
			_extraPreferencesTarget = new ExtraPreferences();
		}

		[Test]
		public void VerifyDefaultValues()
		{
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);
			Assert.AreEqual(BlockFinderType.SingleDay, _extraPreferencesTarget.BlockTypeValue );
			
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{
			_extraPreferencesSource.BlockTypeValue = BlockFinderType.SchedulePeriod;
			_extraPreferencesSource.UseTeams = !_extraPreferencesSource.UseTeams;

            _extraPreferencesSource.FairnessValue = 102d;

		    

			_target.MapFrom(_extraPreferencesSource);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

				Assert.AreEqual(_extraPreferencesSource.BlockTypeValue, _extraPreferencesTarget.BlockTypeValue);
			Assert.AreEqual(_extraPreferencesSource.UseTeams, _extraPreferencesTarget.UseTeams);
            
            Assert.AreEqual(_extraPreferencesSource.FairnessValue, _extraPreferencesTarget.FairnessValue);

		}

        [Test]
        public void MappingShouldGetAndSetForTeamBlockOptions()
        {
            _extraPreferencesSource.UseTeamBlockOption  = true;
			_extraPreferencesSource.UseTeamSameActivity = true;
            _extraPreferencesSource.UseBlockSameEndTime  = true;
            _extraPreferencesSource.UseBlockSameStartTime  = true;
            _extraPreferencesSource.UseBlockSameShift = true;
            _extraPreferencesSource.UseBlockSameShiftCategory  = false;
            _extraPreferencesSource.BlockTypeValue = BlockFinderType.BetweenDayOff;
            _extraPreferencesSource.TeamGroupPage  = _groupPage1;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

            Assert.AreEqual(_extraPreferencesSource.UseTeamBlockOption, _extraPreferencesTarget.UseTeamBlockOption);
			Assert.AreEqual(_extraPreferencesSource.UseTeamSameActivity, _extraPreferencesTarget.UseTeamSameActivity);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameEndTime, _extraPreferencesTarget.UseBlockSameEndTime);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameStartTime, _extraPreferencesTarget.UseBlockSameStartTime);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameShift, _extraPreferencesTarget.UseBlockSameShift);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameShiftCategory, _extraPreferencesTarget.UseBlockSameShiftCategory);
				Assert.AreEqual(_extraPreferencesSource.TeamGroupPage, _extraPreferencesTarget.TeamGroupPage);
            Assert.AreEqual(_extraPreferencesSource.BlockTypeValue, _extraPreferencesTarget.BlockTypeValue);
        }

        [Test]
        public void MappingShouldGetAndSetForTeamBlockOptionsForSingleAgentPage()
        {
            var singleAgentTeamGroupPage = GroupPageLight.SingleAgentGroup(string.Empty);
				_extraPreferencesSource.TeamGroupPage = singleAgentTeamGroupPage;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

				Assert.AreEqual(_extraPreferencesSource.TeamGroupPage.Key, _extraPreferencesTarget.TeamGroupPage.Key);
            
        }

	}
}
