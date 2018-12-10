using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;


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
		private ICheckCalendarPermissionCommand _checkCalendarPermissionCommand;
		private ICheckCalendarActiveCommand _checkCalendarActiveCommand;
		private IFindSharedCalendarScheduleDays _findSharedCalendarScheduleDays;
		private IDataSourceScope _dataSourceScope;

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
			_checkCalendarPermissionCommand = MockRepository.GenerateMock<ICheckCalendarPermissionCommand>();
			_checkCalendarActiveCommand = MockRepository.GenerateMock<ICheckCalendarActiveCommand>();
			_findSharedCalendarScheduleDays = MockRepository.GenerateMock<IFindSharedCalendarScheduleDays>();
			_dataSourceScope = MockRepository.GenerateMock<IDataSourceScope>();
		}

		[Test]
		public void ShouldGetCalendarForPerson()
		{
			const string dataSourceName = "Main";
			var applicationData = MockRepository.GenerateMock<IDataSourceForTenant>();
			applicationData.Stub(x => x.Tenant(dataSourceName)).Return(_dataSource);

			var calendarTransformer = MockRepository.GenerateMock<ICalendarTransformer>();

			var scheduledTo = DateTime.Today.AddDays(181);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduledTo));
			_personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			var models = new[]
			{
				new PersonScheduleDayReadModel
				{
					Model =
						Newtonsoft.Json.JsonConvert.SerializeObject(new Model
						{
							Shift =
								new Shift
								{
									Projection =
										new[]
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
			};

			var calendarlinkid = new CalendarLinkId
			{
				DataSourceName = dataSourceName,
				PersonId = person.Id.GetValueOrDefault()
			};

			_findSharedCalendarScheduleDays.Stub(x => x.GetScheduleDays(calendarlinkid, _unitOfWork, scheduledTo)).Return(models);
			calendarTransformer.Stub(x => x.Transform(models)).Return("success!");

			var target = new CalendarLinkGenerator(_repositoryFactory, applicationData, calendarTransformer,
				_findSharedCalendarScheduleDays, _checkCalendarPermissionCommand, _checkCalendarActiveCommand, _dataSourceScope);
			target.Generate(calendarlinkid).Should().Be.EqualTo("success!");

			_checkCalendarActiveCommand.AssertWasCalled(x => x.Execute(_unitOfWork, person));
			_checkCalendarPermissionCommand.AssertWasCalled(x => x.Execute(_dataSource, person, _personRepository));
		}

		[Test]
		public void ShouldTransformCalendar()
		{
			var target = new CalendarTransformer(new NewtonsoftJsonSerializer());
			var result = target.Transform(new[]
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

			result.Should().Contain("DTSTART:20130708T063000Z");
			result.Should().Contain("DTEND:20130708T083000Z");
		}

		[Test]
		public void ShouldTransformCalendarWithoutShift()
		{
			var target = new CalendarTransformer(new NewtonsoftJsonSerializer());
			var result = target.Transform(new[]
			{
				new PersonScheduleDayReadModel
				{
					Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model
					{
						Shift = null
					}),
				}
			});

			result.Should().Contain("Teleopti");
		}

		[Test]
		public void ShouldTransformCalendarWithoutModel()
		{
			var target = new CalendarTransformer(new NewtonsoftJsonSerializer());
			var result = target.Transform(new[]
			{
				new PersonScheduleDayReadModel
				{
					Model = null,
				}
			});

			result.Should().Contain("Teleopti");
		}

		[Test]
		public void ShouldNotGetUnpublishedCalendarForPerson()
		{
			var personScheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();

			const string dataSourceName = "Main";
			var publishedToDate = DateOnly.Today.AddDays(30);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(publishedToDate);
			_personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										  .Return(new CalendarLinkSettings
										  {
											  IsActive = true
										  });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork)).Return(personalSettingDataRepository);
			_repositoryFactory.Stub(x => x.CreatePersonScheduleDayReadModelFinder(_unitOfWork)).Return(personScheduleDayReadModelFinder);

			var target = new FindSharedCalendarScheduleDays(_repositoryFactory, new Now());
			var calendarlinkid = new CalendarLinkId
			{
				DataSourceName = dataSourceName,
				PersonId = person.Id.GetValueOrDefault()
			};
			var startDate = DateOnly.Today.AddDays(-60);
			personScheduleDayReadModelFinder.Stub(x => x.ForPerson(new DateOnlyPeriod(startDate, publishedToDate), person.Id.GetValueOrDefault())).Return(new PersonScheduleDayReadModel[] { });

			target.GetScheduleDays(calendarlinkid, _unitOfWork, publishedToDate.Date);

			personScheduleDayReadModelFinder.AssertWasNotCalled(x => x.ForPerson(new DateOnlyPeriod(startDate, DateOnly.Today.AddDays(180)), person.Id.GetValueOrDefault()));
			personScheduleDayReadModelFinder.AssertWasCalled(x => x.ForPerson(new DateOnlyPeriod(startDate, publishedToDate), person.Id.GetValueOrDefault()));
		}

		[Test]
		public void ShouldThrowIfCalendarLinkNonActive()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										  .Return(new CalendarLinkSettings
										  {
											  IsActive = false
										  });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork))
							  .Return(personalSettingDataRepository);

			var target = new CheckCalendarActiveCommand(_repositoryFactory);
			Assert.Throws<InvalidOperationException>(() => target.Execute(_unitOfWork, person));
		}

		[Test]
		public void ShouldPassIfCalendarLinkIsActive()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			_personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										  .Return(new CalendarLinkSettings
										  {
											  IsActive = true
										  });
			_repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(_unitOfWork))
							  .Return(personalSettingDataRepository);

			var target = new CheckCalendarActiveCommand(_repositoryFactory);
			target.Execute(_unitOfWork, person);
		}

		[Test]
		public void ShouldSerializeMidnightTimeAsUtcDateTimeString()
		{
			var target = new CalendarTransformer(new NewtonsoftJsonSerializer());
			var result = target.Transform(new[]
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
									End = new DateTime(2013, 7, 9, 00, 00, 0, DateTimeKind.Utc),
								}
							}
						}
					}),
				}
			});

			result.Should().Contain("DTSTART:20130708T063000Z");
			result.Should().Contain("DTEND:20130709T000000Z");
		}
	}
}