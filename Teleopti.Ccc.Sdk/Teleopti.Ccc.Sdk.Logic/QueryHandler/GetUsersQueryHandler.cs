using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetUsersQueryHandler : IHandleQuery<GetUsersQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentAuthorization _currentAuthorization;

		public GetUsersQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentAuthorization currentAuthorization)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentAuthorization = currentAuthorization;
		}
		
		public ICollection<PersonDto> Handle(GetUsersQueryDto query)
		{
			if (!_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage))
			{
				throw new FaultException(new FaultReason("The current user is not permitted to perform this operation."));
			}
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.LoadDeletedIfSpecified(query.LoadDeleted))
				{
					var memberList = new List<IPerson>();
					var foundPersons = _personRepository.FindUsers(query.Date==null ? DateOnly.Today : query.Date.ToDateOnly());
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}