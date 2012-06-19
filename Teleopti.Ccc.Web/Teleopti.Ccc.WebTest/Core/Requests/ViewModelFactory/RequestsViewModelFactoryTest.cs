using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	[TestFixture]
	public class RequestsViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnViewModel()
		{
			var absence = new Absence { Description = new Description("Vacation") };
			absence.SetId(Guid.NewGuid());
			var absences = new List<IAbsence> { absence };

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(absences);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider);

			var result = target.CreatePageViewModel();

			result.AbsenceTypes.FirstOrDefault().Name.Should().Be.EqualTo(absence.Description.Name);
			result.AbsenceTypes.FirstOrDefault().Id.Should().Be.EqualTo(absence.Id);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, MockRepository.GenerateMock<IMappingEngine>(), null);
			var paging = new Paging();

			personRequestProvider.Stub(x => x.RetrieveTextRequests(paging)).Return(new IPersonRequest[] { });

			target.CreatePagingViewModel(paging);

			personRequestProvider.AssertWasCalled(x => x.RetrieveTextRequests(paging));
		}

		[Test]
		public void ShouldMapToViewModelForPagingViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RequestsViewModelFactory(MockRepository.GenerateMock<IPersonRequestProvider>(), mapper, null);
			var requests = new RequestViewModel[] {};

			mapper.Stub(x => x.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(null)).Return(requests);

			var result = target.CreatePagingViewModel(null);

			result.Should().Be.SameInstanceAs(requests);
		}

		[Test]
		public void ShouldMapToViewModelForId()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var provider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(provider, mapper, null);
			var personRequest = new PersonRequest(new Person());
			var id = Guid.NewGuid();
			var requestViewModel = new RequestViewModel();

			personRequest.SetId(id);

			provider.Stub(p => p.RetrieveRequest(id)).Return(personRequest);
			mapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			var result = target.CreateRequestViewModel(id);

			result.Should().Be.SameInstanceAs(requestViewModel);
		}
	}
}
