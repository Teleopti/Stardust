using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class FindExternalLogOnQueryHandler : IHandleQuery<FindExternalLogOnQueryDto, ICollection<ExternalLogOnDto>>
	{
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAssembler<IExternalLogOn, ExternalLogOnDto> _externalLogOnAssembler;

		public FindExternalLogOnQueryHandler(IExternalLogOnRepository externalLogOnRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IAssembler<IExternalLogOn, ExternalLogOnDto> externalLogOnAssembler)
		{
			_externalLogOnRepository = externalLogOnRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_externalLogOnAssembler = externalLogOnAssembler;
		}

		public ICollection<ExternalLogOnDto> Handle(FindExternalLogOnQueryDto query)
		{
			query.ExternalLogOnCollection.VerifyCountLessThan(50, "A maximum of 50 external logons is allowed. You tried to search for {0}.");
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				return _externalLogOnAssembler.DomainEntitiesToDtos(_externalLogOnRepository.LoadByAcdLogOnNames(query.ExternalLogOnCollection)).ToList();
			}
		}
	}
}