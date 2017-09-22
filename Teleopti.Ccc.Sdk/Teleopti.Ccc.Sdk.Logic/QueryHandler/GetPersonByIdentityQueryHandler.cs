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

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonByIdentityQueryHandler : IHandleQuery<GetPersonByIdentityQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ITenantLogonDataManager _tenantLogonDataManager;

		public GetPersonByIdentityQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ITenantLogonDataManager tenantLogonDataManager)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_tenantLogonDataManager = tenantLogonDataManager;
		}

		public ICollection<PersonDto> Handle(GetPersonByIdentityQueryDto query)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var memberList = new List<IPerson>();
					var logonInfo = _tenantLogonDataManager.GetLogonInfoForIdentity(query.Identity);
					if (logonInfo == null)
						return new PersonDto[] { };
					var foundPersons = _personRepository.FindPeople(new[] { logonInfo.PersonId });
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}