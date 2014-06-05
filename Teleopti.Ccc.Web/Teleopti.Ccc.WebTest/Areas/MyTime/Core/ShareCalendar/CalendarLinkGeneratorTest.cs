using System;
using System.Threading;
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
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.ShareCalendar
{
	[TestFixture]
	public class CalendarLinkGeneratorTest
	{
		private IRepositoryFactory _repositoryFactory;
		private IDataSource _dataSource;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;
		private IPersonRepository _personRepository;
		private IDataSourcesProvider _dataSourcesProvider;
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private ICurrentPrincipalContext _currentPrincipalContext;
		private IPersonalSettingDataRepository _personalSettingDataRepository;
		private IRoleToPrincipalCommand _roleToPrincipalCommand;
		private IPermissionProvider _permissionProvider;
		private const string dataSourceName = "Main";

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
			_dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName(dataSourceName)).Return(_dataSource);
			_personScheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			_currentPrincipalContext = MockRepository.GenerateMock<ICurrentPrincipalContext>();
			_personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
		}

		[Test]
		public void ShouldGetCalendarForPerson()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(DateOnly.Today.AddDays(181));
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
							Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model
								{
									Shift = new Shift
										{
											Projection = new[]
												{
													new SimpleLayer
														{
															Start = new DateTime(2013, 7, 8, 6, 30, 0, DateTimeKind.Utc),
															End = new DateTime(2013, 7, 8, 8, 30, 0, DateTimeKind.Utc),
														}
												}
										}
								}),
						}
				});
			_repositoryFactory.Stub(x => x.CreatePersonScheduleDayReadModelFinder(_unitOfWork)).Return(_personScheduleDayReadModelFinder);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
			                   .Return(true);

			var deserializer = new NewtonsoftJsonDeserializer();
			var calendarlinkid = new CalendarLinkId
				{
					DataSourceName = dataSourceName,
					PersonId = person.Id.Value
				};

			var target = new CalendarLinkGenerator(_repositoryFactory, _dataSourcesProvider, deserializer, new Now(), 
			                                       _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider
				);
			var result = target.Generate(calendarlinkid);

			_currentPrincipalContext.AssertWasCalled(x => x.SetCurrentPrincipal(person, _dataSource, null));
			_roleToPrincipalCommand.AssertWasCalled(
				x => x.Execute(Thread.CurrentPrincipal as ITeleoptiPrincipal, _unitOfWorkFactory, _personRepository));
			result.Should().Contain("DTSTART:20130708T063000Z");
			result.Should().Contain("DTEND:20130708T083000Z");
		}

		[Test]
		public void ShouldNotGetUnpublishedCalendarForPerson()
		{
			var publishedToDate = DateOnly.Today.AddDays(30);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(publishedToDate);
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										  .Return(new CalendarLinkSettings
										  {
											  IsActive = true
										  });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork)).Return(_personalSettingDataRepository);
			_repositoryFactory.Stub(x => x.CreatePersonScheduleDayReadModelFinder(_unitOfWork)).Return(_personScheduleDayReadModelFinder);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
							   .Return(true);
			var deserializer = new NewtonsoftJsonDeserializer();
			var target = new CalendarLinkGenerator(_repositoryFactory, _dataSourcesProvider, deserializer, new Now(),
												   _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider
				);
			var calendarlinkid = new CalendarLinkId
			{
				DataSourceName = dataSourceName,
				PersonId = person.Id.Value
			};
			var startDate = DateOnly.Today.AddDays(-60);
			_personScheduleDayReadModelFinder.Stub(x => x.ForPerson(startDate, publishedToDate, person.Id.Value)).Return(new PersonScheduleDayReadModel[] { });

			target.Generate(calendarlinkid);

			_personScheduleDayReadModelFinder.AssertWasNotCalled(x => x.ForPerson(startDate, DateOnly.Today.AddDays(180), person.Id.Value));
			_personScheduleDayReadModelFinder.AssertWasCalled(x => x.ForPerson(startDate, publishedToDate, person.Id.Value));
		}

		[Test, ExpectedException(typeof(PermissionException))]
		public void ShouldThrowIfNoCalendarLinkPermission()
		{
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
			                   .Return(false);

			var target = new CalendarLinkGenerator(_repositoryFactory, _dataSourcesProvider,
			                                       new NewtonsoftJsonDeserializer(), null,
			                                       _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider);
			var calendarlinkid = new CalendarLinkId
				{
					DataSourceName = dataSourceName,
					PersonId = person.Id.Value
				};
			target.Generate(calendarlinkid);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShouldThrowIfCalendarLinkNonActive()
		{
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			_personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
			                              .Return(new CalendarLinkSettings
				                              {
					                              IsActive = false
				                              });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork))
			                  .Return(_personalSettingDataRepository);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
			                   .Return(true);

			var calendarlinkid = new CalendarLinkId
				{
					DataSourceName = dataSourceName,
					PersonId = person.Id.Value
				};

			var target = new CalendarLinkGenerator(_repositoryFactory, _dataSourcesProvider,
			                                       new NewtonsoftJsonDeserializer(), null,
			                                       _currentPrincipalContext, _roleToPrincipalCommand, _permissionProvider
				);
			target.Generate(calendarlinkid);
		}
	}
}