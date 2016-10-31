using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class FindExternalLogOnQueryHandler : IHandleQuery<FindExternalLogOnQueryDto, ICollection<ExternalLogOnDto>>
	{
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public FindExternalLogOnQueryHandler(IExternalLogOnRepository externalLogOnRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_externalLogOnRepository = externalLogOnRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<ExternalLogOnDto> Handle(FindExternalLogOnQueryDto query)
		{
			query.ExternalLogOnCollection.VerifyCountLessThan(50, "A maximum of 50 external logons is allowed. You tried to search for {0}.");
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				// Naïve approach, loads everything from database, should filter on database level instead
				return _externalLogOnRepository.LoadAll()
					.Where(x => query.ExternalLogOnCollection.Contains(x.AcdLogOnName))
					.Select(x => new ExternalLogOnDto
					{
						Id = x.Id,
						AcdLogOnName = x.AcdLogOnName,
						AcdLogOnOriginalId = x.AcdLogOnOriginalId
					}).ToList();
			}
		}
	}
}