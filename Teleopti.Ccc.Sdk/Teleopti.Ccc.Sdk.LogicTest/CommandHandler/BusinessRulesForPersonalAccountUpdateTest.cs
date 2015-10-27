using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class BusinessRulesForPersonalAccountUpdateTest
	{
		private MockRepository _mock;
		private IPersonAbsenceAccountRepository personalAbsenceAccountRepository;
		private ISchedulingResultStateHolder schedulingResultStateHolder;
		private IBusinessRulesForPersonalAccountUpdate target;
		private IScheduleRange scheduleRange;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			personalAbsenceAccountRepository = _mock.DynamicMock<IPersonAbsenceAccountRepository>();
			schedulingResultStateHolder = _mock.DynamicMock<ISchedulingResultStateHolder>();
			scheduleRange = _mock.DynamicMultiMock<IScheduleRange>(typeof(IValidateScheduleRange));

			target = new BusinessRulesForPersonalAccountUpdate(personalAbsenceAccountRepository, schedulingResultStateHolder);
		}

		[Test]
		public void ShouldLoadBusinessRulesAndUpdatePersonalAccountFromScheduleRange()
		{
			using (_mock.Record())
			{
				
			}
			using(_mock.Playback())
			{
				var result = target.FromScheduleRange(scheduleRange).FirstOrDefault(r => r.GetType() == typeof(NewPersonAccountRule));
				result.HaltModify.Should().Be.False();
			}
		}
	}
}