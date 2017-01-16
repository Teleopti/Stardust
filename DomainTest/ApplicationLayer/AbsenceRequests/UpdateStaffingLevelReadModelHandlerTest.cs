using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
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
		public MutableNow Now;
		public FakeJobStartTimeRepository JobStartTimeRepository;
	

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModelHandler>().For<IHandleEvent<UpdateStaffingLevelReadModelEvent>>();
			system.UseTestDouble<FakeUpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
			system.UseTestDouble<FakeJobStartTimeRepository>().For<IJobStartTimeRepository>();
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
			var date = new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc);
			Now.Is(date);
			var buID = Guid.NewGuid();
			var missingBuId = Guid.NewGuid();
			JobStartTimeRepository.Persist(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = missingBuId
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
			JobStartTimeRepository.Records.Keys.Should().Contain(missingBuId);
			JobStartTimeRepository.Records.Values.Should().Contain(date);
		}

		[Test]
		public void ShouldExitIfReadModelIsUpToDate()
		{
			var buID = Guid.NewGuid();
			JobStartTimeRepository.Persist(buID, new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc));

			var newNow = new DateTime(2016, 10, 03, 10, 50, 0, DateTimeKind.Utc);
			Now.Is(newNow);

			Target.Handle(new UpdateStaffingLevelReadModelEvent() { StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
																	EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
			LogOnBusinessUnitId = buID});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldUpdateDateIfReadModelIsToBeUpdated()
		{
			var buID = Guid.NewGuid();
			JobStartTimeRepository.Persist(buID, new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc));

			var newNow = new DateTime(2016, 10, 03, 11, 1, 0, DateTimeKind.Utc);
			Now.Is(newNow);
			
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
			JobStartTimeRepository.Records.Count.Should().Be.EqualTo(1);
			JobStartTimeRepository.Records.Keys.Should().Contain(buID);
			JobStartTimeRepository.Records.Values.Should().Contain(newNow);
		}

		[Test]
		public void ShouldRunJobIfRequestedFromWeb()
		{
			var date = new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc);
			Now.Is(date);
			var buID = Guid.NewGuid();
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
			var date = new DateTime(2016, 10, 03, 10, 10, 0, DateTimeKind.Utc);
			Now.Is(date);
			var buID = Guid.NewGuid();
			Target.Handle(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = new DateTime(2016, 10, 03, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 10, 03, 12, 0, 0, DateTimeKind.Utc),
				LogOnBusinessUnitId = buID,
				RequestedFromWeb = true
			});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
			JobStartTimeRepository.Records.Keys.Should().Contain(buID);
			JobStartTimeRepository.Records.Values.Should().Contain(date);
		}
	}

	public class FakeJobStartTimeRepository : IJobStartTimeRepository
	{
		public Dictionary<Guid,DateTime> Records = new Dictionary<Guid, DateTime>();
		public void Persist(Guid buId, DateTime datetime)
		{
			if (Records.ContainsKey(buId))
				Records.Remove(buId);
			Records.Add(buId,datetime);
		}

		public IDictionary<Guid, DateTime> LoadAll()
		{
			return Records;
		}
	}

	public class FakeUpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		public bool WasCalled { get; set; }
		public void Update(DateTimePeriod period)
		{
			WasCalled = true;
		}

		public void UpdateFromResourceCalculationData(DateTimePeriod period, IResourceCalculationData resCalcData,
			DateOnlyPeriod periodDateOnly, DateTime timeWhenResourceCalcDataLoaded)
		{
			throw new NotImplementedException();
		}

		public IList<SkillStaffingInterval> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary,
			DateTimePeriod period)
		{
			throw new NotImplementedException();
		}
	}
}
