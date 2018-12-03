using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public interface IScheduleDictionaryPersistTestHelper
	{
		IActivity Activity();
		IPerson NewPerson();
		IScheduleDictionary MakeDictionary();
	}

	public class ScheduleDictionaryPersistTestAttribute : PrincipalAndStateTestAttribute, IScheduleDictionaryPersistTestHelper
	{
		public WithUnitOfWork WithUnitOfWork;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IScenarioRepository ScenarioRepository;
		public IAbsenceRepository AbsenceRepository;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public IScheduleStorage ScheduleStorage;
		public IPersonRepository PersonRepository;
		public ICurrentScenario Scenario;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			WithUnitOfWork.Do(() =>
			{
				ActivityRepository.Add(new Activity("persist test"));
				ShiftCategoryRepository.Add(new ShiftCategory("persist test"));
				ScenarioRepository.Add(new Scenario("scenario") { DefaultScenario = true });
				AbsenceRepository.Add(new Absence { Description = new Description("perist", "test") });
				MultiplicatorDefinitionSetRepository.Add(new MultiplicatorDefinitionSet("persist test", MultiplicatorType.Overtime));
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("persist test")));
			});
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService(this);
		}
		
		public IScheduleDictionary MakeDictionary()
		{
			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				schedules = ScheduleStorage.FindSchedulesForPersons(Scenario.Current(),
					new IPerson[] { },
					new ScheduleDictionaryLoadOptions(true, true),
					new DateTimePeriod(1800, 1, 1, 2040, 1, 1), new List<IPerson>(), false);
			});
			return schedules;
		}

		public IActivity Activity()
		{
			return WithUnitOfWork.Get(() => ActivityRepository.LoadAll().First());
		}

		public IPerson NewPerson()
		{
			var person = PersonFactory.CreatePerson();
			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
			});
			return person;
		}
	}
}