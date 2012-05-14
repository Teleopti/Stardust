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
            //spara undan dagvärdet (RMS?)
            bool result = _scheduleDayService.RescheduleDay(scheduleDay, schedulingOptions);
            if (!result)
            {
                return false;
            }
            //kontrollera nya dagvärdet
                //om lika returnera false
                //om sämre, lägg tillbaks orginalet
                    //returnera false
            //returnera true
            return true;
        }
    }
}