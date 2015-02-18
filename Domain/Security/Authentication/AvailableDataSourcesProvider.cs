using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
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
			VerifyInitialize();
			return _availableDataSources;
		}

		private void VerifyInitialize()
		{
			lock (_initializeLock)
			{
				if (!_initialized)
				{
#pragma warning disable 618
					foreach (IDataSource dataSource in _applicationData.Current().RegisteredDataSourceCollection)
#pragma warning restore 618
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

						_initialized = true;
					}
				}
			}
		}

		public IEnumerable<IDataSource> UnavailableDataSources()
		{
			VerifyInitialize();
			return _unavailableDataSources;
		}
	}
}