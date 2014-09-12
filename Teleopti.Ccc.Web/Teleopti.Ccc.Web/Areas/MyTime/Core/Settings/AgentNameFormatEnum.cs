using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings
{

	public enum AgentNameFormat
	{
		[Display(Name = "AgentNameFormatFirstNameLastName", ResourceType = typeof (Resources))] 
		FirstNameLastName = 0,
		[Display(Name = "AgentNameFormatLastNameFirstName", ResourceType = typeof(Resources))]
		LastNameFirstName = 1,
	}

}