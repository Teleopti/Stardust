using System;
using System.Web;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public class CalendarLinkIdGenerator : ICalendarLinkIdGenerator
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ILoggedOnUser _loggedOnUser;

		public CalendarLinkIdGenerator(ICurrentDataSource currentDataSource, ILoggedOnUser loggedOnUser)
		{
			_currentDataSource = currentDataSource;
			_loggedOnUser = loggedOnUser;
		}

		private const string splitter = "/";

		public string Generate()
		{
			return HttpServerUtility.UrlTokenEncode(Encryption.EncryptStringToBytes(_currentDataSource.CurrentName() + splitter + _loggedOnUser.CurrentUser().Id.Value, EncryptionConstants.Image1,
			                                        EncryptionConstants.Image2));
		}

		public CalendarLinkId Parse(string encryptedId)
		{
			var dataSourceNameAndPersonId = Encryption.DecryptStringFromBytes(HttpServerUtility.UrlTokenDecode(encryptedId),
			                                                                  EncryptionConstants.Image1,
			                                                                  EncryptionConstants.Image2);
			var pos = dataSourceNameAndPersonId.LastIndexOf(splitter, StringComparison.Ordinal);
			var dataSourceName = dataSourceNameAndPersonId.Substring(0, pos);
			var personId = new Guid(dataSourceNameAndPersonId.Substring(pos + 1));
			return new CalendarLinkId
				{
					DataSourceName = dataSourceName,
					PersonId = personId
				};
		}
	}
}