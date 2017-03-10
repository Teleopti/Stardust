using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class PrincipalPerson
	{
		public PrincipalPerson(IPerson person)
		{
			PersonId = person.Id.GetValueOrDefault();
			Name = person.Name;
			RightToLeftDisplay = person.PermissionInformation.RightToLeftDisplay;
			UICulture = person.PermissionInformation.UICulture();
			Culture = person.PermissionInformation.Culture();
			DefaultTimeZoneString = person.PermissionInformation.DefaultTimeZoneString();
			DefaultTimeZone = person.PermissionInformation.DefaultTimeZone();
			Email = person.Email;
		}

		public CultureInfo UICulture { get; }
		public CultureInfo Culture { get; }
		public Guid PersonId { get; }
		public Name Name { get; }
		public bool RightToLeftDisplay { get; }
		public string DefaultTimeZoneString { get; }
		public TimeZoneInfo DefaultTimeZone { get; }
		public string Email { get; }

		[Obsolete("This is not recommended, get the person using a PersonRepository on the principal instead")]
		public IPerson UnsafePerson()
		{
			var unsafePerson = new Person();
			unsafePerson.SetId(PersonId);
			return unsafePerson;
		}
	}
}