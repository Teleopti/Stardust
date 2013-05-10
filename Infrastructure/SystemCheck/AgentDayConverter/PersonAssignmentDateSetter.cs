﻿namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentDateSetter : PersonAssignmentDateSetterBase
	{
		protected override string NumberOfNotConvertedCommand
		{
			get
			{
				return "select COUNT(*) as cnt from dbo.PersonAssignment " +
							 "where TheDate = '" + AgentDayConverters.DateOfUnconvertedSchedule + "' " +
				       "and Person=@personId";
			}
		}

		protected override string ReadUnconvertedSchedulesCommand
		{
			get
			{
				return "select pa.Id, pa.Minimum, pa.TheDate " +
				       "from dbo.PersonAssignment pa " +
				       "inner join Person p on pa.Person = p.id " +
							 "where pa.TheDate = '" + AgentDayConverters.DateOfUnconvertedSchedule + "' " +
				       "and p.Id=@personId";
			}
		}

		protected override string UpdateAssignmentDateCommand
		{
			get
			{
				return "update dbo.PersonAssignment " +
				       "set TheDate = @newDate, Version=Version+1 " +
				       "where Id=@id";
			}
		}
	}
}