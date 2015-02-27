using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
	public class AvailableDataSourcesProvider : IAvailableDataSourcesProvider
	{
		private readonly ICurrentApplicationData _applicationData;
		private bool _initialized;
		private readonly IList<IDataSource> _availableDataSources = new List<IDataSource>();
		private readonly IList<IDataSource> _unavailableDataSources = new List<IDataSource>();
		private readonly object _initializeLock = new object();

		public AvailableDataSourcesProvider(ICurrentApplicationData applicationData)
		{
			_applicationData = applicationData;
		}

		public IEnumerable<IDataSource> AvailableDataSources()
		{
			verifyInitialize();
			return _availableDataSources;
		}

		private void verifyInitialize()
		{
			lock (_initializeLock)
			{
				if (!_initialized)
				{
					var field = typeof(ApplicationData).GetField("_registeredDataSourceCollection", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
					if (field != null)
					{
						var dataSources = (IEnumerable<IDataSource>) field.GetValue(_applicationData.Current());

						foreach (IDataSource dataSource in dataSources)
						{
							try
							{
								using (dataSource.Application.CreateAndOpenUnitOfWork())
								{
									//Must probably do something to get a connection opened
								}
								_availableDataSources.Add(dataSource);
							}
							catch (Exception ex)
							{
								var sqlEx = ex.InnerException as SqlException;
								if (sqlEx != null &&
								    (sqlEx.Number == 4060 || sqlEx.Number == 53 || sqlEx.Number == -1 || sqlEx.Number == -2))
									_unavailableDataSources.Add(dataSource);
								else
									throw;
							}
						}
					}
					_initialized = true;
					
				}
			}
		}

		public IEnumerable<IDataSource> UnavailableDataSources()
		{
			verifyInitialize();
			return _unavailableDataSources;
		}
	}
}