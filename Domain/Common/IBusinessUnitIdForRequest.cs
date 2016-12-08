using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IBusinessUnitIdForRequest
	{
		Guid? Get();
	}
}