using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.Validation
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