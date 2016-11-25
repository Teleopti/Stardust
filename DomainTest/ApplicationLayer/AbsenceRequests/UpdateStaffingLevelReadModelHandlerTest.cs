using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[Toggle(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	[DomainTest]
	public class UpdateStaffingLevelReadModelHandlerTest : ISetup
	{
		public UpdateStaffingLevelReadModelHandler Target;
		public FakeUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModelHandler>().For<IHandleEvent<UpdateStaffingLevelReadModelEvent>>();
			system.UseTestDouble<FakeUpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
		}

		[Test]
		public void ShouldUpdateReadModel()
		{
			var buID = Guid.NewGuid();
			Target.Handle(new UpdateStaffingLevelReadModelEvent() {StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc) ,
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateReadModelIfNoBusinessUnitFound()
		{
			Now.Is(new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc));
			var buID = Guid.NewGuid();
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = Guid.NewGuid()
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldExitReadModelIsUpToDate()
		{
			Now.Is(new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc));
			var buID = Guid.NewGuid();
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent() { StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
																	EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
			LogOnBusinessUnitId = buID});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldUpdateDateIfReadModelIsToBeUpdated()
		{
			Now.Is(new DateTime(2016, 10, 03, 11, 1, 0, DateTimeKind.Utc));
			var buID = Guid.NewGuid();
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(buID, new DateTime(2016, 10, 03, 10,0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID
			});
			ScheduleForecastSkillReadModelRepository.UpdateReadModelDateTimeWasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldRunJobIfRequestedFromWeb()
		{
			Now.Is(new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc));
			var buID = Guid.NewGuid();
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID,
				RequestedFromWeb = true
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateInsertedOnWhenRequestedFromWeb()
		{
			Now.Is(new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc));
			var buID = Guid.NewGuid();
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID,
				RequestedFromWeb = true
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
			ScheduleForecastSkillReadModelRepository.UpdateReadModelDateTimeWasCalled.Should().Be.True();
		}
	}

	public class FakeUpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		public bool WasCalled { get; set; }
		public void Update(DateTimePeriod period)
		{
			WasCalled = true;
		}

		public IList<SkillStaffingInterval> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary,
			DateTimePeriod period)
		{
			throw new NotImplementedException();
		}
	}
}
