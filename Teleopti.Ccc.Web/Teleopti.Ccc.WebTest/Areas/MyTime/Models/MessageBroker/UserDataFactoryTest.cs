using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.MessageBroker
{
	[TestFixture]
	public class UserDataFactoryTest
	{
		private ICurrentBusinessUnitProvider buProvider;
		private IDataSource dataSource;
		private IUserDataFactory target;

		[SetUp]
		public void Setup()
		{
			buProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			dataSource = MockRepository.GenerateMock<IDataSource>();
			target = new UserDataFactory(buProvider, dataSource);			
		}

		[Test]
		public void ShouldSetBusinessUnitId()
		{
			var expected = Guid.NewGuid();
			var bu = new BusinessUnit("dd");
			bu.SetId(expected);
			buProvider.Expect(mock => mock.CurrentBusinessUnit()).Return(bu);
			var result = target.CreateViewModel();
			result.BusinessUnitId.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetDataSourceName()
		{
			var expected = "stoj och lek";

			dataSource.Expect(mock => mock.DataSourceName).Return(expected);

			var result = target.CreateViewModel();
			result.DataSourceName.Should().Be.EqualTo(expected);
		}
	}
}