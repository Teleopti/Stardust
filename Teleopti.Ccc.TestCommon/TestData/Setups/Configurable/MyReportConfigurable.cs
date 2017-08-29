using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class MyReportConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public bool DataAvailable { get; set; }
		public int AnsweredCalls { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			if (!DataAvailable)
			{
			}
		}
	}
}