using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Helper class for working with statistics
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-04-02
    /// </remarks>
    public class StatisticHelper : IStatisticHelper
    {
        private readonly ISkillDayRepository _skillDayRepository;
        private readonly IStatisticRepository _statisticTaskRepository;
        private readonly IValidatedVolumeDayRepository _validatedVolumeDayRepository;
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        public StatisticHelper(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
        {
            InParameter.NotNull("repositoryFactory", repositoryFactory);
            InParameter.NotNull("unitOfWork",unitOfWork);

            _skillDayRepository = repositoryFactory.CreateSkillDayRepository(unitOfWork);
            _statisticTaskRepository = repositoryFactory.CreateStatisticRepository();
            _validatedVolumeDayRepository = repositoryFactory.CreateValidatedVolumeDayRepository(unitOfWork);
        }

        /// <summary>
        /// Occurs when [status changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

	    /// <summary>
	    /// Gets the workload days with statistics.
	    /// </summary>
	    /// <param name="period">The period.</param>
	    /// <param name="workload">The workload.</param>
	    /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: robink
	    /// Created date: 2008-04-02
	    /// </remarks>
	    public IList<ITaskOwner> GetWorkloadDaysWithValidatedStatistics(DateOnlyPeriod period, IWorkload workload, IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays)
        {
            InParameter.NotNull("workload", workload);
            InParameter.NotNull("existingValidatedVolumeDays", existingValidatedVolumeDays);

            IList<IWorkloadDayBase> workloadDays = LoadStatisticData(period, workload);

            ICollection<IValidatedVolumeDay> validatedVolumeDays = _validatedVolumeDayRepository.FindRange(period, workload);
            if (!existingValidatedVolumeDays.IsEmpty())
            {
                foreach (IValidatedVolumeDay validatedVolumeDay in existingValidatedVolumeDays)
                {
                    var volumeDayDate = validatedVolumeDay.VolumeDayDate;
                    IValidatedVolumeDay dayToRemoveFromCollection = validatedVolumeDays.
                        FirstOrDefault(v => v.VolumeDayDate == volumeDayDate);
                    if (dayToRemoveFromCollection!=null) validatedVolumeDays.Remove(dayToRemoveFromCollection);
                }
                validatedVolumeDays = validatedVolumeDays.Concat(existingValidatedVolumeDays).ToList();
            }

            return _validatedVolumeDayRepository.MatchDays(workload,
                workloadDays.OfType<ITaskOwner>().ToList(),
                validatedVolumeDays);
        }

        /// <summary>
        /// Raises the <see cref="E:StatusChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="Teleopti.Ccc.Domain.Common.StatusChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        private void OnStatusChanged(StatusChangedEventArgs eventArgs)
        {
            if (StatusChanged != null)
            {
                StatusChanged.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Loads and match statistic data.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
        /// <param name="longterm">if set to <c>true</c> [longterm].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IList<ISkillDay> LoadStatisticData(
            DateOnlyPeriod period,
            ISkill skill,
            IScenario scenario,
            bool longterm)
        {
            IList<ISkillDay> skillDays = new List<ISkillDay>(_skillDayRepository.FindRange(period, skill, scenario));
            OnStatusChanged(new StatusChangedEventArgs("xxLoadedSkillDaysFromDatasource"));
            skillDays = (IList<ISkillDay>)_skillDayRepository.GetAllSkillDays(period, skillDays, skill, scenario, _ => {});
            OnStatusChanged(new StatusChangedEventArgs("xxCreatedNewSkillDays"));
			_workloadDayHelper.CreateLongtermWorkloadDays(skill, skillDays);
			OnStatusChanged(new StatusChangedEventArgs("xxCreatedNewWorkloadDays"));
            foreach (IWorkload workload in skill.WorkloadCollection)
            {
                IList<IWorkloadDayBase> workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(skillDays, workload);
                IList<IStatisticTask> tasks = (IList<IStatisticTask>)_statisticTaskRepository.LoadSpecificDates(workload.QueueSourceCollection, period.ToDateTimePeriod(skill.TimeZone));
                OnStatusChanged(new StatusChangedEventArgs("xxLoadedStatistics"));
                new Statistic(workload).Match(workloadDays, tasks);
                OnStatusChanged(new StatusChangedEventArgs("xxMatchedStatisticsWithWorkloadDays"));
            }

            return skillDays;
        }

        /// <summary>
        /// Loads and match statistic data.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        public IList<IWorkloadDayBase> LoadStatisticData(
            DateOnlyPeriod period,
            IWorkload workload)
        {
            var timeZone = workload.Skill.TimeZone;
            var statistics = _statisticTaskRepository.LoadSpecificDates(workload.QueueSourceCollection, period.ToDateTimePeriod(timeZone).ChangeEndTime(TimeSpan.FromHours(25)));
            var queueStatistics = new QueueStatisticsProvider(statistics, new QueueStatisticsCalculator(workload.QueueAdjustments));

            IList<IWorkloadDayBase> returnList = new List<IWorkloadDayBase>();
            foreach (var day in period.DayCollection())
            {
                var workloadDayForStatistics = new WorkloadDay();
                workloadDayForStatistics.Create(day,workload,new List<TimePeriod>());
                workloadDayForStatistics.MakeOpen24Hours();
                workloadDayForStatistics.SetQueueStatistics(queueStatistics);
                returnList.Add(workloadDayForStatistics);
            }
            return returnList;
        }

        /// <summary>
        /// Excludes the outliers from statistics.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="outliers">The outliers.</param>
        /// <param name="workloadDays">The workload days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        public static IList<T> ExcludeOutliersFromStatistics<T>(DateOnlyPeriod period, IList<IOutlier> outliers, IEnumerable<T> workloadDays) where T : ITaskOwner
        {
            ICollection<DateOnly> allOutlierDates = Outlier.GetOutliersByDates(period, outliers).Keys;
            return workloadDays.Where(w => !allOutlierDates.Contains(w.CurrentDate)).ToList();
        }
    }
}
