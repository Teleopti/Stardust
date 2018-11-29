using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ManageSchedule
{
	public class MoveTestCase<T>
	{
		private readonly string _type;

		public MoveTestCase(string type)
		{
			_type = type;
		}
		public Action<T, IPerson, DateOnlyPeriod, IScenario, IScenario> CreateType;

		public void CreateTypeInSourceScenario(WithUnitOfWork withUnitOfWork, T testClass, IPerson person, DateOnlyPeriod period, IScenario sourceScenario, IScenario targetScenario)
		{
			withUnitOfWork.Do(() =>
			{
				CreateType(testClass, person, period, sourceScenario, targetScenario);
			});
		}
		public Func<T, IScenario, IPersistableScheduleData> LoadMethod { get; set; }

		public void VerifyExistsInTargetScenario(WithUnitOfWork withUnitOfWork, T testClass, IScenario sourceScenario)
		{
			var archivedInstance = withUnitOfWork.Get(() => LoadMethod(testClass, sourceScenario));
			archivedInstance.Should().Not.Be.Null();
		}

		public override string ToString()
		{
			return _type;
		}
	}
}