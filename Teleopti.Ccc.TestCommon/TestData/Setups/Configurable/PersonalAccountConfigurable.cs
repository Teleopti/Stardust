using System;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonalAccountConfigurable : IUserDataSetup
	{
		public string Absence { get; set; }
		public string Type { get; set; }
		public string Accured { get; set; }
		public DateTime PeriodStartTime { get; set; }
		public DateTime PeriodEndTime { get; set; }
		public string Remaining { get; set; }
		public string Used { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			

		}
	}
}