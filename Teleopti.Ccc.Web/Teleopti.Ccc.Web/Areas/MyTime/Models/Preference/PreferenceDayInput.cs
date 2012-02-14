using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput
	{
		public DateOnly Date { get; set; }
		public Guid Id { get; set; }
	}
}