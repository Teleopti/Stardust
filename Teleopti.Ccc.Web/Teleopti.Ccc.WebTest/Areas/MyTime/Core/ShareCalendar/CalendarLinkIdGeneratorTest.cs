using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.ShareCalendar
{
	[TestFixture]
	public class CalendarLinkIdGeneratorTest
	{
		[Test]
		public void ShouldGenerate()
		{
			var currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			currentDataSource.Stub(x => x.CurrentName()).Return("test");
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			var personId = Guid.NewGuid();
			person.SetId(personId);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new CalendarLinkIdGenerator(currentDataSource, loggedOnUser);
			var id = target.Generate();

			id.Should().Be.EqualTo(HttpServerUtility.UrlTokenEncode(Encryption.EncryptStringToBytes("test/" + personId, EncryptionConstants.Image1,
			                                                        EncryptionConstants.Image2)));
		}

		[Test]
		public void ShouldParse()
		{
			var personId = Guid.NewGuid();
			var id = HttpServerUtility.UrlTokenEncode(Encryption.EncryptStringToBytes("test/" + personId, EncryptionConstants.Image1,
			                                          EncryptionConstants.Image2));

			var target = new CalendarLinkIdGenerator(null, null);
			var result = target.Parse(id);
			result.DataSourceName.Should().Be.EqualTo("test");
			result.PersonId.Should().Be.EqualTo(personId);
		}
	}
}