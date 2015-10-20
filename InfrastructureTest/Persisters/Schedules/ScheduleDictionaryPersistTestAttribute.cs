using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		public IScheduleRepository ScheduleRepository;
		public IPersonRepository PersonRepository;
		public ICurrentScenario Scenario;
		public FakePersistCallback MessageSender2;

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

			MessageSender2.Clear();
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.UseTestDouble<FakePersistCallback>().For<IPersistCallback>();
			system.AddService(this);
		}

		public IScheduleDictionary MakeDictionary()
		{
			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				schedules = ScheduleRepository.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
					Scenario.Current(),
					new PersonProvider(new IPerson[] { }),
					new ScheduleDictionaryLoadOptions(true, true),
					new List<IPerson>());
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