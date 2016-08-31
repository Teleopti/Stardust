using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetUsersQueryHandler : IHandleQuery<GetUsersQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public GetUsersQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}
		
		public ICollection<PersonDto> Handle(GetUsersQueryDto query)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.LoadDeletedIfSpecified(query.LoadDeleted))
				{
					var memberList = new List<IPerson>();
					var foundPersons = _personRepository.FindUsers();
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}