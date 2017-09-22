using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.MessageBroker
{
	[TestFixture]
	public class UserDataFactoryTest
	{
		private ICurrentBusinessUnit buProvider;
		private IDataSource dataSource;
		private IUserDataFactory target;
		private FakeConfigReader configReader;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			buProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			dataSource = MockRepository.GenerateMock<IDataSource>();
			configReader = new FakeConfigReader();
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			target = new UserDataFactory(buProvider, () => dataSource, configReader, loggedOnUser);
		}

		[Test]
		public void ShouldSetBusinessUnitId()
		{
			var expected = Guid.NewGuid();
			var bu = new BusinessUnit("dd");
			bu.SetId(expected);
			buProvider.Expect(mock => mock.Current()).Return(bu);
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
			configReader.FakeSetting(UserDataFactory.MessageBrokerUrlKey, expected);

			var result = target.CreateViewModel();
			result.Url.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetMessageBrokerUrlFromContextWhenSetToReplace()
		{
			const string configured = "http://myserver/broker/signalr/";
			configReader.FakeSetting(UserDataFactory.MessageBrokerUrlKey, configured);
			configReader.FakeSetting("UseRelativeConfiguration", "true");

			var result = target.CreateViewModel();
			result.Url.Should().Be.EqualTo("/broker/signalr/");
		}

		[Test]
		public void ShouldSetAgentId()
		{
			var expected = Guid.NewGuid();
			var person = new Person();
			person.SetId(expected);
			loggedOnUser.Expect(mock => mock.CurrentUser()).Return(person);

			var result = target.CreateViewModel();
			result.AgentId.Should().Be.EqualTo(expected);
		}
	}
}