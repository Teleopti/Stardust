using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationNewControllerSignInTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldLoadDataSources()
		{
			var dataSourceProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new AuthenticationNewController(null, dataSourceProvider);

			dataSourceProvider.Stub(x => x.RetrieveDatasourcesForWindows()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "windows" } });
			dataSourceProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "app" } });
			var result = target.LoadDataSources().Data as IEnumerable<DataSourceViewModel>;
			result.Count().Should().Be.EqualTo(2);
			result.First().Name.Should().Be.EqualTo("app");
			result.First().IsApplicationLogon.Should().Be.True();
			result.Last().Name.Should().Be.EqualTo("windows");
			result.Last().DisplayName.Should().Be.EqualTo("windows" + " " + Resources.WindowsLogonWithBrackets);
			result.Last().IsApplicationLogon.Should().Be.False();
		}

	}
}
