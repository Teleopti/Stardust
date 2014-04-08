using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x activities
    /// </summary>
    public class ActivityMapper : Mapper<IActivity, global::Domain.Activity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ActivityMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IActivity Map(global::Domain.Activity oldEntity)
        {
            if (absenceActivity(oldEntity)) return null;

            string oldName = ConversionHelper.MapString(oldEntity.Name,Description.MaxLengthOfName);
            var newAct = new Activity(oldName);
            newAct.DisplayColor = oldEntity.ColorLayout;
            newAct.RequiresSkill = oldEntity.RequiresSkill;
            newAct.InContractTime = oldEntity.InWorkTime;
            newAct.InReadyTime = oldEntity.CtiActivity;
            newAct.InWorkTime = oldEntity.InWorkTime;
            newAct.InPaidTime = oldEntity.PaidTime;
            if (oldEntity.IsShortBreak)
                newAct.ReportLevelDetail = ReportLevelDetail.ShortBreak;
            if (oldEntity.IsLunchBreak)
                newAct.ReportLevelDetail = ReportLevelDetail.Lunch;

            if (oldEntity.Deleted)
                newAct.SetDeleted();
            return newAct;
        }

        /// <summary>
        /// Checks if this activity have been used as intra day absence in old system
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-07-18
        /// </remarks>
        private bool absenceActivity(global::Domain.Activity oldEntity)
        {
            return MappedObjectPair.AbsenceActivity.Obj2Collection().Contains(oldEntity);
        }
    }
}