using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Wfm.Adherence
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
				ModifyAdherence = isPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, person),
				ModifySkillGroup = isPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup, date, person),
				AdjustAdherence = isPermitted(DefinedRaptorApplicationFunctionPaths.AdjustAdherence, date, person)
			};
		}

		private bool isPermitted(string permission, DateTime? date, IPerson person)
		{
			if (date == null)
				return _authorization.Current().IsPermitted(permission);
			return _authorization.Current().IsPermitted(permission, new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(date.Value.Date), person);
		}

		public class PermissionsViewModel
		{
			public bool HistoricalOverview;
			public bool ModifyAdherence;
			public bool ModifySkillGroup;
			public bool AdjustAdherence;
		}
	}
}