using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonByIdentityQueryDtoHandler : IHandleQuery<GetPersonByIdentityQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IFindPersonInfoByIdentity _findPersonInfoByIdentity;

		public GetPersonByIdentityQueryDtoHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IFindPersonInfoByIdentity findPersonInfoByIdentity)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_findPersonInfoByIdentity = findPersonInfoByIdentity;
		}

		public ICollection<PersonDto> Handle(GetPersonByIdentityQueryDto query)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var memberList = new List<IPerson>();
					var personInfo = _findPersonInfoByIdentity.Find(query.Identity);
					var foundPersons = _personRepository.FindPeople(new[] { personInfo.Id });
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}