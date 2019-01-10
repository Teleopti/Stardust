using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class PermissionByPersonHandler : IQueryHandler<PermissionByPersonDto, ApplicationFunctionDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public PermissionByPersonHandler(IPersonRepository personRepository, IRoleToPrincipalCommand roleToPrincipalCommand, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRepository = personRepository;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		[UnitOfWork]
		public virtual QueryResultDto<ApplicationFunctionDto> Handle(PermissionByPersonDto query)
		{
			var person = _personRepository.Get(query.PersonId);
			var owner = new ClaimsOwner(person);
			_roleToPrincipalCommand.Execute(new SingleOwnedPerson(person), owner, _personRepository, _currentUnitOfWorkFactory.Current().Name);
			return new QueryResultDto<ApplicationFunctionDto>
			{
				Successful = true,
				Result = owner.ClaimSets.SelectMany(c => c).Select(c => c.Resource).OfType<IApplicationFunction>().Distinct()
					.Select(c => new ApplicationFunctionDto{ Id = c.Id.GetValueOrDefault(), FunctionCode = c.FunctionCode, FunctionPath = c.FunctionPath}).ToArray()
			};
		}
	}
}