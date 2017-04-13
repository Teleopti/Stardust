using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class OnEventMeetingMessageCommand : IMessageHandlerCommand
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OnEventMeetingMessageCommand));

        private readonly IIntradayView _view;
        private readonly ISchedulingResultLoader _schedulingResultLoader;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public OnEventMeetingMessageCommand(IIntradayView view, ISchedulingResultLoader schedulingResultLoader, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
            : this()
        {
            _view = view;
            _schedulingResultLoader = schedulingResultLoader;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }

        protected OnEventMeetingMessageCommand()
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
                updateInsertOnEvent(eventMessage, person);
            }

            _schedulingResultLoader.InitializeScheduleData();
            _view.DrawSkillGrid();
        }


        private void deleteOnEvent(IEventMessage message)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Message broker - Removing " + message.DomainObjectType + " [" + message.DomainObjectId + "]");

            _schedulingResultLoader.SchedulerState.Schedules.DeleteMeetingFromBroker(message.DomainObjectId);
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

                deleteOnEvent(message);
                associateRootsForPersonAssignment(uow);
                _schedulingResultLoader.SchedulerState.Schedules.MeetingUpdateFromBroker(_repositoryFactory.CreateMeetingRepository(uow), message.DomainObjectId);
                _view.ReloadScheduleDayInEditor(person);
            }
        }

        private void associateRootsForPersonAssignment(IUnitOfWork uow)
        {
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Activities);
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.ShiftCategories);
        }
    }
}