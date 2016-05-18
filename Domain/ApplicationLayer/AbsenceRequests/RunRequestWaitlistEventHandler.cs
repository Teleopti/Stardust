using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RunRequestWaitlistEventHandler : IHandleEvent<RunRequestWaitlistEvent>, IRunOnStardust
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RunRequestWaitlistEventHandler));
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAbsenceRequestWaitlistProcessor _processor;
		private readonly IWorkflowControlSetRepository _wcsRepository;

		public RunRequestWaitlistEventHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IAbsenceRequestWaitlistProcessor processor,
			IWorkflowControlSetRepository wcsRepository)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_processor = processor;
			_wcsRepository = wcsRepository;
		}

		[AsSystem]
		public virtual void Handle(RunRequestWaitlistEvent @event)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.Debug(
					"Consuming event for running request waitlist with "
					+ $"StartTime=\"{@event.StartTime}\", EndTime=\"{@event.EndTime}\" "
					+ $"(Message timestamp=\"{@event.Timestamp}\")");
			}

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var period = new DateTimePeriod(@event.StartTime, @event.EndTime);
				var workflowControlSets = _wcsRepository.LoadAll();
				foreach (var wcs in workflowControlSets)
				{
					_processor.ProcessAbsenceRequestWaitlist(uow, period, wcs);
				}
			}
		}
	}
}