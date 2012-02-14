using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class WebState : State
	{
		public override void SetSessionData(ISessionData sessionData)
		{
			
		}

		public override ISessionData SessionScopeData
		{
			//just for now
			get { return null; }
		}

		public override void ClearSession()
		{
		}
	}
}