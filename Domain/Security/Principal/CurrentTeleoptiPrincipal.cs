using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class CurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private readonly IThreadPrincipalContext _threadPrincipalContext;

		public CurrentTeleoptiPrincipal(IThreadPrincipalContext threadPrincipalContext)
		{
			_threadPrincipalContext = threadPrincipalContext;
		}

		public ITeleoptiPrincipal Current()
		{
			return _threadPrincipalContext.Current();
		}
	}
}