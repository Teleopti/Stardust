using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersonToPermissionValidator : IShiftTradeRequestPersonToPermissionValidator
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPrincipalAuthorizationFactory _authorizationFactory;

		public ShiftTradeRequestPersonToPermissionValidator(
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IPersonRepository personRepository,
			IRoleToPrincipalCommand roleToPrincipalCommand, 
			IPrincipalAuthorizationFactory authorizationFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRepository = personRepository;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_authorizationFactory = authorizationFactory;
		}

		public bool IsSatisfied(IShiftTradeRequest shiftTradeRequest)
		{
			var principal = new ClaimsOwner(shiftTradeRequest.PersonTo);
			_roleToPrincipalCommand.Execute(new SingleOwnedPerson(shiftTradeRequest.PersonTo), principal, _personRepository, _currentUnitOfWorkFactory.Current().Name);
			var authorisation = _authorizationFactory.FromClaimsOwner(principal);
			var permissionProvider = new PermissionProvider(authorisation);
			var checkDate = new DateOnly(shiftTradeRequest.Period.StartDateTime);

			var teamFrom = shiftTradeRequest.PersonFrom.MyTeam(checkDate);
			var teamTo = shiftTradeRequest.PersonTo.MyTeam(checkDate);
			var site = teamTo?.Site;
			var businessUnit = site.GetOrFillWithBusinessUnit_DONTUSE();

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