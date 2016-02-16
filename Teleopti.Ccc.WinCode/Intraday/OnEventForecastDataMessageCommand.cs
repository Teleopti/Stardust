using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class OnEventForecastDataMessageCommand : IMessageHandlerCommand
    {
        private readonly IIntradayView _view;
        private readonly ISchedulingResultLoader _schedulingResultLoader;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public OnEventForecastDataMessageCommand(IIntradayView view, ISchedulingResultLoader schedulingResultLoader, IUnitOfWorkFactory unitOfWorkFactory) : this()
        {
            _view = view;
            _schedulingResultLoader = schedulingResultLoader;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		protected OnEventForecastDataMessageCommand()
		{
		}

        public virtual void Execute(IEventMessage eventMessage)
        {
            ISkill skill = _view.SelectedSkill;
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _schedulingResultLoader.ReloadForecastData(unitOfWork);
            }

            if (_schedulingResultLoader.SchedulerState.SchedulingResultState.Skills.IsEmpty()) return;

            _view.SetupSkillTabs();
            _view.SelectSkillTab(skill);
            _view.DrawSkillGrid();
        }
    }
}