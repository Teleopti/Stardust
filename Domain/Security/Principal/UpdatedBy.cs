using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UpdatedBy : IUpdatedBy, IUpdatedByScope
	{
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ThreadLocal<IPerson> _threadPerson = new ThreadLocal<IPerson>();

		public static IUpdatedBy Make()
		{
			return new UpdatedBy(CurrentTeleoptiPrincipal.Make());
		}

		public UpdatedBy(ICurrentTeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public IPerson Person()
		{
			if (_threadPerson.Value != null)
				return _threadPerson.Value;

			var principal = _principal.Current();
			return ((IUnsafePerson) principal)?.Person;
		}

		public void OnThisThreadUse(IPerson person)
		{
			_threadPerson.Value = person;
		}
	}
}