namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentAuditDateSetter : PersonAssignmentDateSetterBase
	{
		protected override string NumberOfNotConvertedCommand
		{
			get { return "select COUNT(*) as cnt from [Auditing].PersonAssignment_AUD where TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "'"; }
		}

		protected override string ReadCommand
		{
			get
			{
				return "select pa.Id, p.DefaultTimeZone, pa.Minimum, pa.TheDate, pa.Version " +
																	 "from [Auditing].PersonAssignment_AUD pa " +
																	 "inner join Person p on pa.Person = p.id " +
																	 "where pa.TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "'";
			}
		}

		protected override string UpdateAssignmentDate
		{
			get
			{
				return "update [Auditing].PersonAssignment_AUD " +
				       "set TheDate = @newDate, Version = @newVersion " +
				       "where Id=@id";
			}
		}
	}
}