using System;
using System.Dynamic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ShareCalendarControllerTest
	{
		private IRepositoryFactory _repositoryFactory;
		private IDataSource _dataSource;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;
		private IPersonRepository _personRepository;
		private IDataSourcesProvider _dataSourcesProvider;
		private INow _now;
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private ICurrentPrincipalContext _currentPrincipalContext;
		private IPersonalSettingDataRepository _personalSettingDataRepository;
		private IRoleToPrincipalCommand _roleToPrincipalCommand;
		private IPermissionProvider _permissionProvider;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_dataSource.Stub(x => x.Application).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_repositoryFactory.Stub(x => x.CreatePersonRepository(_unitOfWork)).Return(_personRepository);
			_dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			_dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName("Main")).Return(_dataSource);
			_now = MockRepository.GenerateMock<INow>();
			_now.Stub(x => x.DateOnly()).Return(DateOnly.Today);
			_personScheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			_currentPrincipalContext = MockRepository.GenerateMock<ICurrentPrincipalContext>();
			_personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
		}

		[Test]
		public void ShouldGetCalendarForPerson()
		{
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
			                             .Return(new CalendarLinkSettings
				                             {
					                             IsActive = true
				                             });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork)).Return(_personalSettingDataRepository);
			_personScheduleDayReadModelFinder.Stub(x => x.ForPerson(DateOnly.Today.AddDays(-60), DateOnly.Today.AddDays(180), person.Id.Value)).Return(new[]
				{
					new PersonScheduleDayReadModel
						{
							Shift ="{Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Phone'}]}"
						}
				});
			_repositoryFactory.Stub(x => x.CreatePersonScheduleDayReadModelFinder(_unitOfWork)).Return(_personScheduleDayReadModelFinder);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
			                   .Return(true);
			var deserializer = new NewtonsoftJsonDeserializer<ExpandoObject>();

			using (var target = new ShareCalendarController(_repositoryFactory, _dataSourcesProvider, deserializer, _now, _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + person.Id.Value.ToString());
				var result = target.iCal(id);
				_currentPrincipalContext.AssertWasCalled(x => x.SetCurrentPrincipal(person, _dataSource, null));
				_roleToPrincipalCommand.AssertWasCalled(
					x => x.Execute(Thread.CurrentPrincipal as ITeleoptiPrincipal, _unitOfWork, _personRepository));
				result.Content.Should().Contain("DTSTART:20130708T063000Z");
				result.Content.Should().Contain("DTEND:20130708T083000Z");
			}
		}

		[Test]
		public void ShouldReturnErrorIfUrlInvalid()
		{
			using (var target = new ShareCalendarController(_repositoryFactory, _dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null, null, null, null))
			{
				var response = MockRepository.GenerateStub<FakeHttpResponse>();
				var context = new FakeHttpContext("/");
				context.SetResponse(response);
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				var id = StringEncryption.Encrypt("Main" + "/" + Guid.NewGuid().ToString()).Substring(2);
				var result = target.iCal(id);
				result.Content.Should().Contain("Invalid url");
			}
		}

		[Test]
		public void ShouldReturnNullIfPersonNotFound()
		{
			using (var target = new ShareCalendarController(_repositoryFactory, _dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null, null, null, null))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + Guid.NewGuid().ToString());
				var result = target.iCal(id);
				result.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldReturnErrorIfNoCalendarLinkPermission()
		{
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
							   .Return(false);

			using (var target = new ShareCalendarController(_repositoryFactory, _dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null, _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + person.Id.Value.ToString());
				var result = target.iCal(id);
				result.Content.Should().Contain("No permission");
			}
		}

		[Test]
		public void ShouldReturnErrorIfCalendarLinkNonActive()
		{
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										 .Return(new CalendarLinkSettings
										 {
											 IsActive = false
										 });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork)).Return(_personalSettingDataRepository);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
							   .Return(true);

			using (var target = new ShareCalendarController(_repositoryFactory, _dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null, _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + person.Id.Value.ToString());
				var result = target.iCal(id);
				result.Content.Should().Contain("inactive");
			}
		}
	}
}