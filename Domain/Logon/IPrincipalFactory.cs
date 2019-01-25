using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface IPrincipalFactory
	{
		ITeleoptiPrincipal MakePrincipal(IPrincipalSource source, IDataSource dataSource, string tokenIdentity);
	}

	public interface IPrincipalSource
	{
		Guid PrincipalPersonId();
		string PrincipalName();
		TimeZoneInfo PrincipalTimeZone();
		int? PrincipalCultureLCID();
		int? PrincipalUICultureLCID();
		IEnumerable<IPrincipalSourcePeriod> PrincipalPeriods();
		Guid? PrincipalBusinessUnitId();
		string PrincipalBusinessUnitIdName();
		
		object UnsafePerson();
		object UnsafeBusinessUnit();
	}

	public interface IPrincipalSourcePeriod
	{
		DateTime PrincipalStartDate();
		DateTime PrincipalEndDate();
		Guid? PrincipalTeamId();
		Guid? PrincipalSiteId();
		Guid? PrincipalBusinessUnitId();
	}
}