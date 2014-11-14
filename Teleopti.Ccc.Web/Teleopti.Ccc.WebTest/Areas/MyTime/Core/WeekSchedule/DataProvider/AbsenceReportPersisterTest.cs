using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Requests;

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

			NewAbsenceReportCreated message;
			var serviceBusSender = mockPersistAbsenceReport(true, currentUnitOfWork, out message);

			currUow.Expect(c => c.AfterSuccessfulTx(() => serviceBusSender.Publish(message)));
		}

		[Test]
		public void ShouldNotSendMessageToBus()
		{
			NewAbsenceReportCreated message;
			var serviceBusSender = mockPersistAbsenceReport(false, null, out message);

			serviceBusSender.AssertWasNotCalled(x => x.Publish(Arg<MessageWithLogOnInfo>.Is.Anything));
		}

		private static IServiceBusEventPopulatingPublisher mockPersistAbsenceReport(bool shouldEnsureBus,
			ICurrentUnitOfWork currentUnitOfWork, out NewAbsenceReportCreated message)
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusEventPopulatingPublisher>();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(shouldEnsureBus);
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
			message = new NewAbsenceReportCreated
			{
				BusinessUnitId = buId,
				Datasource = datasource.DataSourceName,
				AbsenceId = absenceId,
				PersonId = (Guid) loggedOnUser.CurrentUser().Id,
				Timestamp = time
			};

			var target = new AbsenceReportPersister(serviceBusSender, currentBusinessUnitProvider, currentDataSourceProvider, now,
				currentUnitOfWork, loggedOnUser);
			target.Persist(form);
			return serviceBusSender;
		}
	}
}
