using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceReportPersister : IAbsenceReportPersister
	{
		private readonly IEventPublisher _publisher;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ILoggedOnUser _loggedOnUser;

		public AbsenceReportPersister(IEventPublisher publisher, ICurrentBusinessUnit businessUnitProvider, ICurrentDataSource currentDataSource, INow now, ICurrentUnitOfWork currentUnitOfWork, ILoggedOnUser loggedOnUser)
		{
			_publisher = publisher;
			_businessUnitProvider = businessUnitProvider;
			_currentDataSource = currentDataSource;
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_loggedOnUser = loggedOnUser;
		}

		public AbsenceReportViewModel Persist(AbsenceReportInput input)
		{
			var message = new NewAbsenceReportCreatedEvent
			{
				LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
				LogOnDatasource = _currentDataSource.Current().DataSourceName,
				AbsenceId = input.AbsenceId,
				PersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				RequestedDate = input.Date.Date,
				Timestamp = _now.UtcDateTime()
			};
			_currentUnitOfWork.Current().AfterSuccessfulTx(() => _publisher.Publish(message));
			return new AbsenceReportViewModel
			{
				AbsenceId = input.AbsenceId,
				ReportedDate = input.Date.Date
			};
		}

	}

	public class AbsenceReportViewModel
	{
		public Guid AbsenceId { get; set; }
		public DateTime ReportedDate { get; set; }
	}
}