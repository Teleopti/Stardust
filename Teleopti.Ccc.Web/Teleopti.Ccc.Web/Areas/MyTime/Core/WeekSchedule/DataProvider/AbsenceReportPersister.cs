using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceReportPersister : IAbsenceReportPersister
	{
		private readonly IServiceBusEventPublisher _serviceBusSender;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ILoggedOnUser _loggedOnUser;

		public AbsenceReportPersister(IServiceBusEventPublisher serviceBusSender, ICurrentBusinessUnit businessUnitProvider, ICurrentDataSource currentDataSource, INow now, ICurrentUnitOfWork currentUnitOfWork, ILoggedOnUser loggedOnUser)
		{
			_serviceBusSender = serviceBusSender;
			_businessUnitProvider = businessUnitProvider;
			_currentDataSource = currentDataSource;
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_loggedOnUser = loggedOnUser;
		}

		public AbsenceReportViewModel Persist(AbsenceReportInput input)
		{
			if (_serviceBusSender.EnsureBus())
			{
				var message = new NewAbsenceReportCreated
				{
					BusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					Datasource = _currentDataSource.Current().DataSourceName,
					AbsenceId = input.AbsenceId,
					PersonId = (Guid)_loggedOnUser.CurrentUser().Id,
					RequestedDate = input.Date.Date,
					Timestamp = _now.UtcDateTime()
				};
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _serviceBusSender.Publish(message));
			}
			return new AbsenceReportViewModel
			{
				AbsenceId = input.AbsenceId,
				ReportedDate = input.Date
			};
		}

	}

	public class AbsenceReportViewModel
	{
		public Guid AbsenceId { get; set; }
		public DateTime ReportedDate { get; set; }
	}
}