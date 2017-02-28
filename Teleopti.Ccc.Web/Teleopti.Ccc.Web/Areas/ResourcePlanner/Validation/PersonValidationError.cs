using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonValidationError
	{
		public PersonValidationError()
		{
		}

		public PersonValidationError(IPerson person)
		{
			PersonId = person.Id.GetValueOrDefault();
			PersonName = person.Name.ToString();
		}
		public string PersonName { get; set; }
		public Guid PersonId { get; set; }
		public string ValidationError { get; set; }
	}
}