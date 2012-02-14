using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>
    public class SkillAgentDataMapper : Mapper<SkillPersonData, global::Domain.SkillData>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillAgentDataMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public SkillAgentDataMapper()
            : base(new MappedObjectPair(), null)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public override SkillPersonData Map(global::Domain.SkillData oldEntity)
        {
            SkillPersonData newSkillPersonData = new SkillPersonData();

            int max = oldEntity.MaxStaff;
            if(oldEntity.MaxStaff < oldEntity.MinStaff)
            {
                max = int.MaxValue; 
            }
           
            newSkillPersonData.PersonCollection = new MinMax<int>(oldEntity.MinStaff, max);
           
            return newSkillPersonData;
        }
    }
}