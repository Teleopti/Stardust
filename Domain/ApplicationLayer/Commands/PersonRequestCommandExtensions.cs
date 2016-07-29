using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	static class RequestCommandExtensions
	{
		internal static bool TryReplyMessage<TCommand>(this TCommand command, IPersonRequest personRequest)
			where TCommand : IErrorAttachedCommand, IReplyCommand
		{
			if (string.IsNullOrEmpty(command.ReplyMessage))
			{
				return false;
			}

			try
			{
				if (!personRequest.CheckReplyTextLength(command.ReplyMessage))
				{
					command.ErrorMessages.Add(UserTexts.Resources.RequestInvalidMessageLength);
					return false;
				}
				personRequest.Reply(command.ReplyMessage);
			}
			catch (InvalidOperationException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidMessageModification,
					personRequest.StatusText));
				return false;
			}
			return true;
		}
	}
}
