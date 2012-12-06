using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillCalculator
	{
		void Calculate(IList<IVisualLayerCollection> relevantProjections,
		               ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
		               IList<IVisualLayerCollection> toRemove, IList<IVisualLayerCollection> toAdd);
	}

	public class SingleSkillCalculator : ISingleSkillCalculator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Calculate(IList<IVisualLayerCollection> relevantProjections, 
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, 
			IList<IVisualLayerCollection> toRemove, 
			IList<IVisualLayerCollection> toAdd)
		{
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				var skill = pair.Key;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;

				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result1;
					double result2;
					if(toRemove.Count > 0 || toAdd.Count > 0)
					{
						Tuple<double, double> resultToRemove = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toRemove, skill);
						Tuple<double, double> resultToAdd = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toAdd, skill);
						result1 = skillStaffPeriod.Payload.CalculatedResource - resultToRemove.Item1 + resultToAdd.Item1;
						result2 = skillStaffPeriod.Payload.CalculatedLoggedOn - resultToRemove.Item2 + resultToAdd.Item2;
					}
					else
					{
						Tuple<double, double> result = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, relevantProjections, skill);
						result1 = result.Item1;
						result2 = result.Item2;
					}

					if (!skillStaffPeriod.Payload.CalculatedLoggedOn.Equals(result2))
					{
						skillStaffPeriod.Payload.CalculatedLoggedOn = result2;
						skillStaffPeriod.SetCalculatedResource65(result1);
					}
					
				}
			}
		}

		private static Tuple<double, double> nonBlendSkillImpactOnPeriodForProjection(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayerCollection> shiftList, ISkill skill)
		{
			double result1 = 0;
			double result2 = 0;
			foreach (var layercollection in shiftList)
			{
				DateOnly dateOnly = skillStaffPeriodDate(skillStaffPeriod, layercollection.Person);
				double skillEfficiency = checkPersonSkill(skill, layercollection.Person, dateOnly);
				if (skillEfficiency == 0)
					continue;

				double result = calculateShift(skillStaffPeriod, layercollection, skill.Activity);
				result2 += result;
				result1 += result * skillEfficiency;
			}

			return new Tuple<double, double>(result1, result2);
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

		private static DateOnly skillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person)
		{
			DateTime localStartDateTime =
				skillStaffPeriod.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
			return new DateOnly(localStartDateTime.Date);
		}

		private static double checkPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate)
		{
			IPersonPeriod personPeriod = person.Period(skillStaffPeriodDate);
			if (personPeriod == null)
				return 0;

			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				if (personSkill.Skill.Equals(skill) && personSkill.Active)
					return personSkill.SkillPercentage.Value;
			}

			return 0;
		}
	}
}