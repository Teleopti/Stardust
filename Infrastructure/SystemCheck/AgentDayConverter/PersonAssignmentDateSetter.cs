namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentDateSetter : PersonAssignmentDateSetterBase
	{
		protected override string NumberOfNotConvertedCommand
		{
			get
			{
				return "select COUNT(*) as cnt from dbo.PersonAssignment pa " +
						"inner join ShiftLayer sl on pa.Id = sl.Parent " +
							 "where pa.[Date]=@baseDate " +
				       "and pa.Person=@personId";
			}
		}

		protected override string ReadUnconvertedSchedulesCommand
		{
			get
			{
				return "select pa.Id, pa.Date, min(l.Minimum) as minimum " +
				       "from dbo.PersonAssignment pa " +
				       "inner join Person p on pa.Person = p.id " +
							 "inner join ShiftLayer l on l.Parent = pa.Id " + 
							 "where pa.[Date]=@baseDate " +
				       "and p.Id=@personId " +
				       "group by pa.Id, pa.Date";
			}
		}

		protected override string UpdateAssignmentDateCommand
		{
			get
			{
				return "update dbo.PersonAssignment " +
							 "set [Date]=@newDate, Version=Version+1 " +
				       "where Id=@id";
			}
		}
	}
}