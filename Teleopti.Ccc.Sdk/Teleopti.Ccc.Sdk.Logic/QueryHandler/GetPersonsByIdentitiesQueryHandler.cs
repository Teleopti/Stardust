using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonsByIdentitiesQueryHandler : IHandleQuery<GetPersonsByIdentitiesQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ITenantLogonDataManager _tenantLogonDataManager;

		public GetPersonsByIdentitiesQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ITenantLogonDataManager tenantLogonDataManager)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_tenantLogonDataManager = tenantLogonDataManager;
		}

		public ICollection<PersonDto> Handle(GetPersonsByIdentitiesQueryDto query)
		{
			query.Identities.VerifyCountLessThan(50, "A maximum of 50 persons is allowed to search for. You tried to load persons for {0}.");
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var memberList = new List<IPerson>();
					var logonInfos = _tenantLogonDataManager.GetLogonInfoForIdentities(query.Identities).ToList();
					if (!logonInfos.Any())
						return new PersonDto[] { };
					var foundPersons = _personRepository.FindPeople(logonInfos.Select(li => li.PersonId));
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}