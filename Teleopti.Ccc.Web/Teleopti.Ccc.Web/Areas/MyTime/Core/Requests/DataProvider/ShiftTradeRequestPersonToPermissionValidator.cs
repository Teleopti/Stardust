using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersonToPermissionValidator : IShiftTradeRequestPersonToPermissionValidator
	{
		private readonly IPrincipalFactory _principalFactory;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IPersonRepository _personRepository;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPrincipalAuthorizationFactory _principalAuthorizationFactory;

		public ShiftTradeRequestPersonToPermissionValidator(IPrincipalFactory principalFactory,
			ICurrentDataSource currentDataSource,
			IPersonRepository personRepository,
			IRoleToPrincipalCommand roleToPrincipalCommand,
			IPrincipalAuthorizationFactory principalAuthorizationFactory)
		{
			_principalFactory = principalFactory;
			_currentDataSource = currentDataSource;
			_personRepository = personRepository;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_principalAuthorizationFactory = principalAuthorizationFactory;
		}

		public bool IsSatisfied(IShiftTradeRequest shiftTradeRequest)
		{
			var principal = _principalFactory.MakePrincipal(shiftTradeRequest.PersonTo, _currentDataSource.Current(), null, null);
			_roleToPrincipalCommand.Execute(principal, _currentDataSource.Current().Application, _personRepository);
			var authorisation = _principalAuthorizationFactory.FromPrincipal(principal);
			var permissionProvider = new PermissionProvider(authorisation);
			var checkDate = new DateOnly(shiftTradeRequest.Period.StartDateTime);

			var teamFrom = shiftTradeRequest.PersonFrom.MyTeam(checkDate);
			var teamTo = shiftTradeRequest.PersonTo.MyTeam(checkDate);
			var site = teamTo?.Site;
			var businessUnit = site?.BusinessUnit;

			var authorizeOrganisationDetail = new PersonSelectorShiftTrade
			{
				PersonId = shiftTradeRequest.PersonTo.Id ?? Guid.Empty,
				TeamId = teamTo?.Id,
				SiteId = site?.Id,
				BusinessUnitId = businessUnit?.Id ?? Guid.Empty
			};

			var canViewSchedules =
				permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, checkDate,
					authorizeOrganisationDetail);
			var canViewUnpublishedSchedules =
				permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var canPerformShiftTradeRequest =
				permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, checkDate,
					teamFrom);

			return (canViewSchedules || canViewUnpublishedSchedules) && canPerformShiftTradeRequest;
		}
	}
}