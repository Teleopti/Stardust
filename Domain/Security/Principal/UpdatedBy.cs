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

		public IPerson Person()
		{
			if (_threadPerson.Value != null)
				return _threadPerson.Value;

			var principal = _principal.Current();
			if (principal == null)
				return null;

//			var person = (principal as ITeleoptiPrincipalForLegacy)?.UnsafePerson;
//			if (person != null)
//				return person;

			var hasUnitOfWork = _unitOfWork?.Current()?.HasCurrentUnitOfWork() ?? false;
			if (hasUnitOfWork && _persons != null)
				return _persons.Load(principal.PersonId);

//			if (principal.PersonId == Guid.Empty)
//			{
//				var systemUserOnlyForUpdatedBy = new Person();
//				systemUserOnlyForUpdatedBy.SetId(SystemUser.Id);
//				return systemUserOnlyForUpdatedBy;
//			}

			var personOnlyForUpdateBy = new Person();
			personOnlyForUpdateBy.SetId(principal.PersonId);
			personOnlyForUpdateBy.SetName(principal.PersonName);
			return personOnlyForUpdateBy;

			//return null;
		}

		public void OnThisThreadUse(IPerson person)
		{
			_threadPerson.Value = person;
		}
	}
}