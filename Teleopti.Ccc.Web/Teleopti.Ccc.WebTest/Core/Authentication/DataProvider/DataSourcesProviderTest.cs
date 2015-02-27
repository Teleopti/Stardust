using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class DataSourcesProviderTest
	{
		private IApplicationData applicationData;
		private IDataSourcesProvider target;

		[SetUp]
		public void Setup()
		{
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			target = new DataSourcesProvider(applicationData);
		}

		[Test]
		public void CanFindDataSourceByName()
		{
			var hit = new testDataSource("gnaget!");

			applicationData.Stub(x => x.DataSource("gnaget!")).Return(hit);

			target.RetrieveDataSourceByName("gnaget!").Should().Be.SameInstanceAs(hit);
		}

		[Test]
		public void DataSourceShouldReturnNullIfNonExisting()
		{
			target.RetrieveDataSourceByName("gnaget").Should().Be.Null();
		}

		private class testDataSource : IDataSource
		{
			private readonly string _name;

			public testDataSource(string name)
			{
				_name = name;
			}

			public IUnitOfWorkFactory Statistic
			{
				get { throw new NotImplementedException(); }
			}

			public IReadModelUnitOfWorkFactory ReadModel
			{
				get { throw new NotImplementedException(); }
			}

			public IUnitOfWorkFactory Application
			{
				get { throw new NotImplementedException(); }
			}

			public string DataSourceName
			{
				get { return _name; }
			}

			public void ResetStatistic()
			{
				throw new NotImplementedException();
			}
			public string OriginalFileName { get; set; }
			public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

			public void Dispose()
			{
			}
		}
	}
}