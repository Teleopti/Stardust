using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IDataSourcesFactory
	{
		IDataSource Create(string applicationDataSourceName, string applicationConnectionString, string statisticConnectionString);
		IDataSource Create(IDictionary<string, string> settings, string statisticConnectionString);

		IDataSource Create(string tenantName, string applicationConnectionString, string analyticsConnectionString, IDictionary<string, string> applicationNhibConfiguration);
	}
}