using System.Collections.Generic;

namespace Teleopti.Wfm.Administration.Models
{
	public class ConflictModel
	{
		public int NumberOfConflicting { get; set; }
		public int NumberOfNotConflicting { get; set; }
		public IEnumerable<ImportUserModel> ConflictingUserModels { get; set; }
		public IEnumerable<ImportUserModel> NotConflicting { get; set; }
	}
	public class ImportUserModel
	{
		public string AppLogon { get; set; }
		public string AppPassword { get; set; }
	}
}