using System.Collections.Generic;

namespace Teleopti.Wfm.Administration.Models
{
	public class ConflictModel
	{
		public int NumberOfConflicting { get; set; }
		public int NumberOfNotConflicting { get; set; }
		public IEnumerable<ConflictingUserModel> ConflictingUserModels { get; set; }
	}
	public class ConflictingUserModel
	{
		public string AppLogon { get; set; }
		public string Identity { get; set; }
	}
}