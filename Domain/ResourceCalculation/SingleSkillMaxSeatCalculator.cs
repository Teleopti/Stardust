using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillMaxSeatCalculator
	{
		void Calculate(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,IList<IVisualLayerCollection> toRemove, IList<IVisualLayerCollection> toAdd);	
	}

	public class SingleSkillMaxSeatCalculator : ISingleSkillMaxSeatCalculator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Calculate(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, IList<IVisualLayerCollection> toRemove, IList<IVisualLayerCollection> toAdd)
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

		private static double nonBlendSkillImpactOnPeriodForProjection(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayerCollection> shiftList, ISkill skill)
		{
			double result = 0;
			foreach (var layercollection in shiftList)
			{
				var dateOnly = skillStaffPeriodDate(skillStaffPeriod, layercollection.Person);
				if (!checkPersonSkill(skill, layercollection.Person, dateOnly))
					continue;

				result += calculateShift(skillStaffPeriod, layercollection);
			}

			return result;
		}

		private static double calculateShift(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayer> layercollection)
		{
			double result = 0;
			var skillStaffPeriodElapsedTime = skillStaffPeriod.Period.ElapsedTime().Ticks;
			foreach (var layer in layercollection)
			{
				var activity = layer.Payload as IActivity;
				if (activity == null)
					continue;

				if (!activity.RequiresSeat)
					continue;

				var intersection = skillStaffPeriod.Period.Intersection(layer.Period);
				if (intersection.HasValue)
				{
					result += (double)intersection.Value.ElapsedTime().Ticks / skillStaffPeriodElapsedTime;
				}
			}
			return result;
		}

		private static DateOnly skillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person)
		{
			var localStartDateTime = skillStaffPeriod.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
			return new DateOnly(localStartDateTime.Date);
		}

		private static bool checkPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate)
		{
			var personPeriod = person.Period(skillStaffPeriodDate);
			if (personPeriod == null)
				return false;

			foreach (var personSkill in personPeriod.PersonMaxSeatSkillCollection)
			{
				if (personSkill.Skill.Equals(skill))
					return true;
			}

			return false;
		}
	}
}
