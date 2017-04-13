using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class DefinitionSetPresenter : CommonViewHolder<IDefinitionSetViewModel>, IDefinitionSetPresenter
    {

        #region Methods - Instance Members

        #region Methods - Instance Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionSetPresenter"/> class.
        /// </summary>
        /// <param name="explorerPresenter"></param>
        public DefinitionSetPresenter(IExplorerPresenter explorerPresenter)
            : base(explorerPresenter)
        {

        }

        #endregion

        #region Methods - Instance Members - Public Methods

        /// <summary>
        /// Adds the new definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        public void AddNewDefinitionSet(IMultiplicatorDefinitionSet definitionSet)
        {
            Helper.Save(definitionSet);
            ModelCollection.Add(new DefinitionSetViewModel(definitionSet));
        }

        /// <summary>
        /// Loads the model.
        /// </summary>
        public void LoadModel()
        {
            ModelCollection.Clear();
            IList<IMultiplicatorDefinitionSet> definitionSet = Helper.LoadDefinitionSets();
            foreach (IMultiplicatorDefinitionSet set in definitionSet)
            {
                IDefinitionSetViewModel model = new DefinitionSetViewModel(set);
                ModelCollection.Add(model);
            }
        }

        /// <summary>
        /// Removes the definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        public void RemoveDefinitionSet(IMultiplicatorDefinitionSet definitionSet)
        {
            Helper.Delete(definitionSet);
            RemoveFromModelCollection(definitionSet);
        }

        /// <summary>
        /// Removes from model collection.
        /// </summary>
        private void RemoveFromModelCollection(IMultiplicatorDefinitionSet definitionSet)
        {
            foreach (IDefinitionSetViewModel model in ModelCollection)
            {
                if (model.DomainEntity.Id.Equals(definitionSet.Id))
                {
                    ModelCollection.Remove(model);
                    break;
                }
            }
        }

        /// <summary>
        /// Definitions the set comparison.
        /// </summary>
        /// <returns></returns>
        private static Comparison<IDefinitionSetViewModel> DefinitionSetComparison()
        {
            return delegate(IDefinitionSetViewModel left, IDefinitionSetViewModel right)
            {
                int value = -1;
                if (left.DomainEntity.Name != null && right.DomainEntity.Name != null)
                    value = string.Compare(left.DomainEntity.Name,
                                           right.DomainEntity.Name,
                                           StringComparison.CurrentCulture);
                return value;
            };
        }

        /// <summary>
        /// Renames the definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="newName">The new name.</param>
        public void RenameDefinitionSet(IMultiplicatorDefinitionSet definitionSet, string newName)
        {
            definitionSet.Name = newName;
        }

        #endregion

        #endregion

    }
}
