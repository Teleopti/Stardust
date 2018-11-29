using System;
using System.Timers;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly INow _now;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateStaffingLevelReadModelHandler));
		private readonly UpdateStaffingLevelReadModelStartDate _updateStaffingLevelReadModelStartDate;

		public UpdateStaffingLevelReadModelHandler(IUpdateStaffingLevelReadModel updateStaffingLevelReadModel, INow now, IJobStartTimeRepository jobStartTimeRepository, ICurrentBusinessUnit currentBusinessUnit, ICurrentUnitOfWork currentUnitOfWork, UpdateStaffingLevelReadModelStartDate updateStaffingLevelReadModelStartDate)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_now = now;
			_jobStartTimeRepository = jobStartTimeRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_currentUnitOfWork = currentUnitOfWork;
			_updateStaffingLevelReadModelStartDate = updateStaffingLevelReadModelStartDate;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			DoTheWork(@event);
		}

		public void DoTheWork(UpdateStaffingLevelReadModelEvent @event)
		{
			var jobLockTimer = new Timer(60000) { AutoReset = true };
			jobLockTimer.Elapsed += updateJobLock;

			try
			{
				jobLockTimer.Start();
				_jobStartTimeRepository.UpdateLockTimestamp(@event.LogOnBusinessUnitId);
				var now = _now.UtcDateTime();
				_updateStaffingLevelReadModelStartDate.RememberStartDateTime(now.AddDays(-1).AddHours(-1));
				var period = new DateTimePeriod(_updateStaffingLevelReadModelStartDate.StartDateTime, now.AddDays(@event.Days).AddHours(1));

				_updateStaffingLevelReadModel.Update(period);
				var current = _currentUnitOfWork.Current();
				if (current.IsDirty())
				{
					current.Clear();
					logger.Warn("The unit of work was modified while running the readmodel update job on business unit " +
								@event.LogOnBusinessUnitId);
				}

				_jobStartTimeRepository.ResetLockTimestamp(@event.LogOnBusinessUnitId);
			}
			catch (Exception exception)
			{
				_jobStartTimeRepository.RemoveLock(@event.LogOnBusinessUnitId);
				throw exception;
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

	public class UpdateStaffingLevelReadModelStartDate
	{
		private DateTime _startDateTime;

		public void RememberStartDateTime(DateTime dateTime)
		{
			_startDateTime = dateTime;
		}

		public DateTime StartDateTime => _startDateTime;
	}
}
