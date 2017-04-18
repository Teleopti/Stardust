using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    public interface IMultiplicatorDefinitionPresenter : ICommon<IMultiplicatorDefinitionViewModel>, IPresenterBase
    {
        /// <summary>
        /// Loads the weekday multiplicator definitions.
        /// </summary>
        void LoadMultiplicatorDefinitions();


        /// <summary>
        /// Deletes the selected MultiplicatorDefinition.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="multiplicatorDefinition">The multiplicator definition.</param>
        /// <returns>Returns the Orderindex of the definitionset collection.</returns>
        int DeleteSelected(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition);


        /// <summary>
        /// Adds the new MultiplicatorDefinition at specified OrderIndex. Properties comes in array of strings. Primary used in copy/paste operations.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="properties">Array of string for all properties of the MultiplicatorDefinition.</param>
        /// <param name="orderIndex">Index of the order.</param>
        /// <param name="multiplicatorDefinitionTypeCollection"></param>
        void AddNewAt(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinitionViewModel viewModelToCopy, int orderIndex, IList<IMultiplicatorDefinitionAdapter> multiplicatorDefinitionTypeCollection);
        
        /// <summary>
        /// Adds the new day of week.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void AddNewDayOfWeek(IMultiplicatorDefinitionSet definitionSet);


        /// <summary>
        /// Adds the new day of week with a specified OrderIndex.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="orderIndex">Index of the order.</param>
        /// <param name="multiplicator">Multiplicator</param>
        void AddNewDayOfWeekAt(IMultiplicatorDefinitionSet definitionSet, int orderIndex, IMultiplicator multiplicator);

        /// <summary>
        /// Adds the new date time.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void AddNewDateTime(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Adds the new date time with a specified OrderIndex.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="orderIndex">Index of the order.</param>
        /// <param name="multiplicator">Multiplicator</param>
        void AddNewDateTimeAt(IMultiplicatorDefinitionSet definitionSet, int orderIndex, IMultiplicator multiplicator);

        /// <summary>
        /// Moves up.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="multiplicatorDefinition">The multiplicator definition.</param>
        void MoveUp(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition);

        /// <summary>
        /// Moves down.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="multiplicatorDefinition">The multiplicator definition.</param>
        void MoveDown(IMultiplicatorDefinitionSet definitionSet, IMultiplicatorDefinition multiplicatorDefinition);

        /// <summary>
        /// Sorts the specified mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        void Sort(SortingMode mode);

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        void RefreshView();

        /// <summary>
        /// Builds the copy string of the viewModelCollection.
        /// </summary>
        /// <param name="viewModelCollection">The view model collection.</param>
        /// <returns></returns>
        string BuildCopyString(IList<IMultiplicatorDefinitionViewModel> viewModelCollection);

        /// <summary>
        /// Updates the multiplicator collection upon multiplicator changes.
        /// </summary>
        /// <param name="e">The e.</param>
        void UpdateMultiplicatorCollectionUponMultiplicatorChanges(EventMessageArgs e);
    }
}
