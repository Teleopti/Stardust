using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IDefinitionSetPresenter : ICommon<IDefinitionSetViewModel>, IPresenterBase
    {
        /// <summary>
        /// Adds the new definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void AddNewDefinitionSet(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Removes the definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void RemoveDefinitionSet(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Loads the model.
        /// </summary>
        void LoadModel();

        /// <summary>
        /// Sorts the model collection.
        /// </summary>
        /// <param name="mode">The mode.</param>
        void SortModelCollection(SortingMode mode);

        /// <summary>
        /// Renames the definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="newName">The new name.</param>
        void RenameDefinitionSet(IMultiplicatorDefinitionSet definitionSet, string newName);
    }
}
