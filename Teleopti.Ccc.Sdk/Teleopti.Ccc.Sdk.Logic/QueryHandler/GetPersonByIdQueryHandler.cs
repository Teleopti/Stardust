﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPersonByIdQueryHandler : IHandleQuery<GetPersonByIdQueryDto, ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _assembler;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetPersonByIdQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PersonDto> Handle(GetPersonByIdQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var foundPerson = _personRepository.Get(query.PersonId);
                    if (foundPerson == null) return new List<PersonDto>();
                    return new []{_assembler.DomainEntityToDto(foundPerson)};
                }
            }
        }
    }
}
