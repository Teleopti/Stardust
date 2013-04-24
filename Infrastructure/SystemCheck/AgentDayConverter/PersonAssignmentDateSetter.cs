using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentDateSetter : PersonAssignmentDateSetterBase
	{
		public PersonAssignmentDateSetter(SqlConnectionStringBuilder tempShouldNotBeLikeThis) : base(tempShouldNotBeLikeThis)
		{
		}

		protected override string NumberOfNotConvertedCommand
		{
			get { return "select COUNT(*) as cnt from dbo.PersonAssignment where TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "'"; }
		}

		protected override string ReadUnconvertedSchedulesCommand
		{
			get
			{
				return "select pa.Id, p.DefaultTimeZone, pa.Minimum, pa.TheDate, pa.Version " +
																	 "from dbo.PersonAssignment pa " +
																	 "inner join Person p on pa.Person = p.id " +
																	 "where pa.TheDate = '" + AgentDayConverterDate.DateOfUnconvertedSchedule + "'";
			}
		}

		protected override string UpdateAssignmentDateCommand
		{
			get
			{
				return "update dbo.PersonAssignment " +
				       "set TheDate = @newDate, Version = @newVersion " +
				       "where Id=@id";
			}
		}
	}
}