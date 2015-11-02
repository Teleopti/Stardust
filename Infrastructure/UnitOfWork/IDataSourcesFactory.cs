using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IDataSourcesFactory
	{
		IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString);
		IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString);
		IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration);
	}
}