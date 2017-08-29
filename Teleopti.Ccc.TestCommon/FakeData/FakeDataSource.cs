using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public sealed class FakeDataSource : IDataSource
	{
		public IUnitOfWorkFactory Application { get; set; }
		public IAnalyticsUnitOfWorkFactory Analytics { get; set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }
		public string DataSourceName { get; set; }

		public FakeDataSource()
		{
		}

		public FakeDataSource(string name)
		{
			DataSourceName = name;
		}

		public void RemoveAnalytics()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}

	public class DecoratorDataSource : IDataSource
	{
		private readonly IDataSource _dataSourceImplementation;
		private readonly string _dataSourceName;

		public DecoratorDataSource(IDataSource dataSourceImplementation, string newDataSourceName)
		{
			_dataSourceImplementation = dataSourceImplementation;
			_dataSourceName = newDataSourceName;
		}
		public void Dispose()
		{
			_dataSourceImplementation.Dispose();
		}

		public IUnitOfWorkFactory Application
		{
			get { return _dataSourceImplementation.Application; }
		}

		public IAnalyticsUnitOfWorkFactory Analytics
		{
			get { return _dataSourceImplementation.Analytics; }
		}

		public IReadModelUnitOfWorkFactory ReadModel
		{
			get { return _dataSourceImplementation.ReadModel; }
		}

		public string DataSourceName
		{
			get { return _dataSourceName; }
		}

		public void RemoveAnalytics()
		{
			_dataSourceImplementation.RemoveAnalytics();
		}
	}
}