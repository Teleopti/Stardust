using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class OnEventStatisticMessageCommand : IMessageHandlerCommand
    {
        private readonly IIntradayView _view;

        public OnEventStatisticMessageCommand(IIntradayView view) : this()
        {
            _view = view;
        }

    	protected OnEventStatisticMessageCommand()
    	{
    	}

    	public virtual void Execute(IEventMessage eventMessage)
        {
            //Refresh of the currently selected tab
            _view.DrawSkillGrid();
        }
    }
}