using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentAuditDateSetter : PersonAssignmentDateSetterBase
	{
		public PersonAssignmentAuditDateSetter(SqlTransaction transaction)
			: base(transaction)
		{
		}

		protected override string NumberOfNotConvertedCommand
		{
			get { return "select COUNT(*) as cnt from [Auditing].PersonAssignment_AUD " +
			             "where TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "' " +
			             "and Person=@personId"; }
		}

		protected override string ReadUnconvertedSchedulesCommand
		{
			get
			{
				return "select pa.Id, pa.Minimum, pa.TheDate, pa.Version " +
				       "from [Auditing].PersonAssignment_AUD pa " +
				       "inner join Person p on pa.Person = p.id " +
				       "where pa.TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "' " +
				       "and p.Id=@personId";
			}
		}

		protected override string UpdateAssignmentDateCommand
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