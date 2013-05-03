using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public static class AgentDayConverters
	{
		public static readonly DateOnly DateOfUnconvertedSchedule = new DateOnly(1800, 1, 1);

		public static IEnumerable<IPersonAssignmentConverter> ForDbManager()
		{
			return new IPersonAssignmentConverter[]
				 {
					 new PersonAssignmentAuditDateSetter(),
					 new PersonAssignmentDateSetter()
				 };
		}

		public static IEnumerable<IPersonAssignmentConverter> ForPeople()
		{
			return new IPersonAssignmentConverter[]
				{
					new PersonTimeZoneSetter(),
					new PersonAssignmentAuditDateSetter(), 
					new PersonAssignmentDateSetter()
				};
		}
	}
}