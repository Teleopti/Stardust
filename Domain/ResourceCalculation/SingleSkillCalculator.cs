using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillCalculator
	{
		void Calculate(IList<IVisualLayerCollection> relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods);
	}

	public class SingleSkillCalculator : ISingleSkillCalculator
	{
		public void Calculate(IList<IVisualLayerCollection> relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				var skill = pair.Key;
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
					continue;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;

				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, relevantProjections, skill);
					//if (addToEarlierResult)
					//    result += skillStaffPeriod.Payload.CalculatedLoggedOn;
					skillStaffPeriod.Payload.CalculatedLoggedOn = result;
					skillStaffPeriod.SetCalculatedResource65(result);
				}
			}
		}

		private double nonBlendSkillImpactOnPeriodForProjection(ISkillStaffPeriod skillStaffPeriod, IList<IVisualLayerCollection> shiftList, ISkill skill)
		{
			double result = 0;
			foreach (var layercollection in shiftList)
			{
				DateOnly dateOnly = skillStaffPeriodDate(skillStaffPeriod, layercollection.Person);
				if (!checkPersonSkill(skill, layercollection.Person, dateOnly))
					continue;

				result += calculateShift(skillStaffPeriod, layercollection, skill.Activity);
			}

			return result;
		}

		private static double calculateShift(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayer> layercollection, IActivity skillActivity)
		{
			double result = 0;
			long skillStaffPeriodElapsedTime = skillStaffPeriod.Period.ElapsedTime().Ticks;
			foreach (var layer in layercollection)
			{
				var activity = layer.Payload as IActivity;
				if (activity == null)
					continue;

				if (!skillActivity.Equals(activity))
					continue;

				DateTimePeriod? intersection = skillStaffPeriod.Period.Intersection(layer.Period);
				if (intersection.HasValue)
				{
					result += (double)intersection.Value.ElapsedTime().Ticks / skillStaffPeriodElapsedTime;
				}
			}
			return result;
		}

		private DateOnly skillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person)
		{
			DateTime localStartDateTime =
				skillStaffPeriod.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
			return new DateOnly(localStartDateTime.Date);
		}

		private bool checkPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate)
		{
			IPersonPeriod personPeriod = person.Period(skillStaffPeriodDate);
			if (personPeriod == null)
				return false;

			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				if (personSkill.Skill.Equals(skill))
					return true;
			}

			return false;
		}
	}
}