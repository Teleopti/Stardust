using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class CalculateResourceReadModelTest : ISetup
	{
		public CalculateResourceReadModel Target;
		public MutableNow _now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<CalculateResourceReadModel>().For<CalculateResourceReadModel>();
		}

		[Test]
		public void ShouldPerformResourceCalculation()
		{
			_now.Is(new DateTime(2016,10,03,10,0,0,DateTimeKind.Utc));
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodExt = new SkillSkillStaffPeriodExtendedDictionary();
			Target.ResourceCalculatePeriod(new DateTimePeriod());
		}
	}
	
}
