﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ExtraPreferencesPersonalSettingsTest
	{
		private ExtraPreferencesPersonalSettings _target;
		private IList<IGroupPageLight> _groupPages;
		private string _groupPage1Key;
		private string _groupPage2Key;
		private IGroupPageLight _groupPage1;
		//private IGroupPage _groupPage2;
		private IExtraPreferences _extraPreferencesSource;
		private IExtraPreferences _extraPreferencesTarget;
	    private IList<IGroupPageLight> _groupPagesForLevellingPer;

	    [SetUp]
		public void Setup()
		{
			_groupPage1Key = "Key1";
			_groupPage2Key = "Key2";
			_groupPage1 = new GroupPageLight{Key =_groupPage1Key};
			
			_groupPages = new List<IGroupPageLight> { _groupPage1 };

		    _groupPagesForLevellingPer = _groupPages;
	        var singleAgentTeamGP = new GroupPageLight();
            singleAgentTeamGP.Key = "SingleAgentTeam";
            singleAgentTeamGP.Name = "Single Agent Team";
            _groupPagesForLevellingPer.Add(singleAgentTeamGP);
			_target = new ExtraPreferencesPersonalSettings();
			_extraPreferencesSource = new ExtraPreferences();
			_extraPreferencesTarget = new ExtraPreferences();
		}

		[Test]
		public void VerifyDefaultValues()
		{
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);
			Assert.AreEqual(BlockFinderType.BetweenDayOff, _extraPreferencesTarget.BlockFinderTypeValue);
			
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{
			_extraPreferencesSource.BlockFinderTypeValue = BlockFinderType.SchedulePeriod;
			//_extraPreferencesSource.UseBlockScheduling = !_extraPreferencesSource.UseBlockScheduling;
			_extraPreferencesSource.UseTeams = !_extraPreferencesSource.UseTeams;

            _extraPreferencesSource.FairnessValue = 102d;

		    

			_target.MapFrom(_extraPreferencesSource);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);

			Assert.AreEqual(_extraPreferencesSource.BlockFinderTypeValue, _extraPreferencesTarget.BlockFinderTypeValue);
			//Assert.AreEqual(_extraPreferencesSource.UseBlockScheduling, _extraPreferencesTarget.UseBlockScheduling);
			Assert.AreEqual(_extraPreferencesSource.UseTeams, _extraPreferencesTarget.UseTeams);
            
            Assert.AreEqual(_extraPreferencesSource.FairnessValue, _extraPreferencesTarget.FairnessValue);

            

		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Levelling"), Test]
        public void MappingShouldGetAndSetForLevellingOptions()
        {
            _extraPreferencesSource.UseLevellingOption  = true;
            _extraPreferencesSource.UseLevellingSameEndTime  = true;
            _extraPreferencesSource.UseLevellingSameStartTime  = true;
            _extraPreferencesSource.UseLevellingSameShift = true;
            _extraPreferencesSource.UseLevellingSameShiftCategory  = false;
            _extraPreferencesSource.BlockFinderTypeForAdvanceOptimization = BlockFinderType.BetweenDayOff;
            _extraPreferencesSource.GroupPageOnTeamLevelingPer = _groupPage1;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);

            Assert.AreEqual(_extraPreferencesSource.UseLevellingOption, _extraPreferencesTarget.UseLevellingOption);
            Assert.AreEqual(_extraPreferencesSource.UseLevellingSameEndTime, _extraPreferencesTarget.UseLevellingSameEndTime);
            Assert.AreEqual(_extraPreferencesSource.UseLevellingSameStartTime, _extraPreferencesTarget.UseLevellingSameStartTime);
            Assert.AreEqual(_extraPreferencesSource.UseLevellingSameShift, _extraPreferencesTarget.UseLevellingSameShift);
            Assert.AreEqual(_extraPreferencesSource.UseLevellingSameShiftCategory, _extraPreferencesTarget.UseLevellingSameShiftCategory);
            Assert.AreEqual(_extraPreferencesSource.GroupPageOnTeamLevelingPer , _extraPreferencesTarget.GroupPageOnTeamLevelingPer);
            Assert.AreEqual(_extraPreferencesSource.BlockFinderTypeForAdvanceOptimization, _extraPreferencesTarget.BlockFinderTypeForAdvanceOptimization);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Levelling"), Test]
        public void MappingShouldGetAndSetForLevellingOptionsForSingleAgentPage()
        {
            var singleAgentTeamGroupPage = new GroupPageLight { Key = "SingleAgentTeam" ,Name = "Single Agent Team" };
            _extraPreferencesSource.GroupPageOnTeamLevelingPer = singleAgentTeamGroupPage;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);

            Assert.AreEqual(_extraPreferencesSource.GroupPageOnTeamLevelingPer.Key , _extraPreferencesTarget.GroupPageOnTeamLevelingPer.Key );
            
        }

		[Test]
		public void ShouldFindGroupByIdIfExistInGroupList()
		{
			_target.SetGroupPageOnTeamKey(_groupPage1Key);
			_target.SetGroupPageOnCompareWithKey(_groupPage1Key);
            _target.SetGroupPageOnTeamLevellingPerKey( _groupPage1Key);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnTeam.Key, _groupPage1Key);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnTeamLevelingPer.Key, _groupPage1Key);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnCompareWith.Key, _groupPage1Key);
		}

		[Test]
		public void ShouldUseDefaultScheduleTagIfIdNotExistInScheduleTagList()
		{
			_target.SetGroupPageOnTeamKey(_groupPage2Key);
			_target.SetGroupPageOnCompareWithKey(_groupPage2Key);
            _target.SetGroupPageOnTeamLevellingPerKey(_groupPage2Key);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForLevellingPer);
			Assert.IsNull(_extraPreferencesTarget.GroupPageOnTeam);
            Assert.IsNull(_extraPreferencesTarget.GroupPageOnTeamLevelingPer);
			Assert.IsNull(_extraPreferencesTarget.GroupPageOnCompareWith);
		}
	}
}
