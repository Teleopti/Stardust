using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;

namespace Teleopti.Ccc.Web.Core
{
	public class ErrorMessageProvider : IErrorMessageProvider
	{
		public const string GenericMessage = "Application error, try again or contact your system administrator if the problem persists.";
		public const string GenericShortMessage = "Application error";

		public string ResolveMessage(HandleErrorInfo handleErrorInfo)
		{
			if (handleErrorInfo.Exception is PermissionException || handleErrorInfo.Exception is RequestPersistException)
			{
				return handleErrorInfo.Exception.Message;
			}

			return GenericMessage;
		}

		public string ResolveShortMessage(HandleErrorInfo handleErrorInfo)
		{
			if (handleErrorInfo.Exception is RequestPersistException)
			{
				return (handleErrorInfo.Exception as RequestPersistException).Shortmessage;
			}
			return GenericShortMessage;
		}
	}
}