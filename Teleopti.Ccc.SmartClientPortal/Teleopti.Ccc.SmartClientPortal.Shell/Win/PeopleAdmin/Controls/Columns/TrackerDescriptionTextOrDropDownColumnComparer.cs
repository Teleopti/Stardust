using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{

    /// <summary>
    /// Represents the comparer for tracker description column.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 8/27/2008
    /// </remarks>
    public class ChildTrackerDescriptionTextOrDropDownColumnComparer :
        ITextOrDropDownColumnComparer<IPersonAccountChildModel>
    {
        /// <summary>
        /// Compares the specified data item.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public bool Compare(IPersonAccountChildModel dataItem)
        {
            IEntity containedEntity = dataItem.ContainedEntity;
            return (containedEntity != null) && (containedEntity.Id == null);
        }
    }

    public class ParentTrackerDescriptionTextOrDropDownColumnComparer :
        ITextOrDropDownColumnComparer<IPersonAccountModel>
    {
        /// <summary>
        /// Compares the specified data item.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public bool Compare(IPersonAccountModel dataItem)
        {
            IEntity entityItem = dataItem.CurrentAccount;
            return (entityItem != null) && (entityItem.Id == null);
        }
    }
}
