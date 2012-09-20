using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
    public class PushMessageProvider : IPushMessageProvider
	{

        public PushMessageProvider(IPrincipalAuthorization principalAuthorization)
		{

		}

        public int UnreadMessageCount
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
	}
}