using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels
{
	public class PermissionsViewModelBuilder
	{
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;

		public PermissionsViewModelBuilder(ICurrentAuthorization authorization, IPersonRepository persons)
		{
			_authorization = authorization;
			_persons = persons;
		}

		public PermissionsViewModel Build(Guid? personId, DateTime? date)
		{
			IPerson person = null;
			if (personId != null)
				person = _persons.Load(personId.Value);
			return new PermissionsViewModel
			{
				HistoricalOverview = isPermitted(DefinedRaptorApplicationFunctionPaths.HistoricalOverview, date, person),
				ModifyAdherence = isPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, person)
			};
		}

		private bool isPermitted(string permission, DateTime? date, IPerson person)
		{
			if (date == null)
				return _authorization.Current().IsPermitted(permission);
			return _authorization.Current().IsPermitted(permission, date.Value.Date, person);
		}

		public class PermissionsViewModel
		{
			public bool HistoricalOverview;
			public bool ModifyAdherence;
		}
	}
}