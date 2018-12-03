using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

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
			query.PersonIdCollection.VerifyCountLessThan(50, "A maximum of 50 persons is allowed. You tried to load optional values for {0}.");
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var resultList = new List<PersonOptionalValuesDto>();
					var foundPeople = _personRepository.FindPeople(query.PersonIdCollection);
					foundPeople.VerifyCanBeModifiedByCurrentUser(DateOnly.Today);

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
	}
}