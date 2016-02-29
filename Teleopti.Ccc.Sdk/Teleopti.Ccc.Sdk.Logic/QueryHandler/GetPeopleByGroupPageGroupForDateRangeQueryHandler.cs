using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPeopleByGroupPageGroupForDateRangeQueryHandler : IHandleQuery<GetPeopleByGroupPageGroupForDateRangeQueryDto, ICollection<PersonDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPeopleByGroupPageGroupForDateRangeQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository,IPersonRepository personRepository,IAssembler<IPerson,PersonDto> personAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_personRepository = personRepository;
			_personAssembler = personAssembler;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<PersonDto> Handle(GetPeopleByGroupPageGroupForDateRangeQueryDto query)
		{
			var queryRange = query.QueryRange.ToDateOnlyPeriod();
			var days = queryRange.DayCollection();
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryRange);

				var principalAuthorization = PrincipalAuthorization.Instance();
				var availableDetails = details.Where(
					p => days.Any(date => principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
						date, p)));

				var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
				return _personAssembler.DomainEntitiesToDtos(people).ToList();
			}
		}
	}
}