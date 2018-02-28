using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IActivityChangeCheckerFromScheduleChangeProcessor
	{
		void CheckForActivityChanges();
	}

	public class DontCheckForActivityChangesFromScheduleChangeProcessor : IActivityChangeCheckerFromScheduleChangeProcessor
	{
		public void CheckForActivityChanges()
		{
		}
	}

	public class ActivityChangeChecker : IActivityChangeCheckerFromScheduleChangeProcessor
	{
		private readonly IContextLoader _contextLoader;
		private readonly IRtaTracer _tracer;

		public ActivityChangeChecker(IContextLoader contextLoader, IRtaTracer tracer)
		{
			_contextLoader = contextLoader;
			_tracer = tracer;
		}

		public void CheckForActivityChanges()
		{
			_tracer.ProcessActivityCheck();
			_contextLoader.ForActivityChanges();
		}
	}
}