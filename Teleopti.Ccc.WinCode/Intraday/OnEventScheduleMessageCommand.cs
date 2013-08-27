using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IMessageHandlerCommand
    {
        void Execute(IEventMessage eventMessage);
    }

    public class OnEventScheduleMessageCommand : IMessageHandlerCommand
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OnEventScheduleMessageCommand));

        private readonly IIntradayView _view;
        private readonly ISchedulingResultLoader _schedulingResultLoader;
	    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public OnEventScheduleMessageCommand(IIntradayView view, ISchedulingResultLoader schedulingResultLoader, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory) : this()
        {
            _view = view;
            _schedulingResultLoader = schedulingResultLoader;
	        _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }

		protected OnEventScheduleMessageCommand()
		{
		}

        public virtual void Execute(IEventMessage eventMessage)
		{
			var person =
				_schedulingResultLoader.SchedulerState.SchedulingResultState.PersonsInOrganization.FirstOrDefault(
					p => p.Id == eventMessage.ReferenceObjectId);
			if (person == null)
			{
				Logger.Info("Person with id " + eventMessage.ReferenceObjectId + " was not found in memory.");
				return;
			}

            if (eventMessage.DomainUpdateType == DomainUpdateType.Delete)
            {
                deleteOnEvent(eventMessage);
            }
            else
            {
                updateInsertOnEvent(eventMessage,person);
            }

            _schedulingResultLoader.InitializeScheduleData();
            _view.DrawSkillGrid();
        }


        private void deleteOnEvent(IEventMessage message)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Message broker - Removing " + message.DomainObjectType + " [" + message.DomainObjectId + "]");

				if (message.InterfaceType.IsAssignableFrom(typeof(IMeetingChangedEntity)))
			{
				_schedulingResultLoader.SchedulerState.Schedules.DeleteMeetingFromBroker(message.DomainObjectId);
			}
			else
			{
				_schedulingResultLoader.SchedulerState.Schedules.DeleteFromBroker(message.DomainObjectId);
			}
        }

        private void updateInsertOnEvent(IEventMessage message, IPerson person)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                if (Logger.IsInfoEnabled)
                    Logger.Info("Message broker - Updating " + message.DomainObjectType + " [" + message.DomainObjectId + "]");

                uow.Reassociate(_schedulingResultLoader.Contracts);
                uow.Reassociate(_schedulingResultLoader.ContractSchedules);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.RequestedScenario);
                uow.Reassociate(_schedulingResultLoader.SchedulerState.SchedulingResultState.PersonsInOrganization);

                if (message.InterfaceType.IsAssignableFrom(typeof(IPersonAssignment)))
                {
                    associateRootsForPersonAssignment(uow);
                    _schedulingResultLoader.SchedulerState.Schedules.UpdateFromBroker(_repositoryFactory.CreatePersonAssignmentRepository(uow), message.DomainObjectId);
                    _view.ReloadScheduleDayInEditor(person);
                    return;
                }
                if (message.InterfaceType.IsAssignableFrom(typeof(IPersonAbsence)))
                {
                    associateRoootsForPersonAbsence(uow);
                    _schedulingResultLoader.SchedulerState.Schedules.UpdateFromBroker(_repositoryFactory.CreatePersonAbsenceRepository(uow), message.DomainObjectId);
                    _view.ReloadScheduleDayInEditor(person);
                    return;
                }
					 if (message.InterfaceType.IsAssignableFrom(typeof(IMeetingChangedEntity)))
				{
					deleteOnEvent(message);
					associateRootsForPersonAssignment(uow);
					_schedulingResultLoader.SchedulerState.Schedules.MeetingUpdateFromBroker(_repositoryFactory.CreateMeetingRepository(uow), message.DomainObjectId);
					_view.ReloadScheduleDayInEditor(person);
					return;
				}

                Logger.Warn("Message broker - Got a message of an unknown IScheduleData type: " + message.DomainObjectType);
            }
        }

        private void associateRoootsForPersonAbsence(IUnitOfWork uow)
        {
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Absences);
        }

        private void associateRootsForPersonAssignment(IUnitOfWork uow)
        {
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Activities);
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.ShiftCategories);
        }
    }
}