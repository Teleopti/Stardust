using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	public class TeamScheduleViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new TeamScheduleViewModelFactory(mapper, new FakePermissionProvider());
			var viewModel = new TeamScheduleViewModel();
			var data = new TeamScheduleDomainData();
			var id = Guid.NewGuid();

			mapper.Stub(x => x.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, id))).Return(data);
			mapper.Stub(x => x.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today, id);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void PermissionForShiftTrade_WhenAgentHasNoPermissionToViewShiftTrade_ShouldBFalse()
		{
			var target = new TeamScheduleViewModelFactory(createMappingEngine(), new FakePermissionProvider());

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);			
		}

		[Test]
		public void PermissionForShiftTrade_WhenAgentHasPermissionToViewShiftTrade_ShouldBeTrue()
		{
			var target = new TeamScheduleViewModelFactory(createMappingEngine(), new FakePermissionProvider());

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);
		}

		private static IMappingEngine createMappingEngine()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var viewModel = new TeamScheduleViewModel();

			mapper.Stub(x => x.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, new Guid())))
				  .Return(new TeamScheduleDomainData())
			      .IgnoreArguments();
			mapper.Stub(x => x.Map<TeamScheduleDomainData, TeamScheduleViewModel>(null))
				.IgnoreArguments()
				.Return(viewModel);
			return mapper;
		}
	}
}
