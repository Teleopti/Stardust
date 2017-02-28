using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IIntradayRequestProcessor
	{
		void Process(IPersonRequest personRequest, DateTime startDate);
	}
}