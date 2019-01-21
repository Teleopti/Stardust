using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UpdatedBy : IUpdatedBy, IUpdatedByScope
	{
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly IPersonRepository _persons;
		private readonly ICurrentUnitOfWorkFactory _unitOfWork;
		private readonly ThreadLocal<IPerson> _threadPerson = new ThreadLocal<IPerson>();

		public static IUpdatedBy Make()
		{
			return new UpdatedBy(CurrentTeleoptiPrincipal.Make(), null, null);
		}

		public UpdatedBy(ICurrentTeleoptiPrincipal principal, IPersonRepository persons, ICurrentUnitOfWorkFactory unitOfWork)
		{
			_principal = principal;
			_persons = persons;
			_unitOfWork = unitOfWork;
		}

		public object Person()
		{
			if (_threadPerson.Value != null)
				return _threadPerson.Value;

			var principal = _principal.Current();
			if (principal == null)
				return null;

			if (principal is ITeleoptiPrincipalForLegacy principalWithPerson)
				return principalWithPerson.UnsafePersonObject();
			
			var hasUnitOfWork = _unitOfWork?.Current()?.HasCurrentUnitOfWork() ?? false;
			if (hasUnitOfWork && _persons != null)
				return _persons.Load(principal.PersonId);

			var personOnlyForUpdateBy = new Person();
			personOnlyForUpdateBy.SetId(principal.PersonId);
			return personOnlyForUpdateBy;
		}

		public void OnThisThreadUse(IPerson person)
		{
			_threadPerson.Value = person;
		}
	}
}