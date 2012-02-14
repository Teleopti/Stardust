using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Used to get all projections out of ScheduleDictionary, regardless of user rights
    /// </summary>
    public sealed class ScheduleProjectionExtractor : IScheduleExtractor
    {
        readonly IList<IVisualLayerCollection> retList = new List<IVisualLayerCollection>();


        /// <summary>
        /// Creates the relevant projection list.
        /// </summary>
        /// <param name="scheduleDictionary">The schedule dictionary.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public IList<IVisualLayerCollection> CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary)
        {
            retList.Clear();
#pragma warning disable 618
            scheduleDictionary.ExtractAllScheduleData(this);
#pragma warning restore 618
            return retList;
        }

        /// <summary>
        /// Creates the relevant projection list.
        /// </summary>
        /// <param name="scheduleDictionary">The schedule dictionary.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public IList<IVisualLayerCollection> CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary, DateTimePeriod period)
        {
            retList.Clear();
            scheduleDictionary.ExtractAllScheduleData(this, period);
            return retList;
        }

        // just returns collections with layers
        public IList<IVisualLayerCollection> CreateRelevantProjectionWithScheduleList(IScheduleDictionary scheduleDictionary, DateTimePeriod period)
        {
            CreateRelevantProjectionList(scheduleDictionary, period);
            
            return retList;
        }

        void IScheduleExtractor.AddSchedulePart(IScheduleDay schedulePart)
        {
            IProjectionService svc = schedulePart.ProjectionService();
            var projection = svc.CreateProjection();
            if (projection.HasLayers)
            {
                retList.Add(projection);
            }
        }
    }
}
