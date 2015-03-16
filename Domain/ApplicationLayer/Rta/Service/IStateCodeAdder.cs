using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateCodeAdder
	{
		void AddUnknownStateCode(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription);
	}
}