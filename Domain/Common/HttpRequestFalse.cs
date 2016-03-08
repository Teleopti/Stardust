using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class HttpRequestFalse : IBusinessUnitForRequest
	{
		public bool IsHttpRequest()
		{
			return false;
		}

		public IBusinessUnit TryGetBusinessUnit()
		{
			return null;
		}
	}
}