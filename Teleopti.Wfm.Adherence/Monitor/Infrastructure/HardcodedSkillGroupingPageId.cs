using System;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class HardcodedSkillGroupingPageId
	{
		private static Guid id = new Guid("4CE00B41-0722-4B36-91DD-0A3B63C545CF");

		public static string Id => id.ToString();
		
		public string Get()
		{
			return id.ToString();
		}

		public Guid GetGuid()
		{
			return id;
		}
	}
}