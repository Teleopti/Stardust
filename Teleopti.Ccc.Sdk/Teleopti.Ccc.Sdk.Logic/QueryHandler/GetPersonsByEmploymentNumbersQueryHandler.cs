using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetPersonsByEmploymentNumbersQueryHandler :
		IHandleQuery<GetPersonsByEmploymentNumbersQueryDto, ICollection<PersonDto>>
	{
		private readonly PersonCredentialsAppender _credentialsAppender;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetPersonsByEmploymentNumbersQueryHandler(PersonCredentialsAppender credentialsAppender, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_credentialsAppender = credentialsAppender;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PersonDto> Handle(GetPersonsByEmploymentNumbersQueryDto query)
		{
			query.EmploymentNumbers.VerifyCountLessThan(50, "A maximum of 50 persons is allowed. You tried to load persons for {0}.");
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var memberList = _personRepository.FindPeopleByEmploymentNumbers(query.EmploymentNumbers).ToArray();
					return _credentialsAppender.Convert(memberList).ToList();
				}
			}
		}
	}
}