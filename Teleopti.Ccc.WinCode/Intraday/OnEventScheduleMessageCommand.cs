using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IMessageHandlerCommand
    {
        void Execute(IEventMessage eventMessage);
    }

    public class OnEventScheduleMessageCommand : IMessageHandlerCommand
    {
        private readonly IIntradayView _view;
        private readonly ISchedulingResultLoader _schedulingResultLoader;
	    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IScheduleRefresher _scheduleRefresher;

        public OnEventScheduleMessageCommand(IIntradayView view, ISchedulingResultLoader schedulingResultLoader, IUnitOfWorkFactory unitOfWorkFactory, IScheduleRefresher scheduleRefresher) : this()
        {
            _view = view;
            _schedulingResultLoader = schedulingResultLoader;
	        _unitOfWorkFactory = unitOfWorkFactory;
            _scheduleRefresher = scheduleRefresher;
        }

		protected OnEventScheduleMessageCommand()
		{
		}

        public virtual void Execute(IEventMessage eventMessage)
        {
            var conflicts = new List<PersistConflict>();
			var refreshedEntitiesBuffer = new Collection<IPersistableScheduleData>();

            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                uow.Reassociate(_schedulingResultLoader.Contracts);
                uow.Reassociate(_schedulingResultLoader.ContractSchedules);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.RequestedScenario);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.SchedulingResultState.PersonsInOrganization);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Absences);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Activities);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.ShiftCategories);

                _scheduleRefresher.Refresh(_schedulingResultLoader.SchedulerState.Schedules, new List<IEventMessage>{eventMessage}, refreshedEntitiesBuffer, conflicts, isRelevantPerson);

                if (refreshedEntitiesBuffer.Count > 0)
                {
                    _view.ReloadScheduleDayInEditor(refreshedEntitiesBuffer[0].Person);
                }
            }

            _schedulingResultLoader.InitializeScheduleData();
            _view.DrawSkillGrid();
        }

		private bool isRelevantPerson(Guid personId)
		{
			return _schedulingResultLoader.SchedulerState.SchedulingResultState.PersonsInOrganization.Any(p => p.Id == personId);
		}
    }
}