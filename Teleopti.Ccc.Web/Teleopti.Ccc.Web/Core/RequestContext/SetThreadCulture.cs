using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SetThreadCulture : ISetThreadCulture
	{
		public void SetCulture(IRegional regional)
		{
			if (regional.CultureLCID > 0)
				Thread.CurrentThread.CurrentCulture = regional.Culture;
			if (regional.UICultureLCID > 0)
				Thread.CurrentThread.CurrentUICulture = regional.UICulture;
		}
	}
}