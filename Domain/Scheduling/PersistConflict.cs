using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PersistConflict
	{
		private const string exMessage = "Incorrect conflict. DatabaseVersion = {0}, OrgItem = {1}, CurrentItem = {2}.";

		public PersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			ClientVersion = clientVersion;
			DatabaseVersion = databaseVersion;
		}

		public DifferenceCollectionItem<IPersistableScheduleData> ClientVersion { get; private set; }
		public IPersistableScheduleData DatabaseVersion { get; private set; }

		public Guid InvolvedId()
		{
			if (DatabaseVersion!=null && DatabaseVersion.Id.HasValue) 
				return DatabaseVersion.Id.Value;
			if (ClientVersion.OriginalItem != null && ClientVersion.OriginalItem.Id.HasValue)
				return ClientVersion.OriginalItem.Id.Value;
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessage, DatabaseVersion, ClientVersion.OriginalItem, ClientVersion.CurrentItem));
		}
	}
}