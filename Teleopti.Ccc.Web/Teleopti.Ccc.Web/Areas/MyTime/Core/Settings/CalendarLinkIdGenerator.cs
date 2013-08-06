using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings
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

		private const string spilter = "/";

		public string Generate()
		{
			return Encryption.EncryptStringToBase64(_currentDataSource.CurrentName() + spilter + _loggedOnUser.CurrentUser().Id.Value, EncryptionConstants.Image1,
			                                        EncryptionConstants.Image2);
		}

		public CalendarLinkId Parse(string encryptedId)
		{
			var dataSourceNameAndPersonId = Encryption.DecryptStringFromBase64(encryptedId, EncryptionConstants.Image1, EncryptionConstants.Image2);
			var pos = dataSourceNameAndPersonId.LastIndexOf(spilter, StringComparison.Ordinal);
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