using System;
using Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// TaskMapper class
    /// </summary>
    public class TaskMapper : Mapper<Task, ForecastData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public TaskMapper() : base(new MappedObjectPair(), null)
        {
        }


        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override Task Map(ForecastData oldEntity)
        {
            double tasks = oldEntity.Tasks;
            double aht = oldEntity.AvgHandlingSec;
            if (aht < 1)
                aht = 1;
            double awt = oldEntity.AvgWrapupSec;
            if (awt < 1)
                awt = 1;

            Task newTask = new Task(tasks,TimeSpan.FromSeconds(aht),TimeSpan.FromSeconds(awt));

            return newTask;
        }
    }
}