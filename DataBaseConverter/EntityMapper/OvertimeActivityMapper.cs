using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Activity=Domain.Activity;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x overtime activities
    /// </summary>
    public class OvertimeActivityMapper : Mapper<IMultiplicatorDefinitionSet, global::Domain.Overtime>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (OvertimeActivityMapper));
        private readonly IDictionary<int, Activity> _activities;

        /// <summary>
        /// Initializes a new instance of the <see cref="OvertimeActivityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="activities">The activities.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public OvertimeActivityMapper(MappedObjectPair mappedObjectPair, IDictionary<int, Activity> activities)
            : base(mappedObjectPair, null)
        {
            _activities = activities;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IMultiplicatorDefinitionSet Map(global::Domain.Overtime oldEntity)
        {
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet = null;
            
            Activity theActivity, theUnderlyingActivity;
            if (!_activities.TryGetValue(oldEntity.MapActivityId, out theUnderlyingActivity) ||
                !_activities.TryGetValue(oldEntity.Id, out theActivity))
            {
                return multiplicatorDefinitionSet;
            }

            multiplicatorDefinitionSet =
                new MultiplicatorDefinitionSet(ConversionHelper.MapString(oldEntity.Name, Description.MaxLengthOfName),
                                               MultiplicatorType.Overtime);

            if (oldEntity.Deleted)
            {
                ((IDeleteTag)multiplicatorDefinitionSet).SetDeleted();
            }

            //Make sure the activity layers with the overtime activity will get the underlying activity instead
            MappedObjectPair.OvertimeUnderlyingActivity.Add(theActivity, theUnderlyingActivity);
            //This is done to be able to find the correct overtime from activity later on
            MappedObjectPair.OvertimeActivity.Add(theActivity, oldEntity);

            //We don't want to convert overtime activities as normal activities in later steps
            _activities.Remove(oldEntity.Id); //This can mean problems!
            Logger.InfoFormat("Removed the activity {0}. Activity layers will get the activity {1} with multiplicator definition set instead.", theActivity.Name, theUnderlyingActivity.Name);
            
            return multiplicatorDefinitionSet;
        }
    }
}