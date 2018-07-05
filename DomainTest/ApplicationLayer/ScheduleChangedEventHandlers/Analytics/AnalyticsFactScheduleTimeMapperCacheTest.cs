using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsFactScheduleTimeMapperCacheTest : IIsolateSystem
	{
		public AnalyticsAbsenceMapper Target;
		public AnalyticsAbsenceRepositoryCountNumberOfCalls AnalyticsAbsenceRepository;

		[Test]
		public void ShouldCacheMapOvertimeId()
		{
			var absenceCode = Guid.NewGuid();
			
			Target.Map(absenceCode);
			Target.Map(absenceCode);

			AnalyticsAbsenceRepository.NumberOfCalls
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldCacheEachIdSeperatly()
		{
			var absenceCode1 = Guid.NewGuid();
			var absenceCode2 = Guid.NewGuid();
			
			Target.Map(absenceCode1);
			Target.Map(absenceCode2);
			Target.Map(absenceCode1);
			Target.Map(absenceCode2);

			AnalyticsAbsenceRepository.NumberOfCalls
				.Should().Be.EqualTo(2);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AnalyticsAbsenceRepositoryCountNumberOfCalls>().For<IAnalyticsAbsenceRepository>();
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