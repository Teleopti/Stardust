using System;
using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.MessageBroker
{
	[TestFixture]
	public class UserDataFactoryTest
	{
		private ICurrentBusinessUnitProvider buProvider;
		private IDataSource dataSource;
		private IUserDataFactory target;
		private IConfigReader configReader;

		[SetUp]
		public void Setup()
		{
			buProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			dataSource = MockRepository.GenerateMock<IDataSource>();
			configReader = MockRepository.GenerateMock<IConfigReader>();
			target = new UserDataFactory(buProvider, dataSource, configReader);
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
			const string expected = "stoj och lek";
			dataSource.Expect(mock => mock.DataSourceName).Return(expected);

			var result = target.CreateViewModel();
			result.DataSourceName.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetMessageBrokerUrl()
		{
			const string expected = "http://donkeyXXX.com";
			var nameValue = new NameValueCollection();
			nameValue.Add(UserDataFactory.MessageBrokerUrlKey, expected);
			configReader.Expect(mock => mock.AppSettings).Return(nameValue);

			var result = target.CreateViewModel();
			result.Url.Should().Be.EqualTo(expected);
		}
	}
}