using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPeopleByGroupPageGroupQueryHandler : IHandleQuery<GetPeopleByGroupPageGroupQueryDto, ICollection<PersonDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPersonRepository _personRepository;
		private readonly PersonCredentialsAppender _personAssembler;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPeopleByGroupPageGroupQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository,IPersonRepository personRepository,PersonCredentialsAppender personAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_personRepository = personRepository;
			_personAssembler = personAssembler;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<PersonDto> Handle(GetPeopleByGroupPageGroupQueryDto query)
		{
			var queryDate = query.QueryDate.ToDateOnly();
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

				var availableDetails = details.Where(
					p => PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
				                                                                  queryDate, p));

				var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
				return _personAssembler.Convert(people.ToArray()).ToList();
			}
		}
	}
}