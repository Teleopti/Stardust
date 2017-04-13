using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
        /// Renames the definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="newName">The new name.</param>
        void RenameDefinitionSet(IMultiplicatorDefinitionSet definitionSet, string newName);
    }
}
