using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public interface ILogOnHelper : IDisposable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IList<IBusinessUnit> GetBusinessUnitCollection();
		IList<DataSourceContainer> GetDataSourceCollection();
		IDataSourceContainer SelectedDataSourceContainer { get; }
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		bool SelectDataSourceContainer(string dataSourceName);
		void LogOff();
	}
}