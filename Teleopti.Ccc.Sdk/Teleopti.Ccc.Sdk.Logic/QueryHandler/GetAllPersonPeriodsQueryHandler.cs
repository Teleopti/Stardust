using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
#pragma warning disable 618
	public class GetAllPersonPeriodsQueryHandler : IHandleQuery<GetAllPersonPeriodsQueryDto, ICollection<PersonPeriodDetailDto>>
#pragma warning restore 618
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAssembler<IPersonPeriod, PersonPeriodDetailDto> _personPeriodAssembler;
		private readonly ICurrentAuthorization _authorization;

		public GetAllPersonPeriodsQueryHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IAssembler<IPersonPeriod,PersonPeriodDetailDto> personPeriodAssembler, ICurrentAuthorization authorization)
		{
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_personPeriodAssembler = personPeriodAssembler;
			_authorization = authorization;
		}

#pragma warning disable 618
		public ICollection<PersonPeriodDetailDto> Handle(GetAllPersonPeriodsQueryDto query)
#pragma warning restore 618
		{
			IList<PersonPeriodDetailDto> personPeriodDto2List;

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var persons = _personRepository.FindAllSortByName();

					var dateOnlyPeriod = query.Range.ToDateOnlyPeriod();
					var personPeriods =
						persons.Where(
							p =>
								_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, p))
							.SelectMany(person => person.PersonPeriods(dateOnlyPeriod))
							.ToList();
					
					personPeriodDto2List = _personPeriodAssembler.DomainEntitiesToDtos(personPeriods).ToList();
				}
			}

			return personPeriodDto2List;
		}
	}
}