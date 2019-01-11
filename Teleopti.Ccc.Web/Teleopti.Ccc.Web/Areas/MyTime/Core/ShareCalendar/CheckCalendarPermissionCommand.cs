using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class CheckCalendarPermissionCommand : ICheckCalendarPermissionCommand
    {
        private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
        private readonly IPrincipalAuthorizationFactory _principalAuthorizationFactory;

        public CheckCalendarPermissionCommand(IRoleToPrincipalCommand roleToPrincipalCommand, IPrincipalAuthorizationFactory principalAuthorizationFactory)
        {
            _roleToPrincipalCommand = roleToPrincipalCommand;
            _principalAuthorizationFactory = principalAuthorizationFactory;
        }

        public void Execute(IDataSource dataSource, IPerson person, IPersonRepository personRepository)
        {
            var principal = new ClaimsOwner(person);
            _roleToPrincipalCommand.Execute(new SingleOwnedPerson(person), principal, personRepository, dataSource.Application.Name);
            var permission = _principalAuthorizationFactory.FromClaimsOwner(principal);

            if (!permission.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
                throw new PermissionException("No permission for calendar sharing");
        }
    }
}