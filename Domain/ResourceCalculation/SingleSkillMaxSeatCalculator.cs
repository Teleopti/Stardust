using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillMaxSeatCalculator
	{
		void Calculate(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd);	
	}

	public class SingleSkillMaxSeatCalculator : ISingleSkillMaxSeatCalculator
	{
		private readonly IPersonSkillProvider _personSkillProvider;

		public SingleSkillMaxSeatCalculator(IPersonSkillProvider personSkillProvider)
		{
			_personSkillProvider = personSkillProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Calculate(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			if (toRemove.Count == 0 && toAdd.Count == 0)
				return;

			foreach (var pair in relevantSkillStaffPeriods)
			{
				var skill = pair.Key;
				var skillStaffPeriodDictionary = pair.Value;

				foreach (var skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					var resultToRemove = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toRemove, skill);
					var resultToAdd = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toAdd, skill);
					var result = skillStaffPeriod.Payload.CalculatedLoggedOn - resultToRemove + resultToAdd;
					
					if (!skillStaffPeriod.Payload.CalculatedLoggedOn.Equals(result))
					{
						skillStaffPeriod.Payload.CalculatedLoggedOn = result;
						skillStaffPeriod.SetCalculatedResource65(result);
						skillStaffPeriod.Payload.CalculatedUsedSeats = result;
					}
				}
			}
		}

		private double nonBlendSkillImpactOnPeriodForProjection(ISkillStaffPeriod skillStaffPeriod,
		                                                               IEnumerable<IScheduleDay> shiftList, ISkill skill)
		{
			var container = new ResourceCalculationDataContainer(_personSkillProvider);
			
			var resolution = skill.DefaultResolution;
			foreach (var scheduleDay in shiftList)
			{
				container.AddScheduleDayToContainer(scheduleDay, resolution);
			}

			return container.ActivityResourcesWhereSeatRequired(skill, skillStaffPeriod.Period);
		}
	}
}
