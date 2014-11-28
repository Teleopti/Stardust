using System;
using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.MessageBroker
{
	[TestFixture]
	public class UserDataFactoryTest
	{
		private ICurrentBusinessUnit buProvider;
		private IDataSource dataSource;
		private IUserDataFactory target;
		private IConfigReader configReader;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			buProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			dataSource = MockRepository.GenerateMock<IDataSource>();
			configReader = MockRepository.GenerateMock<IConfigReader>();
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
			var result = target.CreateViewModel(null);
			result.BusinessUnitId.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetDataSourceName()
		{
			const string expected = "stoj och lek";
			dataSource.Expect(mock => mock.DataSourceName).Return(expected);

			var result = target.CreateViewModel(null);
			result.DataSourceName.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetMessageBrokerUrl()
		{
			const string expected = "http://donkeyXXX.com";
			var nameValue = new NameValueCollection();
			nameValue.Add(UserDataFactory.MessageBrokerUrlKey, expected);
			configReader.Expect(mock => mock.AppSettings).Return(nameValue);

			var result = target.CreateViewModel(null);
			result.Url.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetMessageBrokerUrlFromContextWhenSetToReplace()
		{
			const string configured = "http://-replace-";
			var nameValue = new NameValueCollection();
			nameValue.Add(UserDataFactory.MessageBrokerUrlKey, configured);
			nameValue.Add("UseRelativeConfiguration", "true");
			configReader.Expect(mock => mock.AppSettings).Return(nameValue);

			var request = new FakeHttpRequest("/asdf", new Uri("http://asdf/asdf/"),new Uri("http://asdf"));
			var result = target.CreateViewModel(request);
			result.Url.Should().Be.EqualTo("http://asdf/");
		}

		[Test]
		public void ShouldSetAgentId()
		{
			var expected = Guid.NewGuid();
			var person = new Person();
			person.SetId(expected);
			loggedOnUser.Expect(mock => mock.CurrentUser()).Return(person);

			var result = target.CreateViewModel(null);
			result.AgentId.Should().Be.EqualTo(expected);
		}
	}
}