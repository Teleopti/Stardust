using System;
using System.Threading;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class ExternalLogOnAssembler : Assembler<IExternalLogOn, ExternalLogOnDto>
	{
		public override ExternalLogOnDto DomainEntityToDto(IExternalLogOn entity)
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			var externalLogOnDto = new ExternalLogOnDto{AcdLogOnOriginalId = entity.AcdLogOnOriginalId, AcdLogOnName = entity.AcdLogOnName};

			return externalLogOnDto;
		}

		public override IExternalLogOn DtoToDomainEntity(ExternalLogOnDto dto)
		{
			throw new NotImplementedException();
		}
	}
}
