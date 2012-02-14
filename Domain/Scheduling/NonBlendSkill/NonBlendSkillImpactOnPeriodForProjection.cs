using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
    public interface INonBlendSkillImpactOnPeriodForProjection
    {
        double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IList<IVisualLayerCollection> shiftList, ISkill skill);
        double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayer> shift, IActivity skillActivity);
        DateOnly SkillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person);
        bool CheckPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate);
    }

    public class NonBlendSkillImpactOnPeriodForProjection : INonBlendSkillImpactOnPeriodForProjection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IList<IVisualLayerCollection> shiftList, ISkill skill)
        {
            //ISkill skill = ((ISkillDay)skillStaffPeriod.Parent).Skill;
            //if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
            //    return 0;
            
            double result = 0;
            foreach (var layercollection in shiftList)
            {
                DateOnly dateOnly = SkillStaffPeriodDate(skillStaffPeriod, layercollection.Person);
                if (!CheckPersonSkill(skill, layercollection.Person, dateOnly))
                    continue;

                result += calculateShift(skillStaffPeriod, layercollection, skill.Activity);
            }

            return result;
        }

        public double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayer> shift, IActivity skillActivity)
        {
            return calculateShift(skillStaffPeriod, shift, skillActivity);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public DateOnly SkillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person)
        {
            DateTime localStartDateTime =
                skillStaffPeriod.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
            return new DateOnly(localStartDateTime.Date);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool CheckPersonSkill(ISkill skill, IPerson person, DateOnly skillStaffPeriodDate)
        {
            IPersonPeriod personPeriod = person.Period(skillStaffPeriodDate);
            if (personPeriod == null)
                return false;

            foreach (var personSkill in personPeriod.PersonNonBlendSkillCollection)
            {
                if (personSkill.Skill.Equals(skill))
                    return true;
            }

            return false;
        }
    }
}