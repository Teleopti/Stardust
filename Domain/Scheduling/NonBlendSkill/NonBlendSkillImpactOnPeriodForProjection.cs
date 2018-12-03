using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
    public interface INonBlendSkillImpactOnPeriodForProjection
    {
        double CalculatePeriod(ISkillStaffPeriod skillStaffPeriod, IEnumerable<IVisualLayer> shift, IActivity skillActivity);
        DateOnly SkillStaffPeriodDate(ISkillStaffPeriod skillStaffPeriod, IPerson person);
    }

    public class NonBlendSkillImpactOnPeriodForProjection : INonBlendSkillImpactOnPeriodForProjection
    {
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
    }
}