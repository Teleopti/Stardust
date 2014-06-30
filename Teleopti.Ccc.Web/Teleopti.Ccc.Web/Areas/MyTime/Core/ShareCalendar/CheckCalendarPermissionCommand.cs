using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class CheckCalendarPermissionCommand : ICheckCalendarPermissionCommand
    {
        private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
        private readonly IPrincipalFactory _principalFactory;
        private readonly IPrincipalAuthorizationFactory _principalAuthorizationFactory;

        public CheckCalendarPermissionCommand(IRoleToPrincipalCommand roleToPrincipalCommand, IPrincipalFactory principalFactory, IPrincipalAuthorizationFactory principalAuthorizationFactory)
        {
            _roleToPrincipalCommand = roleToPrincipalCommand;
            _principalFactory = principalFactory;
            _principalAuthorizationFactory = principalAuthorizationFactory;
        }

        public void Execute(IDataSource dataSource, IPerson person, IPersonRepository personRepository)
        {
            var principal = _principalFactory.MakePrincipal(person, dataSource, null);
            _roleToPrincipalCommand.Execute(principal, dataSource.Application.CurrentUnitOfWork(), personRepository);
            var permission = _principalAuthorizationFactory.FromPrincipal(principal);

            if (!permission.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
                throw new PermissionException("No permission for calendar sharing");
        }
    }
}