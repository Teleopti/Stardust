using System;
using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class ShiftBagUpdateCommandInputModel: PersonCommandInputModel
	{
		public Guid? ShiftBagId { get; set; }
	}
}