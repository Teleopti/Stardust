using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	public class GetPersonOptionalValuesByPersonIdQueryHandler : IHandleQuery<GetPersonOptionalValuesByPersonIdQueryDto, ICollection<PersonOptionalValuesDto>>
	{
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public GetPersonOptionalValuesByPersonIdQueryHandler(IOptionalColumnRepository optionalColumnRepository, IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_optionalColumnRepository = optionalColumnRepository;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<PersonOptionalValuesDto> Handle(GetPersonOptionalValuesByPersonIdQueryDto query)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var resultList = new List<PersonOptionalValuesDto>();
					var foundPerson = _personRepository.Get(query.PersonId);
					if (foundPerson == null) return resultList;

					var result = new PersonOptionalValuesDto{PersonId = query.PersonId};
					var foundColumns = _optionalColumnRepository.GetOptionalColumns<Person>();
					foreach (var optionalColumn in foundColumns)
					{
						var columnValue = foundPerson.GetColumnValue(optionalColumn);
						result.OptionalValueCollection.Add(new OptionalValueDto { Key = optionalColumn.Name, Value = columnValue == null ? string.Empty : columnValue.Description });
					}
					resultList.Add(result);
					return resultList;
				}
			}
		}
	}
}