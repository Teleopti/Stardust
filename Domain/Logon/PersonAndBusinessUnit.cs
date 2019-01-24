using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public class PersonAndBusinessUnit : IPrincipalSource
	{
		private readonly IPerson _person;
		private readonly IBusinessUnit _businessUnit;

		public PersonAndBusinessUnit(IPerson person, IBusinessUnit businessUnit)
		{
			_person = person;
			_businessUnit = businessUnit;
		}

		public Guid PrincipalPersonId() => _person?.Id.GetValueOrDefault() ?? Guid.Empty;
		public string PrincipalName() => _person?.Name.ToString();
		public TimeZoneInfo PrincipalTimeZone() => _person?.PermissionInformation?.DefaultTimeZone();
		public int? PrincipalCultureLCID() => _person?.PermissionInformation.CultureLCID();
		public int? PrincipalUICultureLCID() => _person?.PermissionInformation.UICultureLCID();
		public IEnumerable<IPrincipalSourcePeriod> PrincipalPeriods() => _person?.PersonPeriodCollection;

		public Guid? PrincipalBusinessUnitId() => _businessUnit?.Id;
		public string PrincipalBusinessUnitIdName() => _businessUnit?.Name;

		public object UnsafePerson() => _person;
		public object UnsafeBusinessUnit() => _businessUnit;
	}
}