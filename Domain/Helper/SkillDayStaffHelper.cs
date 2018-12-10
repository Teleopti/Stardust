using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
    /// <summary>
    /// Helper class for periodization of data needed for erlang calculation
    /// </summary>
    public static class SkillDayStaffHelper
    {
        /// <summary>
        /// Traverses the list, returning a combined TemplateTaskPeriod list.
        /// </summary>
        /// <param name="allTaskPeriods">All periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-14
        /// </remarks>
        public static IEnumerable<ITemplateTaskPeriod> CombineList(IEnumerable<ITemplateTaskPeriod> allTaskPeriods)
        {
            IList<ITemplateTaskPeriod> outList = new List<ITemplateTaskPeriod>();
            IList<DateTimePeriod> sortedUniqueTimePeriods = CreateUniqueDateTimePeriodList(allTaskPeriods.OfType<IPeriodized>());
            var sortedTaskPeriods = allTaskPeriods.OrderBy(t => t.Period.StartDateTime).ToList();

            var previousIndex = 0;
            for (int i = 0; i < sortedUniqueTimePeriods.Count; i++)
            {
                DateTimePeriod timePeriod = sortedUniqueTimePeriods[i];

                IList<ITemplateTaskPeriod> allAffectedTaskPeriods = new List<ITemplateTaskPeriod>();
                for (int j = previousIndex; j < sortedTaskPeriods.Count; j++)
                {
                    var periodToCompare = sortedTaskPeriods[j].Period;
                    if (periodToCompare.StartDateTime>=timePeriod.EndDateTime) break;
                    if (periodToCompare.EndDateTime<=timePeriod.StartDateTime) continue;

                    allAffectedTaskPeriods.Add(sortedTaskPeriods[j]);
                    previousIndex = Math.Max(0, j - 1);
                }

                if (allAffectedTaskPeriods.Count == 0) continue;

                ITemplateTaskPeriod thePeriodTask;
                if (allAffectedTaskPeriods.Count == 1)
                {
                    thePeriodTask = CreatePercentageTask(timePeriod, allAffectedTaskPeriods[0]);
                }
                else
                {
                    thePeriodTask = CreateCombinedPecentageTask(timePeriod, allAffectedTaskPeriods);
                }
                outList.Add(thePeriodTask);
            }

            return outList;
        }

        /// <summary>
        /// Combines the task periods and service level periods.
        /// </summary>
        /// <param name="combinedTaskPeriods">The combined task periods.</param>
        /// <param name="saPeriods">The sa periods.</param>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-02-13
        /// </remarks>
        public static void CombineTaskPeriodsAndServiceLevelPeriods(IEnumerable<ITemplateTaskPeriod> combinedTaskPeriods, IEnumerable<ISkillDataPeriod> saPeriods, IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            var periodizedTaskPeriods = combinedTaskPeriods.OfType<IPeriodized>();
            var periodizedSkillData = saPeriods.OfType<IPeriodized>();

            //Fill skill staff periods with data
            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
                skillStaffPeriod.Reset();
                skillStaffPeriod.IsAvailable = false;

                ITemplateTaskPeriod taskPeriod = (ITemplateTaskPeriod)FindItemAtPointInTime(
                    periodizedTaskPeriods,
                    skillStaffPeriod.Period.StartDateTime);
                if (taskPeriod != null)
                {
                    ISkillDataPeriod skillDataPeriod = (ISkillDataPeriod)FindItemAtPointInTime(
                        periodizedSkillData,
                        skillStaffPeriod.Period.StartDateTime);
                    if (skillDataPeriod!=null)
                    {
                        DateTimePeriod intersection = skillStaffPeriod.Period.Intersection(taskPeriod.Period).Value;
                        double percentage = intersection.ElapsedTime().TotalSeconds / taskPeriod.Period.ElapsedTime().TotalSeconds;
                        skillStaffPeriod.Payload.TaskData = new Task(
                            taskPeriod.TotalTasks*percentage +
                            taskPeriod.AggregatedTasks*percentage,
                            taskPeriod.TotalAverageTaskTime,
                            taskPeriod.TotalAverageAfterTaskTime);
                        skillStaffPeriod.Payload.ServiceAgreementData = skillDataPeriod.ServiceAgreement;
                        skillStaffPeriod.Payload.Shrinkage = skillDataPeriod.Shrinkage;
                        skillStaffPeriod.Payload.Efficiency = skillDataPeriod.Efficiency;
                        skillStaffPeriod.Payload.ManualAgents = skillDataPeriod.ManualAgents;
                        skillStaffPeriod.Payload.SkillPersonData = skillDataPeriod.SkillPersonData;
                        skillStaffPeriod.IsAvailable = true;
                    }
                }
            }
        }

        /// <summary>
        /// Combines the skill staff periods and multisite periods.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="multisitePeriods">The multisite periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-05
        /// </remarks>
        public static IList<ISkillStaffPeriod> CombineSkillStaffPeriodsAndMultisitePeriods(
            IList<ISkillStaffPeriod> skillStaffPeriods,
            IEnumerable<IMultisitePeriod> multisitePeriods)
        {
            InParameter.NotNull(nameof(skillStaffPeriods), skillStaffPeriods);
            InParameter.NotNull(nameof(multisitePeriods), multisitePeriods);

            IEnumerable<IPeriodized> periodizedSkillStaff = skillStaffPeriods.OfType<IPeriodized>();
            IEnumerable<IPeriodized> periodizedMultisite = multisitePeriods.OfType<IPeriodized>();

            IList<ISkillStaffPeriod> outList = new List<ISkillStaffPeriod>();
            IList<DateTimePeriod> timePeriods = CreateUniqueDateTimePeriodList(
                periodizedSkillStaff.Concat(periodizedMultisite));

            //for each timeperiod, get the values from the two lists and create a skillstaffperiod
            foreach (DateTimePeriod timePeriod in timePeriods)
            {
                ISkillStaffPeriod skillStaffPeriod = (ISkillStaffPeriod)FindItemAtPointInTime(
                    periodizedSkillStaff, 
                    timePeriod.StartDateTime);
                if (skillStaffPeriod != null)
                {
                    IMultisitePeriod multisitePeriod = (IMultisitePeriod)FindItemAtPointInTime(
                        periodizedMultisite, 
                        timePeriod.StartDateTime);
                    if (multisitePeriod != null)
                    {
                        DateTimePeriod intersection = timePeriod.Intersection(skillStaffPeriod.Period).Value;
                        double percentage = intersection.ElapsedTime().TotalSeconds / skillStaffPeriod.Period.ElapsedTime().TotalSeconds;
                        ISkillStaffPeriod skillstaffPeriod = new SkillStaffPeriod(timePeriod,
                            new Task(skillStaffPeriod.Payload.TaskData.Tasks * percentage, 
                                     skillStaffPeriod.Payload.TaskData.AverageTaskTime, 
                                     skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime),
                            skillStaffPeriod.Payload.ServiceAgreementData);
                        skillstaffPeriod.Payload.SkillPersonData = skillStaffPeriod.Payload.SkillPersonData;
                        skillstaffPeriod.Payload.Shrinkage = skillStaffPeriod.Payload.Shrinkage;
                        skillstaffPeriod.Payload.Efficiency = skillStaffPeriod.Payload.Efficiency;
                        ((SkillStaff)skillstaffPeriod.Payload).CalculatedOccupancy = skillStaffPeriod.Payload.CalculatedOccupancy;
                        skillstaffPeriod.IsAvailable = true;
                        outList.Add(skillstaffPeriod);
                    }
                }
            }
            return outList;
        }


        private static IPeriodized FindItemAtPointInTime(IEnumerable<IPeriodized> periods, DateTime time)
        {
            foreach (var periodized in periods)
            {
                var period = periodized.Period;
                if (period.StartDateTime>time) continue;
                if (period.EndDateTime <= time) continue;
                return periodized;
            }
            return null;
        }

        public static IList<DateTimePeriod> CreateUniqueDateTimePeriodList(IEnumerable<IPeriodized> periodizedItems)
        {
            IList<DateTime> sortedUniqueTimes = SortedUniqueTimes(periodizedItems.Select(p => p.Period));

            //create timeperiods of all the unique times
            IList<DateTimePeriod> timePeriods = new List<DateTimePeriod>();
            int max = sortedUniqueTimes.Count;
            for (int index = 1; index < max; index++)
            {
                timePeriods.Add(
                    new DateTimePeriod(
                        sortedUniqueTimes[index - 1],
                        sortedUniqueTimes[index]));
            }
            return timePeriods;
        }

        private static IList<DateTime> SortedUniqueTimes(IEnumerable<DateTimePeriod> allPeriods)
		{
			return allPeriods.SelectMany(p => new[] {p.StartDateTime, p.EndDateTime}).Distinct().OrderBy(t => t).ToList();
		}

        private static ITemplateTaskPeriod CreatePercentageTask(DateTimePeriod basePeriod, ITemplateTaskPeriod taskPeriod)
        {
            double percent = basePeriod.ElapsedTime().TotalSeconds /
                                     taskPeriod.Period.ElapsedTime().
                                         TotalSeconds;
            double tasks = taskPeriod.Task.Tasks * percent;
            double aggregatedTasks = taskPeriod.AggregatedTasks*percent;
            TimeSpan averageTaskTime = taskPeriod.Task.AverageTaskTime;
            TimeSpan averageAfterTaskTime = taskPeriod.Task.AverageAfterTaskTime;

            ITemplateTaskPeriod resultTaskPeriod = new TemplateTaskPeriod(
                new Task(tasks, averageTaskTime, averageAfterTaskTime),
                taskPeriod.Campaign,
                basePeriod);
            resultTaskPeriod.AggregatedTasks = aggregatedTasks;

            return resultTaskPeriod;
        }

        private static ITemplateTaskPeriod CreateCombinedPecentageTask(DateTimePeriod basePeriod, IList<ITemplateTaskPeriod> allAffectedTaskPeriods)
        {
            double percent = basePeriod.ElapsedTime().TotalSeconds /
                                     allAffectedTaskPeriods[0].Period.ElapsedTime().
                                         TotalSeconds;
            double tasks = allAffectedTaskPeriods[0].Tasks * percent;
            double totalTasks = allAffectedTaskPeriods[0].TotalTasks * percent;
            double taskTime = allAffectedTaskPeriods[0].AverageTaskTime.TotalSeconds * tasks;
            double afterTaskTime = allAffectedTaskPeriods[0].AverageAfterTaskTime.TotalSeconds * tasks;
            double totalTaskTime = allAffectedTaskPeriods[0].TotalAverageTaskTime.TotalSeconds * totalTasks;
            double totalAfterTaskTime = allAffectedTaskPeriods[0].TotalAverageAfterTaskTime.TotalSeconds * totalTasks;
            double aggregatedTasks = allAffectedTaskPeriods[0].AggregatedTasks*percent;
            for (int affectedTaskPeriodsIndex = 1; affectedTaskPeriodsIndex < allAffectedTaskPeriods.Count; affectedTaskPeriodsIndex++)
            {
                percent = basePeriod.ElapsedTime().TotalSeconds /
                             allAffectedTaskPeriods[affectedTaskPeriodsIndex].Period.ElapsedTime().
                                 TotalSeconds;
                double otherTasks = allAffectedTaskPeriods[affectedTaskPeriodsIndex].Tasks * percent;
                tasks += otherTasks;
                taskTime +=
                    allAffectedTaskPeriods[affectedTaskPeriodsIndex].AverageTaskTime.TotalSeconds *
                    otherTasks;
                afterTaskTime +=
                    allAffectedTaskPeriods[affectedTaskPeriodsIndex].AverageAfterTaskTime.TotalSeconds *
                    otherTasks;
                double otherTotalTasks = allAffectedTaskPeriods[affectedTaskPeriodsIndex].TotalTasks * percent;
                totalTasks += otherTotalTasks;
                totalTaskTime +=
                    allAffectedTaskPeriods[affectedTaskPeriodsIndex].TotalAverageTaskTime.TotalSeconds *
                    otherTotalTasks;
                totalAfterTaskTime +=
                    allAffectedTaskPeriods[affectedTaskPeriodsIndex].TotalAverageAfterTaskTime.TotalSeconds *
                    otherTotalTasks;
                aggregatedTasks +=  allAffectedTaskPeriods[affectedTaskPeriodsIndex].AggregatedTasks * percent;
            }

            TimeSpan averageTaskTime = TimeSpan.FromSeconds(0);
            TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(0);
            Percent campaignTasks = new Percent();
            Percent campaignTaskTime = new Percent();
            Percent campaignAfterTaskTime = new Percent();

            if (denominatorNotZero(new[]{tasks}))
            {
                campaignTasks = new Percent(totalTasks / tasks - 1d);

                averageTaskTime = TimeSpan.FromMilliseconds(taskTime * 1000 / tasks);
                averageAfterTaskTime = TimeSpan.FromMilliseconds(afterTaskTime * 1000 / tasks);
            }
	        if (denominatorNotZero(new[] {totalTasks, tasks}) && taskTime > 0)
		        campaignTaskTime = new Percent(((totalTaskTime/totalTasks)/(taskTime/tasks)) - 1);

	        if (denominatorNotZero(new []{totalTasks, tasks}) && afterTaskTime > 0)
		        campaignAfterTaskTime = new Percent(((totalAfterTaskTime/totalTasks)/(afterTaskTime/tasks)) - 1);

	        var resultingTaskPeriod = new TemplateTaskPeriod(
                new Task(tasks, averageTaskTime, averageAfterTaskTime),
                new Campaign(campaignTasks, campaignTaskTime, campaignAfterTaskTime),
                basePeriod);
            resultingTaskPeriod.AggregatedTasks = aggregatedTasks;

            return resultingTaskPeriod;
        }

		private static bool denominatorNotZero(IEnumerable<double> args)
		{
			return args.All(a => a > 0);
		}
    }
}
