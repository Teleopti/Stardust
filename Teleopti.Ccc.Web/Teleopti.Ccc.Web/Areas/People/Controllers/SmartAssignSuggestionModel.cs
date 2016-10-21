using System;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class SmartAssignSuggestionModel
	{
		public Guid PersonId { get; set; }
		public Guid? CurrentId { get; set; }
		public Guid SuggestedId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string CurrentSite { get; set; }
		public string CurrentTeam { get; set; }
		public double Confidence { get; set; }
	}
}