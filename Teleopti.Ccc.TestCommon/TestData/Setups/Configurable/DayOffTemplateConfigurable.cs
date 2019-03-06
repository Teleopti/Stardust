using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class DayOffTemplateConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string ShortName { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var dayOffTemplate = new DayOffTemplate(new Description(Name, ShortName));
			dayOffTemplate.SetTargetAndFlexibility(new TimeSpan(24, 0, 0), new TimeSpan(6, 0, 0));

			var dayOffRepository = DayOffTemplateRepository.DONT_USE_CTOR2(currentUnitOfWork);
			dayOffRepository.Add(dayOffTemplate);
		}
	}
}