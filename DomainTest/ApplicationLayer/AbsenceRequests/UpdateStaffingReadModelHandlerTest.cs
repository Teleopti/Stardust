using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Rhino.Mocks.Exceptions;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	[DomainTest]
	public class UpdateStaffingReadModelHandlerTest: IIsolateSystem
	{
		public UpdateStaffingLevelReadModelHandler Target;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public MutableNow Now;
		public FakeJobStartTimeRepository JobStartTimeRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeUpdateStaffingLevelReadModel UpdateStaffingLevelReadModel;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
		}

		[Test]
		public void ShouldResetLockWhenJobSuccess()
		{
			var bu = Guid.NewGuid();
			JobStartTimeRepository.CheckAndUpdate(5, bu);
			Target.DoTheWork(new UpdateStaffingLevelReadModelEvent{LogOnBusinessUnitId = bu});
			JobStartTimeRepository.Records[bu].LockTimestamp.Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldRemoveLockWhenJobFails()
		{
			UpdateStaffingLevelReadModel.Exception = SqlExceptionConstructor.CreateSqlException("bloody timeout", 1205);

			var bu = Guid.NewGuid();
			JobStartTimeRepository.CheckAndUpdate(5, bu);

			Assert.Throws<SqlException>(() => Target.DoTheWork(new UpdateStaffingLevelReadModelEvent { LogOnBusinessUnitId = bu }));
			
			JobStartTimeRepository.Records.ContainsKey(bu).Should().Be.False();
		}
		
	}
}
