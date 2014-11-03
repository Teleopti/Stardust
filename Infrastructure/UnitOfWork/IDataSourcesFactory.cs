using System.Collections.Generic;
using System.Xml.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IDataSourcesFactory
	{
		IDataSource Create(string applicationDataSourceName, string applicationConnectionString, string statisticConnectionString);
		IDataSource Create(IDictionary<string, string> settings, string statisticConnectionString);
		bool TryCreate(XElement nhibernateConfiguration, out IDataSource dataSource);
	}
}