using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Shake the brakes :-)
    /// </summary>
    public interface IIntradayActivityOptimizerService
    {
        /// <summary>
        /// Optimizes this instance.
        /// </summary>
		bool Optimize(IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions);
    }

    public class IntradayActivityOptimizerService : IIntradayActivityOptimizerService
    {
        private readonly IScheduleDayService _scheduleDayService;

        public IntradayActivityOptimizerService(IScheduleDayService scheduleDayService)
        {
            _scheduleDayService = scheduleDayService; 
        }

		public bool Optimize(IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions)
        {
            //spara undan dagv�rdet (RMS?)
            bool result = _scheduleDayService.RescheduleDay(scheduleDay, schedulingOptions);
            if (!result)
            {
                return false;
            }
            //kontrollera nya dagv�rdet
                //om lika returnera false
                //om s�mre, l�gg tillbaks orginalet
                    //returnera false
            //returnera true
            return true;
        }
    }
}