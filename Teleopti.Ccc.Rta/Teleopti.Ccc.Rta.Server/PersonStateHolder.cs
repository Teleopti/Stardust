using System;

namespace Teleopti.Ccc.Rta.Server
{
	public class PersonStateHolder
	{
		public string StateCode { get; set; }
		public DateTime ReceivedTime { get; set; }

		public PersonStateHolder(string stateCode, DateTime timestamp)
		{
			StateCode = stateCode;
			ReceivedTime = timestamp;
		}

		public override bool Equals(object obj)
		{
			return obj != null && GetHashCode().Equals(obj.GetHashCode());
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = 0;
				result = (result * 397) ^ StateCode.GetHashCode();
				result = (result * 397) ^ ReceivedTime.GetHashCode();
				return result;
			}
		}
	}
}