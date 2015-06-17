using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Wfm.Administration.Models
{
	public class ConflictModel
	{
		public int NumberOfConflicting { get; set; }
		public int NumberOfNotConflicting { get; set; }
		public IEnumerable<ImportUserModel> ConflictingUserModels { get; set; }
		public IEnumerable<NotConflictingUserModel> NotConflicting { get; set; }
	}

	{
		public string AppLogon { get; set; }
		public string Identity { get; set; }
	}

	public class NotConflictingUserModel
	{
		public Guid PersonId { get; set; }
		public string AppLogon { get; set; }
		public string Password { get; set; }
	}
}