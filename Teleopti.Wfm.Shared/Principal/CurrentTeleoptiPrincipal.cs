namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class CurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private readonly IThreadPrincipalContext _threadPrincipalContext;

		public static CurrentTeleoptiPrincipal Make()
		{
			return new CurrentTeleoptiPrincipal(new ThreadPrincipalContext());
		}

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