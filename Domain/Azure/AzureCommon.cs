using System;

namespace Teleopti.Ccc.Domain.Azure
{
	public static class AzureCommon
	{
		public static bool IsAzure => Environment.GetEnvironmentVariable("TeleoptiIsAzure")?.ToLower() == "true";
	}
}