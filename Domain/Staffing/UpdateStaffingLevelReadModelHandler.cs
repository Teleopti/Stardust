using System.Timers;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly UpdateStaffingLevelReadModelOnlySkillCombinationResources _updateStaffingLevelReadModel;
		private readonly INow _now;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public UpdateStaffingLevelReadModelHandler(UpdateStaffingLevelReadModelOnlySkillCombinationResources updateStaffingLevelReadModel, INow now, IJobStartTimeRepository jobStartTimeRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_now = now;
			_jobStartTimeRepository = jobStartTimeRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			var jobLockTimer = new Timer(30000) {AutoReset = true};
			jobLockTimer.Elapsed += updateJobLock;

			try
			{
				jobLockTimer.Start();
				_jobStartTimeRepository.UpdateLockTimestamp(@event.LogOnBusinessUnitId);
				var now = _now.UtcDateTime();
				var period = new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(@event.Days).AddHours(1));

				_updateStaffingLevelReadModel.Update(period);

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
