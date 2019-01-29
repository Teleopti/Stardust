using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPeopleByGroupPageGroupForDateRangeQueryHandler : IHandleQuery<GetPeopleByGroupPageGroupForDateRangeQueryDto, ICollection<PersonDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPersonRepository _personRepository;
		private readonly PersonCredentialsAppender _personAssembler;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPeopleByGroupPageGroupForDateRangeQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository,IPersonRepository personRepository,PersonCredentialsAppender personAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory)
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

				var principalAuthorization = PrincipalAuthorization.Current_DONTUSE();
				var availableDetails = details.Where(
					p => days.Any(date => principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
						date, p)));

				var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
				return _personAssembler.Convert(people.ToArray()).ToList();
			}
		}
	}
}