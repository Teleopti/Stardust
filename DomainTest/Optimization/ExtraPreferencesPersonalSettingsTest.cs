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
	    private IList<IGroupPageLight> _groupPagesForTeamBlockPer;

	    [SetUp]
		public void Setup()
		{
			_groupPage1Key = "Key1";
			_groupPage2Key = "Key2";
			_groupPage1 = new GroupPageLight{Key =_groupPage1Key};
			
			_groupPages = new List<IGroupPageLight> { _groupPage1 };

		    _groupPagesForTeamBlockPer = _groupPages;
	        var singleAgentTeamGP = new GroupPageLight();
            singleAgentTeamGP.Key = "SingleAgentTeam";
            singleAgentTeamGP.Name = "Single Agent Team";
            _groupPagesForTeamBlockPer.Add(singleAgentTeamGP);
			_target = new ExtraPreferencesPersonalSettings();
			_extraPreferencesSource = new ExtraPreferences();
			_extraPreferencesTarget = new ExtraPreferences();
		}

		[Test]
		public void VerifyDefaultValues()
		{
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);
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
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

			Assert.AreEqual(_extraPreferencesSource.BlockFinderTypeValue, _extraPreferencesTarget.BlockFinderTypeValue);
			//Assert.AreEqual(_extraPreferencesSource.UseBlockScheduling, _extraPreferencesTarget.UseBlockScheduling);
			Assert.AreEqual(_extraPreferencesSource.UseTeams, _extraPreferencesTarget.UseTeams);
            
            Assert.AreEqual(_extraPreferencesSource.FairnessValue, _extraPreferencesTarget.FairnessValue);

            

		}

        [Test]
        public void MappingShouldGetAndSetForTeamBlockOptions()
        {
            _extraPreferencesSource.UseTeamBlockOption  = true;
            _extraPreferencesSource.UseBlockSameEndTime  = true;
            _extraPreferencesSource.UseBlockSameStartTime  = true;
            _extraPreferencesSource.UseBlockSameShift = true;
            _extraPreferencesSource.UseBlockSameShiftCategory  = false;
            _extraPreferencesSource.BlockTypeValue = BlockFinderType.BetweenDayOff;
            _extraPreferencesSource.GroupPageOnTeamBlockPer = _groupPage1;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

            Assert.AreEqual(_extraPreferencesSource.UseTeamBlockOption, _extraPreferencesTarget.UseTeamBlockOption);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameEndTime, _extraPreferencesTarget.UseBlockSameEndTime);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameStartTime, _extraPreferencesTarget.UseBlockSameStartTime);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameShift, _extraPreferencesTarget.UseBlockSameShift);
            Assert.AreEqual(_extraPreferencesSource.UseBlockSameShiftCategory, _extraPreferencesTarget.UseBlockSameShiftCategory);
            Assert.AreEqual(_extraPreferencesSource.GroupPageOnTeamBlockPer , _extraPreferencesTarget.GroupPageOnTeamBlockPer);
            Assert.AreEqual(_extraPreferencesSource.BlockTypeValue, _extraPreferencesTarget.BlockTypeValue);
        }

        [Test]
        public void MappingShouldGetAndSetForTeamBlockOptionsForSingleAgentPage()
        {
            var singleAgentTeamGroupPage = new GroupPageLight { Key = "SingleAgentTeam" ,Name = "Single Agent Team" };
            _extraPreferencesSource.GroupPageOnTeamBlockPer = singleAgentTeamGroupPage;
            _target.MapFrom(_extraPreferencesSource);

            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);

            Assert.AreEqual(_extraPreferencesSource.GroupPageOnTeamBlockPer.Key , _extraPreferencesTarget.GroupPageOnTeamBlockPer.Key );
            
        }

		[Test]
		public void ShouldFindGroupByIdIfExistInGroupList()
		{
			_target.SetGroupPageOnTeamKey(_groupPage1Key);
			_target.SetGroupPageOnCompareWithKey(_groupPage1Key);
            _target.SetGroupPageOnTeamBlockPerKey( _groupPage1Key);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);
			Assert.AreEqual(_extraPreferencesTarget.TeamGroupPage.Key, _groupPage1Key);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnTeamBlockPer.Key, _groupPage1Key);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnCompareWith.Key, _groupPage1Key);
		}

		[Test]
		public void ShouldUseDefaultScheduleTagIfIdNotExistInScheduleTagList()
		{
			_target.SetGroupPageOnTeamKey(_groupPage2Key);
			_target.SetGroupPageOnCompareWithKey(_groupPage2Key);
            _target.SetGroupPageOnTeamBlockPerKey(_groupPage2Key);
            _target.MapTo(_extraPreferencesTarget, _groupPages, _groupPagesForTeamBlockPer);
			Assert.IsNull(_extraPreferencesTarget.TeamGroupPage);
            Assert.IsNull(_extraPreferencesTarget.GroupPageOnTeamBlockPer);
			Assert.IsNull(_extraPreferencesTarget.GroupPageOnCompareWith);
		}
	}
}
