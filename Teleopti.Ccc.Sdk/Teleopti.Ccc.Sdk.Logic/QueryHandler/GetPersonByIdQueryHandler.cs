using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonByIdQueryHandler : IHandleQuery<GetPersonByIdQueryDto, ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _assembler;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetPersonByIdQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PersonDto> Handle(GetPersonByIdQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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
