using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class RouteToGroupName
	{
		public static string Convert(string route)
		{
			//gethashcode won't work in 100% of the cases...
			UInt64 hashedValue = 3074457345618258791ul;
			for (int i = 0; i < route.Length; i++)
			{
				hashedValue += route[i];
				hashedValue *= 3074457345618258799ul;
			}

			return hashedValue.GetHashCode().ToString(CultureInfo.InvariantCulture);
		}
	}
}