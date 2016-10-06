using System;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public class FakeIntradayRequestProcessor : IIntradayRequestProcessor
	{
		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			
		}
	}
}