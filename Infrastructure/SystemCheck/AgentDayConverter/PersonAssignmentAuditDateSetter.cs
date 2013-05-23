namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentAuditDateSetter : PersonAssignmentDateSetterBase
	{
		protected override string NumberOfNotConvertedCommand
		{
			get { return "select COUNT(*) as cnt from [Auditing].PersonAssignment_AUD " +
									 "where [Date]=@baseDate " +
			             "and Person=@personId"; }
		}

		protected override string ReadUnconvertedSchedulesCommand
		{
			get
			{
				return "select pa.Id, pa.Minimum, pa.Date " +
				       "from [Auditing].PersonAssignment_AUD pa " +
				       "inner join Person p on pa.Person = p.id " +
							 "where pa.[Date]=@baseDate " +
				       "and p.Id=@personId";
			}
		}

		protected override string UpdateAssignmentDateCommand
		{
			get
			{
				return "update [Auditing].PersonAssignment_AUD " +
								"set [Date]=@newDate, version=version+1 " +
								"where Id=@id";
			}
		}
	}
}