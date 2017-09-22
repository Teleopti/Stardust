using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class ExternalLogOnAssembler : Assembler<IExternalLogOn, ExternalLogOnDto>
	{
		public override ExternalLogOnDto DomainEntityToDto(IExternalLogOn entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			var externalLogOnDto = new ExternalLogOnDto
			{
				AcdLogOnOriginalId = entity.AcdLogOnOriginalId,
				AcdLogOnName = entity.AcdLogOnName,
				DataSourceId = entity.DataSourceId,
				Id = entity.Id
			};

			return externalLogOnDto;
		}

		public override IExternalLogOn DtoToDomainEntity(ExternalLogOnDto dto)
		{
			throw new NotImplementedException();
		}
	}
}
