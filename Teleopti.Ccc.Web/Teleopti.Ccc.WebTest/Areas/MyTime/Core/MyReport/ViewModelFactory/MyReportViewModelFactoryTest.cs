using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	[TestFixture]
	public class MyReportViewModelFactoryTest
	{
		private FakeLoggedOnUser _loggedOnUser;
		private IDetailedAdherenceForDayQuery _adherenceForDayQuery;
		private IDetailedAdherenceMapper _detailedAdherenceMapper;

		[SetUp]

		public void Setup()
		{
			_loggedOnUser = new FakeLoggedOnUser();

			var userCulture = new FakeUserCulture();
			userCulture.IsSwedish();
			_detailedAdherenceMapper = new DetailedAdherenceMapper(userCulture);
			_adherenceForDayQuery = MockRepository.GenerateMock<IDetailedAdherenceForDayQuery>();
		}

		[Test]
		public void ShouldViewPublishedDetailAdherence()
		{
			var date = new DateOnly(2016, 01, 01);
			_loggedOnUser.CurrentUser().WithName(new Name("Loggon", "User"));

			var dataModels = new List<DetailedAdherenceForDayResult>();
			dataModels.Add(new DetailedAdherenceForDayResult());

			_adherenceForDayQuery.Stub(x => x.Execute(date)).Return(dataModels);

			var target = new MyReportViewModelFactory(null, _adherenceForDayQuery, null, _detailedAdherenceMapper, null, null, new FakePermissionProvider(false), _loggedOnUser);

			var result = target.CreateDetailedAherenceViewModel(date);

			result.Should().Be.OfType<DetailedAdherenceViewModel>();
			result.DataAvailable.Should().Be.True();
		}

		[Test]
		public void ShouldNotViewUnpublishedDetailAdherence()
		{
			var date = new DateOnly(2016, 01, 01);
			_loggedOnUser.CurrentUser().WithName(new Name("Unpublish", "loggonUser"));

			var dataModels = new List<DetailedAdherenceForDayResult>();
			dataModels.Add(new DetailedAdherenceForDayResult());

			_adherenceForDayQuery.Stub(x => x.Execute(date)).Return(dataModels);

			var target = new MyReportViewModelFactory(null, _adherenceForDayQuery, null, _detailedAdherenceMapper, null, null, new FakePermissionProvider(false), _loggedOnUser);

			var result = target.CreateDetailedAherenceViewModel(date);

			result.DataAvailable.Should().Be.False();
		}

		[Test]
		public void ShouldViewUnpublishedDetailAdherenceWhenUserCanViewUnpublishedSchedule()
		{
			var date = new DateOnly(2016, 01, 01);
			_loggedOnUser.CurrentUser().WithName(new Name("Unpublish", "loggonUser"));

			var dataModels = new List<DetailedAdherenceForDayResult>();
			dataModels.Add(new DetailedAdherenceForDayResult());

			_adherenceForDayQuery.Stub(x => x.Execute(date)).Return(dataModels);

			var target = new MyReportViewModelFactory(null, _adherenceForDayQuery, null, _detailedAdherenceMapper, null, null, new FakePermissionProvider(), _loggedOnUser);

			var result = target.CreateDetailedAherenceViewModel(date);

			result.Should().Be.OfType<DetailedAdherenceViewModel>();
			result.DataAvailable.Should().Be.True();
		}
	}
}