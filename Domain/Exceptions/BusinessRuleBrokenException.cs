using System;

namespace Teleopti.Ccc.Domain.Exceptions
{
	public class BusinessRuleBrokenException : Exception
	{
		public object ReturnObject { get; set; }
	}
}