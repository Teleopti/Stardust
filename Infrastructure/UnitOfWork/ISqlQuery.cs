using System;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ISqlQuery
	{
		ISqlQuery SetString(string parameter, string value);
		ISqlQuery SetDateTime(string parameter, DateTime value);
		ISqlQuery SetGuid(string parameter, Guid value);

		void Execute();

	}
}