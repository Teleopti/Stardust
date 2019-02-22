using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class InfrastructureTestSetup
	{
		private static IPerson _person;
		private static IBusinessUnit _businessUnit;

		public static (IPerson Person, IBusinessUnit BusinessUnit) Before()
		{
			DatabaseTestSetup.Setup(context =>
			{
				_businessUnit = new BusinessUnit(RandomName.Make());
				_person = new Person()
					.WithName(RandomName.Make())
					.InTimeZone(TimeZoneInfo.Utc);
				context.UpdatedByScope.OnThisThreadUse(_person);
				context.WithUnitOfWork.Do(() =>
				{
					context.Persons.Add(_person);
					context.Persons.Remove(_person); // SetDeleted
					context.BusinessUnits.Add(_businessUnit);
				});
				context.UpdatedByScope.OnThisThreadUse(null);
				return 254875;
			});

			return (_person, _businessUnit);
		}

		public static void After()
		{
		}
	}
}