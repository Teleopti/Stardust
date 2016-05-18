using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.MultiplicatorDefinitionSetHandlers
{
	[TestFixture]
	public class AnalyticsOvertimeUpdaterTest
	{
		private AnalyticsOvertimeUpdater _target;

		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsOvertimeRepository _analyticsOvertimeRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsOvertimeRepository = MockRepository.GenerateMock<IAnalyticsOvertimeRepository>();

			_target = new AnalyticsOvertimeUpdater(_analyticsOvertimeRepository, _analyticsBusinessUnitRepository);
		}

		[Test, TestCaseSource("OvertimeEvents")]
		public void ShouldAddOrUpdate(MultiplicatorDefinitionSetChangedBase @event)
		{
			_target.Handle((dynamic)@event);

			_analyticsOvertimeRepository.AssertWasCalled(r =>
					r.AddOrUpdate(Arg<AnalyticsOvertime>.Matches(ao =>
								ao.IsDeleted == @event.IsDeleted &&
								ao.OvertimeCode == @event.MultiplicatorDefinitionSetId &&
								ao.OvertimeName == @event.MultiplicatorDefinitionSetName)));
		}

		[Test, TestCaseSource("NonOvertimeEvents")]
		public void ShouldDoNothingWhenWrongTypeOfMultiplicatorDefinitionSet(MultiplicatorDefinitionSetChangedBase @event)
		{
			_target.Handle((dynamic)@event);

			_analyticsOvertimeRepository.AssertWasNotCalled(r => r.AddOrUpdate(Arg<AnalyticsOvertime>.Is.Anything));
		}

		#region TestData
		private static object[] OvertimeEvents =
		{
			new object[] {CreateEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.Overtime)},
			new object[] {CreateEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.Overtime)},
			new object[] {CreateEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.Overtime) }
		};

		private static object[] NonOvertimeEvents =
		{
			new object[] {CreateEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.OBTime)},
			new object[] {CreateEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.OBTime) },
			new object[] {CreateEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.OBTime) }
		};

		private static T CreateEvent<T>(MultiplicatorType type) where T : MultiplicatorDefinitionSetChangedBase, new()
		{
			return new T
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				IsDeleted = false,
				DatasourceUpdateDate = DateTime.UtcNow,
				MultiplicatorDefinitionSetId = Guid.NewGuid(),
				MultiplicatorDefinitionSetName = "Test",
				MultiplicatorType = type
			};
		}
		#endregion
	}
}