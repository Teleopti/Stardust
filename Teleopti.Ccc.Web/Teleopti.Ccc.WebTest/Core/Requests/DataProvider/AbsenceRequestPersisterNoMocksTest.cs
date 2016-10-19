using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, RequestsTest]
	public class AbsenceRequestPersisterNoMocksTest : ISetup
	{
		public IPersonRequestRepository PersonRequestRepository;
		public ILoggedOnUser LoggedOnUser;
		public IUserTimeZone UserTimeZone;
		public IAbsenceRepository AbsenceRepository;
		public IEventPublisher EventPublisher;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public ICurrentDataSource CurrentDataSource;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;

		private INow _now;
		private DateTime _nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest _absenceRequestFormToPersonRequest;
		private RequestsViewModelMappingProfile _requestsViewModelMappingProfile;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeAbsenceRepository()).For<IAbsenceRepository>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			system.UseTestDouble(new FakeCurrentDatasource("test")).For<ICurrentDataSource>();
		}

		public void SetUp()
		{
			_absenceRequestFormToPersonRequest =
				new AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest(() => Mapper.Engine, () => LoggedOnUser, () => AbsenceRepository, () => UserTimeZone);

			_requestsViewModelMappingProfile = new RequestsViewModelMappingProfile(UserTimeZone
				, null, LoggedOnUser, null, null,
				null);

			Mapper.Reset();
			Mapper.Initialize(c =>
			{
				c.AddProfile(new AbsenceRequestFormMappingProfile(() => _absenceRequestFormToPersonRequest));
				c.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
				c.AddProfile(_requestsViewModelMappingProfile);
			});

			_now = new ThisIsNow(_nowTime);

			((FakeScheduleDataReadScheduleStorage) ScheduleStorage).Clear();

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), LoggedOnUser.CurrentUser()
				, DateOnly.Today.ToDateTimePeriod(UserTimeZone.TimeZone())));

			((FakeCurrentBusinessUnit) CurrentBusinessUnit).FakeBusinessUnit(new BusinessUnit("test"));
		}

		[Test]
		public void ShouldAddRequest()
		{
			SetUp();

			setWorkflowControlSet();

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = DateOnly.Today,
				EndDate = DateOnly.Today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var requestViewModel = persist(form);
			var personRequest = PersonRequestRepository.Get(new Guid(requestViewModel.Id));
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(false);

			((FakeEventPublisher) EventPublisher).PublishedEvents.Count().Should().Be(1);
		}

		[Test]
		public void ShouldDenyExpiredRequest()
		{
			SetUp();

			setWorkflowControlSet(15);

			var period = new DateTimePeriodForm
			{
				StartDate = new DateOnly(_nowTime),
				EndDate = new DateOnly(_nowTime),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};
			var form = createAbsenceRequestForm(period);

			var requestViewModel = persist(form);
			var personRequest = PersonRequestRepository.Get(new Guid(requestViewModel.Id));
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(string.Format(Resources.RequestDenyReasonRequestExpired, personRequest.Request.Period.StartDateTime, 15));

			((FakeEventPublisher)EventPublisher).PublishedEvents.Count().Should().Be(0);
		}

		[Test]
		public void ShouldDenyWhenAlreadyAbsent()
		{
			SetUp();

			setWorkflowControlSet();

			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = DateOnly.Today,
				EndDate = DateOnly.Today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var form = createAbsenceRequestForm(dateTimePeriodForm);

			var absence = AbsenceRepository.Get(form.AbsenceId);

			ScheduleStorage.Add(PersonAbsenceFactory.CreatePersonAbsence(LoggedOnUser.CurrentUser(), CurrentScenario.Current()
				, DateOnly.Today.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
					, UserTimeZone.TimeZone()), absence).WithId());

			var requestViewModel = persist(form);
			var personRequest = PersonRequestRepository.Get(new Guid(requestViewModel.Id));
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(Resources.RequestDenyReasonAlreadyAbsent);

			((FakeEventPublisher)EventPublisher).PublishedEvents.Count().Should().Be(0);
		}

		private void setWorkflowControlSet(int? absenceRequestExpiredThreshold = null)
		{
			LoggedOnUser.CurrentUser().WorkflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold
			}.WithId();
		}

		private RequestViewModel persist(AbsenceRequestForm form)
		{
			var absenceRequestSynchronousValidator =
				new AbsenceRequestSynchronousValidator(new ExpiredRequestValidator(new FakeGlobalSettingDataRepository(), _now),
					new AlreadyAbsentValidator(), ScheduleStorage, CurrentScenario);
			var target = new AbsenceRequestPersister(PersonRequestRepository, Mapper.Engine, EventPublisher
				, CurrentBusinessUnit, CurrentDataSource, _now, new ThisUnitOfWork(new FakeUnitOfWork())
				, absenceRequestSynchronousValidator, new PersonRequestAuthorizationCheckerForTest());
			var requestViewModel = target.Persist(form);
			return requestViewModel;
		}

		private AbsenceRequestForm createAbsenceRequestForm(DateTimePeriodForm period)
		{
			var absence = createAbsence();
			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Subject = "test",
				Period = period
			};
			return form;
		}

		private IAbsence createAbsence()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			AbsenceRepository.Add(absence);
			return absence;
		}
	}
}
