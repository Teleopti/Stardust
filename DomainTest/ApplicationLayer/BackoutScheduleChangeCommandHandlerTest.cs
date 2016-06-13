using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[TestFixture]
	public class BackoutScheduleChangeCommandHandlerTest : ISetup
	{
		public FakeScheduleHistoryRepository ScheduleHistoryRepository;		
		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public FakeScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleDifferenceSaver ScheduleDifferenceSaver;

		public BackoutScheduleChangeCommandHandler target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleHistoryRepository>().For<IScheduleHistoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<FakeAggregateRootInitializer>().For<IAggregateRootInitializer>();
			system.AddService<BackoutScheduleChangeCommandHandler>();
		}

		[Test]
		public void HanlderShouldBeResolved()
		{
			target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIndicateCannotBackoutWhenNoHistoryIsFound()
		{
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			ScheduleHistoryRepository.ClearRevision();

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2016, 06, 11)
			};

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotBackoutScheduleChange);
		}

		[Test]
		public void ShouldIndicateCannotBackoutWhenLastChangeIsFromDifferentUser()
		{
			var person1 = PersonFactory.CreatePerson("aa","aa").WithId();
			var person2 = PersonFactory.CreatePerson("bb","bb").WithId();
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);

			LoggedOnUser.SetFakeLoggedOnUser(person2);
			ScheduleHistoryRepository.ClearRevision();

			var rev = new Revision {Id = 1};
			rev.SetRevisionData(person1);
			ScheduleHistoryRepository.SetRevision(rev, null);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person1.Id.Value,
				Date = new DateOnly(2016,06,11)
			};

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotBackoutScheduleChange);
		}

	}	
}
