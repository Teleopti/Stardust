using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPersonByEmploymentNumberQueryHandler : IHandleQuery<GetPersonByEmploymentNumberQueryDto,ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _assembler;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetPersonByEmploymentNumberQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<PersonDto> Handle(GetPersonByEmploymentNumberQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    var memberList = new List<IPerson>();
                    var foundPersons =
                        _personRepository.FindPeopleByEmploymentNumber(query.EmploymentNumber);
                    memberList.AddRange(foundPersons);
                    return _assembler.DomainEntitiesToDtos(memberList).ToList();
                }
            }
        }
    }
}
