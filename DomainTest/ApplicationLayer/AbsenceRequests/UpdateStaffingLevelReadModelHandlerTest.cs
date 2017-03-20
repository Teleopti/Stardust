using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class UpdateStaffingLevelReadModelHandlerTest : ISetup
	{
		public UpdateStaffingLevelReadModelHandler Target;
		public FakeUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;
		public MutableNow Now;
		public FakeJobStartTimeRepository JobStartTimeRepository;
	

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeUpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
			system.UseTestDouble<UpdateStaffingLevelReadModelHandler>().For<IHandleEvent<UpdateStaffingLevelReadModelEvent>>();
		}

		[Test]
		public void ShouldUpdateReadModel()
		{
			Target.Handle(
				new UpdateStaffingLevelReadModelEvent
				{
					Days = 1,
					LogOnBusinessUnitId = Guid.NewGuid()
				});
			UpdateStaffingLevelReadModel.WasCalled.Should().Be.True();
		}
	}

	public class FakeUpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		public bool WasCalled { get; set; }
		public void Update(DateTimePeriod period)
		{
			WasCalled = true;
		}

		public void UpdateFromResourceCalculationData(DateTimePeriod period, ResourceCalculationData resCalcData,
			DateOnlyPeriod periodDateOnly, DateTime timeWhenResourceCalcDataLoaded)
		{
			throw new NotImplementedException();
		}

		public IList<SkillStaffingInterval> CreateReadModel(ISkillResourceCalculationPeriodDictionary skillSkillStaffPeriodExtendedDictionary,
			DateTimePeriod period)
		{
			throw new NotImplementedException();
		}
	}
}
