using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	//This interface is not needed but circle ref otherwise. Need to get rid of interface assembly first!
	public interface IResourceCalculationData
	{
		IScheduleDictionary Schedules { get; }
		bool ConsiderShortBreaks { get; }
		bool DoIntraIntervalCalculation { get; }
		IEnumerable<ISkill> Skills { get; }
		IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		bool SkipResourceCalculation { get; }
		SkillCombinationHolder SkillCombinationHolder {get;}
		ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }
		object ShovelingCallback { get; }
		void SetShovelingCallback(object shovelingCallback);
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

		public DateTimePeriod Period()
		{
			return new DateTimePeriod(StartDateTime, EndDateTime);
		}
	}

	public class SkillCombinationHolder
	{
		private List<SkillCombinationResource> _skillCombinationResources = new List<SkillCombinationResource>();

		public void Add(SkillCombinationResource skillCombinationResource)
		{
			_skillCombinationResources.Add(skillCombinationResource);
		}

		public void StartRecodingValuesWithShrinkage()
		{
			_skillCombinationResources = new List<SkillCombinationResource>();
		}

		public IEnumerable<SkillCombinationResource> SkillCombinationResources => _skillCombinationResources;
	}
}