using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting shiftCategories from old version to new version
    /// </summary>
    public class SkillMapper : Mapper<ISkill, global::Domain.Skill>
    {
        private int _intervalLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-29
        /// </remarks>
        public SkillMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone, int intervalLength)
            : base(mappedObjectPair, timeZone)
        {
            _intervalLength = intervalLength;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-29
        /// </remarks>
        public override ISkill Map(global::Domain.Skill oldEntity)
        {
            ISkillType skillType = MappedObjectPair.SkillType.GetPaired(oldEntity.TypeOfSkill);
      
            string oldName = oldEntity.Name;
            if (oldName.Length == 0)
                oldName = " - ";
            string oldDesc = oldEntity.Description;
            ISkill newSkill = new Skill("", "", oldEntity.DisplayColor, _intervalLength, skillType);
            newSkill.Priority = oldEntity.Priority + 1;
            bool flgOk = false;
            while (!flgOk)
            {
                try
                {
                    newSkill.Name = oldName;
                    flgOk = true;
                }
                catch (ArgumentException)
                {
                    oldName = oldName.Remove(oldName.Length - 1);
                }
            }
            flgOk = false;
            while (!flgOk)
            {
                try
                {
                    newSkill.Description = oldDesc;
                    flgOk = true;
                }
                catch (ArgumentException)
                {
                    oldDesc = oldDesc.Remove(oldDesc.Length - 1);
                }
            }
            newSkill.Activity = MappedObjectPair.ResolveActivity(oldEntity.SkillActivity);
            newSkill.TimeZone = TimeZone;
            if (oldEntity.Deleted)
                ((IDeleteTag)newSkill).SetDeleted();

            return newSkill;
        }
    }
}