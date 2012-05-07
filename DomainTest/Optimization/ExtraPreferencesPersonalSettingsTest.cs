using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ExtraPreferencesPersonalSettingsTest
	{
		private ExtraPreferencesPersonalSettings _target;
		private IList<IGroupPage> _groupPages;
		private Guid _groupPage1Id;
		private Guid _groupPage2Id;
		private IGroupPage _groupPage1;
		private IGroupPage _groupPage2;
		private IExtraPreferences _extraPreferencesSource;
		private IExtraPreferences _extraPreferencesTarget;

		[SetUp]
		public void Setup()
		{
			_groupPage1 = new GroupPage("1");
			_groupPage2 = new GroupPage("1");
			_groupPage1Id = Guid.NewGuid();
			_groupPage2Id = Guid.NewGuid();
			_groupPage1.SetId(_groupPage1Id);
			_groupPage2.SetId(_groupPage2Id);

			_groupPages = new List<IGroupPage>{ _groupPage1 };
			_target = new ExtraPreferencesPersonalSettings(_groupPages);
			_extraPreferencesSource = new ExtraPreferences();
			_extraPreferencesTarget = new ExtraPreferences();
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{
			_extraPreferencesSource.BlockFinderTypeValue = BlockFinderType.SchedulePeriod;
			_extraPreferencesSource.UseBlockScheduling = !_extraPreferencesSource.UseBlockScheduling;
			_extraPreferencesSource.UseTeams = !_extraPreferencesSource.UseTeams;

			_extraPreferencesSource.KeepShiftCategories = !_extraPreferencesSource.KeepShiftCategories;
			_extraPreferencesSource.KeepStartAndEndTimes = !_extraPreferencesSource.KeepStartAndEndTimes;
			_extraPreferencesSource.KeepShifts = !_extraPreferencesSource.KeepShifts;

			_extraPreferencesSource.KeepShiftsValue = 101d;
			_extraPreferencesSource.FairnessValue = 102d;

			_target.MapFrom(_extraPreferencesSource);
			_target.MapTo(_extraPreferencesTarget);

			Assert.AreEqual(_extraPreferencesSource.BlockFinderTypeValue, _extraPreferencesTarget.BlockFinderTypeValue);
			Assert.AreEqual(_extraPreferencesSource.UseBlockScheduling, _extraPreferencesTarget.UseBlockScheduling);
			Assert.AreEqual(_extraPreferencesSource.UseTeams, _extraPreferencesTarget.UseTeams);

			Assert.AreEqual(_extraPreferencesSource.KeepShiftCategories, _extraPreferencesTarget.KeepShiftCategories);
			Assert.AreEqual(_extraPreferencesSource.KeepStartAndEndTimes, _extraPreferencesTarget.KeepStartAndEndTimes);
			Assert.AreEqual(_extraPreferencesSource.KeepShifts, _extraPreferencesTarget.KeepShifts);

			Assert.AreEqual(_extraPreferencesSource.KeepShiftsValue, _extraPreferencesTarget.KeepShiftsValue);
			Assert.AreEqual(_extraPreferencesSource.FairnessValue, _extraPreferencesTarget.FairnessValue);
		}

		[Test]
		public void ShouldFindGroupByIdIfExistInGroupList()
		{
			_target.SetGroupPageOnTeamId(_groupPage1Id);
			_target.SetGroupPageOnCompareWithId(_groupPage1Id);
			_target.MapTo(_extraPreferencesTarget);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnTeam.Id.Value, _groupPage1Id);
			Assert.AreEqual(_extraPreferencesTarget.GroupPageOnCompareWith.Id.Value, _groupPage1Id);
		}

		[Test]
		public void ShouldUseDefaultScheduleTagIfIdNotExistInScheduleTagList()
		{
			_target.SetGroupPageOnTeamId(_groupPage2Id);
			_target.SetGroupPageOnCompareWithId(_groupPage2Id);
			_target.MapTo(_extraPreferencesTarget);
			Assert.IsNull(_extraPreferencesTarget.GroupPageOnTeam);
			Assert.IsNull(_extraPreferencesTarget.GroupPageOnCompareWith);
		}
	}
}
