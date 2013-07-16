using System;
using System.Dynamic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ShareCalendarControllerTest
	{
		[Test]
		public void ShouldGetCalendarForPerson()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowf = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			dataSource.Stub(x => x.Application).Return(uowf);
			uowf.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
			                             .Return(new CalendarLinkSettings
				                             {
					                             IsActive = true
				                             });
			repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(uow)).Return(personalSettingDataRepository);
			var finder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			finder.Stub(x => x.ForPerson(DateOnly.Today.AddDays(-60), DateOnly.Today.AddDays(180), person.Id.Value)).Return(new[]
				{
					new PersonScheduleDayReadModel
						{
							Shift ="{Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Phone'}]}"
						}
				});
			repositoryFactory.Stub(x => x.CreatePersonScheduleDayReadModelFinder(uow)).Return(finder);
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName("Main")).Return(dataSource);

			var deserializer = new NewtonsoftJsonDeserializer<ExpandoObject>();
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.DateOnly()).Return(DateOnly.Today);
			using (var target = new ShareCalendarController(repositoryFactory, dataSourcesProvider, deserializer, now))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + person.Id.Value.ToString());
				var result = target.iCal(id);
				result.Content.Should().Contain("DTSTART:20130708T063000Z");
				result.Content.Should().Contain("DTEND:20130708T083000Z");
			}
		}

		[Test]
		public void ShouldReturnNulIfPersonNotFound()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowf = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			dataSource.Stub(x => x.Application).Return(uowf);
			uowf.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName("Main")).Return(dataSource);

			using (var target = new ShareCalendarController(repositoryFactory, dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + Guid.NewGuid().ToString());
				var result = target.iCal(id);
				result.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldReturnNulIfCalendarLinkNonActive()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowf = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			dataSource.Stub(x => x.Application).Return(uowf);
			uowf.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName("Main")).Return(dataSource);

			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			personalSettingDataRepository.Stub(
				x => x.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person, new CalendarLinkSettings())).IgnoreArguments()
										 .Return(new CalendarLinkSettings
										 {
											 IsActive = false
										 });
			repositoryFactory.Stub(x => x.CreatePersonalSettingDataRepository(uow)).Return(personalSettingDataRepository);

			using (var target = new ShareCalendarController(repositoryFactory, dataSourcesProvider, new NewtonsoftJsonDeserializer<ExpandoObject>(), null))
			{
				var id = StringEncryption.Encrypt("Main" + "/" + person.Id.Value.ToString());
				var result = target.iCal(id);
				result.Should().Be.Null();
			}
		}
	}
}