using System;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
    public class RunWaitlistCommandHandler : IHandleCommand<RunWaitlistCommand>
    {
	    private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;

        public RunWaitlistCommandHandler(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository)
        {
	        _queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
        }

	    public void Handle(RunWaitlistCommand command)
	    {
		    foreach (var day in command.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection())
		    {
			    var queuedAbsenceRequest = new QueuedAbsenceRequest
			    {
					PersonRequest = Guid.Empty,
					Created = DateTime.UtcNow,
					StartDateTime = day.Date,
					EndDateTime = day.Date.AddDays(1).AddSeconds(-1)
			    };
			    _queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
		    }
		}
	}
}