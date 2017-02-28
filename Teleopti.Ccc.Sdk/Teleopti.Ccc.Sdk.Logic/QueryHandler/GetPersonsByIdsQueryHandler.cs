﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonsByIdsQueryHandler : IHandleQuery<GetPersonsByIdsQueryDto, ICollection<PersonDto>>
	{
		private readonly IAssembler<IPerson, PersonDto> _assembler;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPersonsByIdsQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_assembler = assembler;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<PersonDto> Handle(GetPersonsByIdsQueryDto query)
		{
			query.PersonIds.VerifyCountLessThan(50, "A maximum of 50 persons is allowed to search for. You tried to load persons for {0}.");
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var memberList = new List<IPerson>();
					var foundPersons = _personRepository.FindPeopleSimplify(query.PersonIds);
					memberList.AddRange(foundPersons);
					return _assembler.DomainEntitiesToDtos(memberList).ToList();
				}
			}
		}
	}
}