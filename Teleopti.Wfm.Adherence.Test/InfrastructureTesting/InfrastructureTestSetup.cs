using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class InfrastructureTestSetup
	{
		public static (IPerson Person, IBusinessUnit BusinessUnit) Before()
		{
			return DatabaseTestSetup.Setup(context =>
			{
				var businessUnit = new BusinessUnit(RandomName.Make());
				var person = new Person()
					.WithName(RandomName.Make())
					.InTimeZone(TimeZoneInfo.Utc);
				context.UpdatedByScope.OnThisThreadUse(person);
				context.WithUnitOfWork.Do(() =>
				{
					context.Persons.Add(person);
					context.Persons.Remove(person); // SetDeleted
					context.BusinessUnits.Add(businessUnit);
				});
				context.UpdatedByScope.OnThisThreadUse(null);
				return new CreateDataResult<(IPerson, IBusinessUnit)>
				{
					Hash = 213124,
					Data = (person, businessUnit)
				};
			});
		}

		public static void After()
		{
			DatabaseTestSetup.Release();
		}
	}
}