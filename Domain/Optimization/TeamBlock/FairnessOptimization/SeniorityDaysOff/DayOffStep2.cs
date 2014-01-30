namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IDayOffStep2
    {
        void PerformStep2();
    }

    public class DayOffStep2:IDayOffStep2
    {
    
        public void PerformStep2()
        {
            //get the high day among a week 
            //get the high agent
            //get the agent with low sen
            //try swap if works on the same day between B and A on day D1
            //find a day < D1 and swap between A and B
            //if period still in illegal state rollback
            //if doesnt work pick the next senior agent
        }
    }
}
