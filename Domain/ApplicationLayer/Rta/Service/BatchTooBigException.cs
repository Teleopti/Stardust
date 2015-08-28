using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class BatchTooBigException : Exception
	{
		public BatchTooBigException(string message) : base(message)
		{
		}
	}
}