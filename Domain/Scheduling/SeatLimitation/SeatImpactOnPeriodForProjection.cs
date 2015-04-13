using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface ISeatImpactOnPeriodForProjection
	{
		double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IList<IVisualLayerCollection> shiftList);
		double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IVisualLayerCollection shift);
		DateOnly SkillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person);
		bool CheckPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate);
	}

	public class SeatImpactOnPeriodForProjection : ISeatImpactOnPeriodForProjection
	{
        public double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IList<IVisualLayerCollection> shiftList)
		{
			var skill = skillStaffPeriod.SkillDay.Skill;
			if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
				return 0;

			double result = 0;
			foreach (var layercollection in shiftList)
			{
				DateOnly dateOnly = SkillStaffPeriodDate(skillStaffPeriod, layercollection.Person);
				if (!CheckPersonSkill(skill, layercollection.Person, dateOnly))
					continue;

				result += calculateShift(skillStaffPeriod, layercollection);
			}

			return result;
		}

        public double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IVisualLayerCollection shift)
		{
			return calculateShift(skillStaffPeriod, shift);
		}

		private static double calculateShift(ISkillStaffPeriod skillStaffPeriod, IVisualLayerCollection layercollection)
		{
			double result = 0;
			foreach (var layer in layercollection)
			{
				IActivity activity = layer.Payload as IActivity;
				if (activity == null)
				{
					activity = layer.Payload.UnderlyingPayload as IActivity;
					if(activity == null)
						continue;	
				}
					
				if (!activity.RequiresSeat)
					continue;

				DateTimePeriod? intersection = skillStaffPeriod.Period.Intersection(layer.Period);
				if (intersection.HasValue)
				{
					result += intersection.Value.ElapsedTime().TotalSeconds / skillStaffPeriod.Period.ElapsedTime().TotalSeconds;
				}
			}
			return result;
		}

		public DateOnly SkillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person)
		{
			DateTime localStartDateTime =
				skillStaffPeriod.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
			return new DateOnly(localStartDateTime.Date);
		}

		public bool CheckPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate)
		{
			IPersonPeriod personPeriod = person.Period(skillStaffPeriodDate);
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