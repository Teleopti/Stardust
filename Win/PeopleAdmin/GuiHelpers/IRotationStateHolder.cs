using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
    public interface IRotationStateHolder
    {
        #region Methods - Instance Members

        /// <summary>
        /// Adds a person rotation to the specified parent row
        /// </summary>
        /// <param name="parentRowIndex"></param>
        void AddPersonRotation(int parentRowIndex );

        /// <summary>
        /// Deletes all person rotations in the specified parent row index
        /// </summary>
        /// <param name="parentRowIndex">The parent row index who's children are to be deleted</param>
        void DeleteAllChildPersonRotations(int parentRowIndex );

        /// <summary>
        /// Deletes person from the specified parent row and in the specified child index
        /// </summary>
        /// <param name="parentRowIndex">The parent row index</param>
        /// <param name="childRowIndex">The child row index</param>
        void DeletePersonRotation(int parentRowIndex, int childRowIndex );

        void DeletePersonRotation(int rowIndex);

        /// <summary>
        /// Deletes person from the specified parent row 
        /// </summary>
        /// <param name="rowIndex">The parent row index</param>
        void DeleteAllPersonRotation(int rowIndex);

        /// <summary>
        /// Get all child rotations for the specified parent ro index
        /// </summary>
        /// <param name="rowIndex">The parent row index</param>
        void GetChildPersonRotations(int rowIndex );

        /// <summary>
        /// Gets the child person rotations.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-17
        /// </remarks>
        void GetChildPersonRotations(int rowIndex, GridControl grid);

        /// <summary>
        /// Resets the parent item upon addition of anew item or when an existing child item is updated
        /// </summary>
        /// <param name="rowIndex">The parent rown index</param>
        void GetParentPersonRotationWhenAddedOrUpdated(int rowIndex );

        /// <summary>
        /// Resets the the parent item when a item is deleted
        /// </summary>
        /// <param name="rowIndex"></param>
        void GetParentPersonRotationWhenDeleted(int rowIndex );

        void SetChildrenPersonRotationCollection(object childrenPersonRotationCollection);

        object GetCurrentEntity(GridControl grid, int gridInCellColumnIndex);

		FilteredPeopleHolder FilteredStateHolder { get; set; }
        #endregion
    }
}
