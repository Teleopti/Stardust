using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SpeedUpEvents_74996)]
	public class AnalyticsFactScheduleTimeMapperCacheTest : ISetup
	{
		public IAnalyticsFactScheduleTimeMapper Target;
		public AnalyticsAbsenceRepositoryCountNumberOfCalls AnalyticsAbsenceRepository;

		[Test]
		public void ShouldCacheMapOvertimeId()
		{
			var absenceCode = Guid.NewGuid();
			
			Target.MapAbsenceId(absenceCode);
			Target.MapAbsenceId(absenceCode);

			AnalyticsAbsenceRepository.NumberOfCalls
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldCacheEachIdSeperatly()
		{
			var absenceCode1 = Guid.NewGuid();
			var absenceCode2 = Guid.NewGuid();
			
			Target.MapAbsenceId(absenceCode1);
			Target.MapAbsenceId(absenceCode2);
			Target.MapAbsenceId(absenceCode1);
			Target.MapAbsenceId(absenceCode2);

			AnalyticsAbsenceRepository.NumberOfCalls
				.Should().Be.EqualTo(2);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AnalyticsAbsenceRepositoryCountNumberOfCalls>().For<IAnalyticsAbsenceRepository>();
		}

		public class AnalyticsAbsenceRepositoryCountNumberOfCalls : IAnalyticsAbsenceRepository
		{
			public int NumberOfCalls { get; set; }
			
			public IList<AnalyticsAbsence> Absences()
			{
				throw new NotImplementedException();
			}

			public void AddAbsence(AnalyticsAbsence analyticsAbsence)
			{
				throw new NotImplementedException();
			}

			public void UpdateAbsence(AnalyticsAbsence analyticsAbsence)
			{
				throw new NotImplementedException();
			}

			public AnalyticsAbsence Absence(Guid absenceId)
			{
				NumberOfCalls++;
				return null;
			}
		}
	}
}