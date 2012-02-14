namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IRunSql
	{
		ISqlQuery Create(string sqlCommand);
	}
}