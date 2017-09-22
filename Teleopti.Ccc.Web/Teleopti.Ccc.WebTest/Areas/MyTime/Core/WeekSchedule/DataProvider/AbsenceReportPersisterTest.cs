using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class AbsenceReportPersisterTest
	{
		[Test]
		public void ShouldSendMessageToBus()
		{
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var currUow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(c => c.Current()).Return(currUow);

			NewAbsenceReportCreatedEvent message;
			var serviceBusSender = mockPersistAbsenceReport(currentUnitOfWork, out message);

			currUow.Expect(c => c.AfterSuccessfulTx(() => serviceBusSender.Publish(message)));
		}

		private static IEventPublisher mockPersistAbsenceReport(ICurrentUnitOfWork currentUnitOfWork, out NewAbsenceReportCreatedEvent message)
		{
			var serviceBusSender = MockRepository.GenerateMock<IEventPublisher>();
			var currentBusinessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var currentDataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();

			var form = new AbsenceReportInput();
			var absenceId = Guid.NewGuid();
			var bu = MockRepository.GenerateMock<IBusinessUnit>();
			var buId = Guid.NewGuid();
			bu.Stub(x => x.Id).Return(buId);
			currentBusinessUnitProvider.Stub(x => x.Current()).Return(bu);

			var datasource = MockRepository.GenerateMock<IDataSource>();
			datasource.Stub(x => x.DataSourceName).Return("Data Source");
			currentDataSourceProvider.Stub(x => x.Current()).Return(datasource);

			var now = MockRepository.GenerateMock<INow>();
			var time = new DateTime(2012, 05, 08, 12, 01, 01);
			now.Stub(x => x.UtcDateTime()).Return(time);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(u => u.CurrentUser().Id).Return(Guid.NewGuid());
			message = new NewAbsenceReportCreatedEvent
			{
				LogOnBusinessUnitId = buId,
				LogOnDatasource = datasource.DataSourceName,
				AbsenceId = absenceId,
				PersonId = (Guid)loggedOnUser.CurrentUser().Id,
				Timestamp = time
			};

			var target = new AbsenceReportPersister(serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now,
				currentUnitOfWork, loggedOnUser);
			target.Persist(form);
			return serviceBusSender;
		}
	}
}
