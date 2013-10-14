using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPeopleByGroupPageGroupQueryHandler : IHandleQuery<GetPeopleByGroupPageGroupQueryDto, ICollection<PersonDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPeopleByGroupPageGroupQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository,IPersonRepository personRepository,IAssembler<IPerson,PersonDto> personAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_personRepository = personRepository;
			_personAssembler = personAssembler;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PersonDto> Handle(GetPeopleByGroupPageGroupQueryDto query)
		{
			var queryDate = query.QueryDate.ToDateOnly();
			var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

			var availableDetails = details.Where(
				p => PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
				                                                                  queryDate, p));

			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
				return _personAssembler.DomainEntitiesToDtos(people).ToList();
			}
		}
	}
}