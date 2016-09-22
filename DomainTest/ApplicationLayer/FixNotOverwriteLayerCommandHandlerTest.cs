using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
	public class FixNotOverwriteLayerCommandHandlerTest : ISetup
	{
		public FixNotOverwriteLayerCommandHandler Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey>>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FixNotOverwriteLayerCommandHandler>().For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldFixNotOverwriteLayerInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,PersonRepository.Single(),
				new DateTimePeriod(2013,11,14,8,2013,11,14,15),shiftCategory,scenario);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013,11,14)			
			};
			Target.Handle(command);
			
			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,12,2013,11,14,15));			
		}

	}
}
