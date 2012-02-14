using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core
{
	public interface IErrorMessageProvider
	{
		string ResolveMessage(HandleErrorInfo handleErrorInfo);
		string ResolveShortMessage(HandleErrorInfo handleErrorInfo);
	}
}