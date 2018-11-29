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

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetUsersQueryHandler : IHandleQuery<GetUsersQueryDto, ICollection<PersonDto>>
	{
		private readonly PersonCredentialsAppender _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentAuthorization _currentAuthorization;

		public GetUsersQueryHandler(PersonCredentialsAppender assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentAuthorization currentAuthorization)
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
					var foundPersons = _personRepository.FindUsers(query.Date?.ToDateOnly() ?? DateOnly.Today);
					return _assembler.Convert(foundPersons.ToArray()).ToList();
				}
			}
		}
	}
}