using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Helper functionalities to TreeItem.
    /// </summary>
    public static class TreeItemEntityHelper
    {

        /// <summary>
        /// Creates a tree item.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        public static TreeItem<TreeNode> CreateTreeItem(IEntity entity)
        {
            // try IListBoxPresenter
            IListBoxPresenter presenter = entity as IListBoxPresenter;
            if (presenter != null)
                return new TreeItem<TreeNode>(presenter.Id.ToString(), presenter.DataBindDescriptionText, presenter.DataBindText, presenter.EntityObject);

            // try IAuthorizationEntity
            IAuthorizationEntity authorizationEntity = entity as IAuthorizationEntity;
            if (authorizationEntity != null)
            {
                IListBoxPresenter authorizationPresenter = new AuthorizationEntityListBoxPresenter(authorizationEntity);
                return new TreeItem<TreeNode>(authorizationPresenter.Id.ToString(), authorizationPresenter.DataBindDescriptionText, authorizationPresenter.DataBindText, entity);
            }

            // IEntity
            TreeItem<TreeNode> ret = new TreeItem<TreeNode>(entity.Id.ToString());
            ret.Data = entity;
            return ret;
        }

        /// <summary>
        /// Transforms the entity to tree structure.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        public static TreeItem<TreeNode> TransformEntityCollectionToTreeStructure(ICollection<IParentChildEntity> list)
        {
            TreeItem<TreeNode> rootTreeItem;
            IParentChildEntity rootEntity = FindRootEntity(list);
            // if there is no root item specified (the item that has no parents) 
            // then return null
            if (rootEntity == null)
                return null;

            rootTreeItem = CreateTreeItem(rootEntity);

            DigestEntityItem(rootTreeItem, rootEntity);

            return rootTreeItem;

        }

        /// <summary>
        /// Digests the entity item.
        /// </summary>
        /// <param name="parentTreeItem">The parent tree item.</param>
        /// <param name="parentEntity">The parent entity.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        private static void DigestEntityItem(TreeItem<TreeNode> parentTreeItem, IParentChildEntity parentEntity)
        {
            foreach (IParentChildEntity childItem in parentEntity.ChildCollection)
            {
                TreeItem<TreeNode> newTreeItem;

                newTreeItem = CreateTreeItem(childItem);

                parentTreeItem.AddChild(newTreeItem);
                DigestEntityItem(newTreeItem, childItem);
            }
        }

        /// <summary>
        /// Finds the root entity.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        private static IParentChildEntity FindRootEntity(IEnumerable<IParentChildEntity> list)
        {
            foreach (IParentChildEntity item in list)
            {
                if (item.Parent == null)
                    return item;
            }
            return null;
        }
    }
}