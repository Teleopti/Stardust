﻿namespace Teleopti.Wfm.Administration.Models
{
	public class ImportDatabaseModel
	{
		public string Tenant { get; set; }

		public string Server { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public string AppDatabase { get; set; }

		public string AnalyticsDatabase { get; set; }
	}
}