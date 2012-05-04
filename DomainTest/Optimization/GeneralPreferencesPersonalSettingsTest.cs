using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GeneralPreferencesPersonalSettingsTest
	{
		private GeneralPreferencesPersonalSettings _target;
		private IList<IScheduleTag> _scheduleTags;
		private IScheduleTag _scheduleTag1;
		private Guid _scheduleTag1Id;
		private IScheduleTag _scheduleTag2;
		private Guid _scheduleTag2Id;
		private IGeneralPreferences _generalPreferences;

			[SetUp]
		public void Setup()
		{
			_scheduleTag1Id = Guid.NewGuid();
			_scheduleTag2Id = Guid.NewGuid();
			_scheduleTag1 = new ScheduleTag();
			_scheduleTag2 = new ScheduleTag();
			_scheduleTag1.SetId(_scheduleTag1Id);
			_scheduleTag2.SetId(_scheduleTag2Id);
			_scheduleTags = new List<IScheduleTag> { _scheduleTag1 };
			_target = new GeneralPreferencesPersonalSettings(_scheduleTags);
			_generalPreferences = new GeneralPreferences();
		}

		[Test]
		public void ShouldFindScheduleTagByIdIfExistInScheduleTagList()
		{
			_target.SetScheduleTagId(_scheduleTag1Id);	
			_target.MapTo(_generalPreferences);
			Assert.AreEqual(_generalPreferences.ScheduleTag.Id.Value, _scheduleTag1Id);
		}

		[Test]
		public void ShouldUseDefaultScheduleTagIfIdNotExistInScheduleTagList()
		{
			_target.SetScheduleTagId(_scheduleTag2Id);
			_target.MapTo(_generalPreferences);
			Assert.AreEqual(_generalPreferences.ScheduleTag, NullScheduleTag.Instance);
		}
	}
}
