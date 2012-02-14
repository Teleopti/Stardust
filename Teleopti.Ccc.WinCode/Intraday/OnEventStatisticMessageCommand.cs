﻿using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Intraday
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