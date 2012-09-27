using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public interface IPushMessageProvider
	{
	    int UnreadMessageCount { get; }
        IList<IPushMessageDialogue> GetMessages();
	}
}