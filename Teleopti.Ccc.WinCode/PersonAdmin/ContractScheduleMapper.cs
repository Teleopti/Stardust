using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Presentation;

namespace Teleopti.Ccc.WinCode.PersonAdmin
{
    /// <summary>
    /// This is simple mapper class for mapping contract schedule to grid grouping grid.Fix for grid grouping control drop down error.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-04-17
    /// </remarks>
    public class ContractScheduleMapper : EntityContainer<ContractSchedule>
    {

        /// <summary>
        /// Gets the unique id for this entity.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-04-17
        /// </remarks>
        public new Guid? Id
        {
            get { return ContainedEntity.Id; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-04-17
        /// </remarks>
        public String Name
        {
            get { return ContainedEntity.Description.Name; }
        }
    }
}