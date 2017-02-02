using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	//This interface is not needed but circle ref otherwise. Need to get rid of interface assembly first!
	public interface IResourceCalculationData
	{
		IScheduleDictionary Schedules { get; }
		bool ConsiderShortBreaks { get; }
		bool DoIntraIntervalCalculation { get; }
		IEnumerable<ISkill> Skills { get; }
		ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
		IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		bool SkipResourceCalculation { get; }
		SkillCombinationHolder SkillCombinationHolder {get;}
		ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }
	}

	public class SkillCombinationResource
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resource { get; set; }
		public IEnumerable<Guid> SkillCombination { get; set; }

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}
	}

	public class SkillCombinationHolder
	{
		private readonly List<SkillCombinationResource> _skillCombinationResources = new List<SkillCombinationResource>();
		private bool _withShrinkage;

		public void Add(SkillCombinationResource skillCombinationResource)
		{
			if (!_withShrinkage)
			{
				_skillCombinationResources.Add(skillCombinationResource);
			}
		}

		public void StartRecodingValuesWithShrinkage()
		{
			_withShrinkage = true;
		}

		public IEnumerable<SkillCombinationResource> SkillCombinationResources => _skillCombinationResources;
	}
}