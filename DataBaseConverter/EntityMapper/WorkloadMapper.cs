using System;
using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// WorkLoadMapper
    /// </summary>
    public class WorkloadMapper : Mapper<IWorkload, global::Domain.Forecast>
    {
        private readonly IDictionary<int, global::Domain.IntegerPair> _queueSourceMap;
        private readonly ISkill _skill;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="queueSourceMap">The queue source map.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-29
        /// </remarks>
        public WorkloadMapper(MappedObjectPair mappedObjectPair, ISkill skill, IDictionary<int, global::Domain.IntegerPair> queueSourceMap)
            : base(mappedObjectPair, null)
        {
            _skill = skill;
            _queueSourceMap = queueSourceMap;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-30
        /// </remarks>
        public override IWorkload Map(global::Domain.Forecast oldEntity)
        {
            IWorkload newWorkload = null;
            string oldName = oldEntity.Name;
            string oldDescription = oldEntity.Description;

            while (newWorkload == null)
            {
                try
                {
                    newWorkload = new Workload(_skill);
                    newWorkload.Description = oldDescription;
                    newWorkload.Name = oldName;
                }
                catch (ArgumentException)
                {
                    oldName = oldName.Remove(oldName.Length - 1);
                    //oldDescription = oldDescription.Remove(oldDescription.Length - 1);
                    newWorkload = null;
                }
            }

            //This will add the forecast id:s to the new Workloads
            //Add the QueueSource to the correct Workload
            mapWorkloadQueues(oldEntity, newWorkload);

            mapQueueAdjustments(oldEntity, newWorkload);

            return newWorkload;
        }

        private static void mapQueueAdjustments(Forecast oldEntity, IWorkload newWorkload)
        {
            var overflowOut = oldEntity.ActOflOutPercent/ 100d;
            var overflowIn = oldEntity.ActOflInPercent/100d;
            const double offered = 1d;
            const double abandoned = -1d;
            var abandonedShort = oldEntity.ActshortAbnPercent/100d;
            var abandonedWithinSL = oldEntity.ActInSlAbnPercent/100d;
            var abandonedAfterSL = oldEntity.ActOverSlAbnPercent/100d;

            newWorkload.QueueAdjustments = new QueueAdjustment
                                               {
                                                   AbandonedAfterServiceLevel = new Percent(abandonedAfterSL),
                                                   AbandonedShort = new Percent(abandonedShort),
                                                   AbandonedWithinServiceLevel = new Percent(abandonedWithinSL),
                                                   OfferedTasks = new Percent(offered),
                                                   Abandoned = new Percent(abandoned),
                                                   OverflowIn = new Percent(overflowIn),
                                                   OverflowOut = new Percent(overflowOut)
                                               };
        }

        private void mapWorkloadQueues(Forecast oldEntity, IWorkload newWorkload)
        {
            foreach (KeyValuePair<int, global::Domain.IntegerPair> pair in _queueSourceMap)
            {
                //Ok, first find the correct forecastid
                if (pair.Value.IntegerValue1 == oldEntity.Id)
                {
                    //Add the queue from the uniqe id list
                    newWorkload.AddQueueSource(MappedObjectPair.QueueSource.GetPaired(pair.Value.IntegerValue2));
                }
            }
        }
    }
}