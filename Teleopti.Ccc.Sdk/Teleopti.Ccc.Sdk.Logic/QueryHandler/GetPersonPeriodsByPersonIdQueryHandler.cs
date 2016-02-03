using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonPeriodsByPersonIdQueryHandler : IHandleQuery<GetPersonPeriodsByPersonIdQueryDto, ICollection<PersonPeriodDetailDto>>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAssembler<IPersonPeriod, PersonPeriodDetailDto> _personPeriodAssembler;

		public GetPersonPeriodsByPersonIdQueryHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IAssembler<IPersonPeriod, PersonPeriodDetailDto> personPeriodAssembler)
		{
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_personPeriodAssembler = personPeriodAssembler;
		}

		public ICollection<PersonPeriodDetailDto> Handle(GetPersonPeriodsByPersonIdQueryDto query)
		{
			verifyNotTooMuchPeople(query.PersonIdCollection);
			IList<PersonPeriodDetailDto> personPeriodDto2List;

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var persons = _personRepository.FindPeople(query.PersonIdCollection);

					var dateOnlyPeriod = query.Range.ToDateOnlyPeriod();
					var personPeriods =
						persons.Where(
							p =>
								PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, p))
							.SelectMany(person => person.PersonPeriods(dateOnlyPeriod))
							.ToList();

					personPeriodDto2List = _personPeriodAssembler.DomainEntitiesToDtos(personPeriods).ToList();
				}
			}

			return personPeriodDto2List;
		}

		private static void verifyNotTooMuchPeople(ICollection<Guid> people)
		{
			if (people.Count > 50)
			{
				throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "A maximum of 50 persons is allowed. You tried to load person periods for {0}.", people.Count));
			}
		}
	}
}