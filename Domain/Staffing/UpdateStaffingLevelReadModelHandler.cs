using System.Timers;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;
		private readonly INow _now;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public UpdateStaffingLevelReadModelHandler(IUpdateStaffingLevelReadModel updateStaffingLevelReadModel, ICurrentUnitOfWorkFactory current, INow now, IJobStartTimeRepository jobStartTimeRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_currentFactory = current;
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
				_jobStartTimeRepository.UpdateLockTimestamp(_currentBusinessUnit.Current().Id.GetValueOrDefault());
				var period = new DateTimePeriod(_now.UtcDateTime().AddDays(-1).AddHours(-1), _now.UtcDateTime().AddDays(@event.Days).AddHours(1));

				_updateStaffingLevelReadModel.Update(period);

				var current = _currentFactory.Current().CurrentUnitOfWork();
				//an ugly solution for bug 39594
				if (current.IsDirty())
					current.Clear();

				_jobStartTimeRepository.ResetLockTimestamp(_currentBusinessUnit.Current().Id.GetValueOrDefault());
			}
			finally
			{
				jobLockTimer.Dispose();
			}
		}

		private void updateJobLock(object source, ElapsedEventArgs e)
		{
			_jobStartTimeRepository.UpdateLockTimestamp(_currentBusinessUnit.Current().Id.GetValueOrDefault());
		}
	}
}
