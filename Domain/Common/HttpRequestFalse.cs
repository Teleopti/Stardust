using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class HttpRequestFalse : IIsHttpRequest
	{
		public bool IsHttpRequest()
		{
			return false;
		}

		public IBusinessUnit BusinessUnitForRequest()
		{
			throw new NotImplementedException();
		}
	}
}