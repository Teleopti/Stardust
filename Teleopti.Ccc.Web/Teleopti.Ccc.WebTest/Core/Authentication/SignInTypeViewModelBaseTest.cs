using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.WebTest.Core.Authentication
{

	[TestFixture]
	public class SignInTypeViewModelBaseTest
	{

		[Test]
		public void ShouldReportNotHavingDataSourceOnNewViewModel()
		{
			var viewModel = new SignInTypeViewModelBaseStub();
			viewModel.HasDataSource.Should().Be.False();
		}

		[Test]
		public void ShouldReportNotHavingDataSourceOnEmptyEnumerable()
		{
			var viewModel = new SignInTypeViewModelBaseStub {DataSources = new DataSourceViewModel[] {}};
			viewModel.HasDataSource.Should().Be.False();
		}

		[Test]
		public void ShouldReportHavingDataSourceOnEnumerableWithElement()
		{
			var viewModel = new SignInTypeViewModelBaseStub {DataSources = new[] {new DataSourceViewModel()}};
			viewModel.HasDataSource.Should().Be.True();
		}

		private class SignInTypeViewModelBaseStub : SignInTypeViewModelBase<object>
		{
		}
	}
}
