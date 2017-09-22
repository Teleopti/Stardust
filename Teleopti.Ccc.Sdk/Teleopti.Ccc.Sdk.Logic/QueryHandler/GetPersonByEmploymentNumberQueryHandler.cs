using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonByEmploymentNumberQueryHandler : IHandleQuery<GetPersonByEmploymentNumberQueryDto,ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _assembler;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetPersonByEmploymentNumberQueryHandler(IAssembler<IPerson, PersonDto> assembler, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PersonDto> Handle(GetPersonByEmploymentNumberQueryDto query)
        {
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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
