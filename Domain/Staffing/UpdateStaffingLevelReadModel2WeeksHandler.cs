using System.Timers;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModel2WeeksHandler : IHandleEvent<UpdateStaffingLevelReadModel2WeeksEvent>, IRunOnStardust
	{
		private readonly INow _now;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateStaffingLevelReadModel2WeeksHandler));
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public UpdateStaffingLevelReadModel2WeeksHandler(INow now, CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory, 
			LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceCalculation, ISkillCombinationResourceRepository skillCombinationResourceRepository, IStardustJobFeedback stardustJobFeedback, ICurrentUnitOfWork currentUnitOfWork, IJobStartTimeRepository jobStartTimeRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_now = now;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceCalculation = resourceCalculation;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_stardustJobFeedback = stardustJobFeedback;
			_currentUnitOfWork = currentUnitOfWork;
			_jobStartTimeRepository = jobStartTimeRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModel2WeeksEvent @event)
		{
			var jobLockTimer = new Timer(30000) { AutoReset = true };
			jobLockTimer.Elapsed += updateJobLock;

			try
			{
				jobLockTimer.Start();
				_jobStartTimeRepository.UpdateLockTimestamp(@event.LogOnBusinessUnitId);

				var updateThingy = new UpdateStaffingLevelReadModelOnlySkillCombinationResources(_now, _cascadingResourceCalculationContextFactory,
				_loaderForResourceCalculation, _resourceCalculation, _skillCombinationResourceRepository, _stardustJobFeedback);
				updateThingy.Update(new DateTimePeriod(_now.UtcDateTime().AddDays(-1).AddHours(-1), _now.UtcDateTime().AddDays(@event.Days).AddHours(1)));

				var current = _currentUnitOfWork.Current();
				if (current.IsDirty())
				{
					current.Clear();
					logger.Warn("The unit of work was modified while running the readmodel update job on business unit " + @event.LogOnBusinessUnitId);
				}
				_jobStartTimeRepository.ResetLockTimestamp(@event.LogOnBusinessUnitId);
			}
			finally
			{
				jobLockTimer.Elapsed -= updateJobLock;
				jobLockTimer.Dispose();
			}
			
		}
		private void updateJobLock(object source, ElapsedEventArgs e)
		{
			_jobStartTimeRepository.UpdateLockTimestamp(_currentBusinessUnit.Current().Id.GetValueOrDefault());
		}
	}
}
