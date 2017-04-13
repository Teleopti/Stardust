using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.MultiplicatorDefinitionSetHandlers
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsOvertimeUpdaterTest : ISetup
	{
		public AnalyticsOvertimeUpdater Target;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public IAnalyticsOvertimeRepository AnalyticsOvertimeRepository;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsOvertimeUpdater>();
		}

		[Test, TestCaseSource(nameof(overtimeEvents))]
		public void ShouldAddOrUpdate(MultiplicatorDefinitionSetChangedBase @event)
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(@event.LogOnBusinessUnitId));
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Test", MultiplicatorType.Overtime).WithId(Guid.NewGuid());
			MultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();
			Target.Handle((dynamic)@event);

			var analyticsOvertime = AnalyticsOvertimeRepository.Overtimes().FirstOrDefault(x => x.OvertimeCode == multiplicatorDefinitionSet.Id);
			analyticsOvertime.Should().Not.Be.Null();
		}

		[Test, TestCaseSource(nameof(nonOvertimeEvents))]
		public void ShouldDoNothingWhenWrongTypeOfMultiplicatorDefinitionSet(MultiplicatorDefinitionSetChangedBase @event)
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(@event.LogOnBusinessUnitId));
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Test", MultiplicatorType.Overtime).WithId(Guid.NewGuid());
			MultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();

			Target.Handle((dynamic)@event);

			AnalyticsOvertimeRepository.Overtimes().Should().Be.Empty();
		}

		#region TestData
		private static readonly object[] overtimeEvents =
		{
			new object[] {createEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.Overtime)},
			new object[] {createEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.Overtime)},
			new object[] {createEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.Overtime) }
		};

		private static readonly object[] nonOvertimeEvents =
		{
			new object[] {createEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.OBTime)},
			new object[] {createEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.OBTime) },
			new object[] {createEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.OBTime) }
		};

		

		private static T createEvent<T>(MultiplicatorType type) where T : MultiplicatorDefinitionSetChangedBase, new()
		{
			return new T
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				MultiplicatorDefinitionSetId = Guid.NewGuid(),
				MultiplicatorType = type
			};
		}

		#endregion
	}
}