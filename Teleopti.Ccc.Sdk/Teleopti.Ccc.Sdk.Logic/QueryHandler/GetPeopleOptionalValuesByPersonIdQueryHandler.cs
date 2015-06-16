using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPeopleOptionalValuesByPersonIdQueryHandler : IHandleQuery<GetPeopleOptionalValuesByPersonIdQueryDto, ICollection<PersonOptionalValuesDto>>
	{
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPeopleOptionalValuesByPersonIdQueryHandler(IOptionalColumnRepository optionalColumnRepository, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_optionalColumnRepository = optionalColumnRepository;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<PersonOptionalValuesDto> Handle(GetPeopleOptionalValuesByPersonIdQueryDto query)
		{
			verifyNotTooMuchPeople(query.PersonIdCollection);
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var resultList = new List<PersonOptionalValuesDto>();
					var foundPeople = _personRepository.FindPeople(query.PersonIdCollection);
					checkIfAuthorized(foundPeople,DateOnly.Today);

					var foundColumns = _optionalColumnRepository.GetOptionalColumns<Person>();

					foreach (var foundPerson in foundPeople)
					{
						var result = new PersonOptionalValuesDto { PersonId = foundPerson.Id.GetValueOrDefault() };
						foreach (var optionalColumn in foundColumns)
						{
							var columnValue = foundPerson.GetColumnValue(optionalColumn);
							result.OptionalValueCollection.Add(new OptionalValueDto { Key = optionalColumn.Name, Value = columnValue == null ? string.Empty : columnValue.Description });
						}
						resultList.Add(result);
					}
					return resultList;
				}
			}
		}

		private static void verifyNotTooMuchPeople(ICollection<Guid> people)
		{
			if (people.Count > 50)
			{
				throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "A maximum of 50 persons is allowed. You tried to load optional values for {0}.", people.Count));
			}
		}

		private static void checkIfAuthorized(IEnumerable<IPerson> people, DateOnly dateOnly)
		{
			var authorizationInstance = PrincipalAuthorization.Instance();
			foreach (var person in people)
			{
				if (!authorizationInstance.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
				{
					throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "You're not allowed to work with this person ({0}).", person.Name));
				}
			}
		}
	}
}