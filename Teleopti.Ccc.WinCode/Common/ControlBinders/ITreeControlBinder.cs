using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCode.Common.ControlBinders
{
    /// <summary>
    /// Interface for tree controls to display tree collections.
    /// </summary>
    public interface ITreeControlBinder<TNode>
    {
        /// <summary>
        /// Displays data.
        /// </summary>
        /// <param name="expandLevel">The expand level.</param>
        void Display(int expandLevel);

        /// <summary>
        /// Gets the rootItem dataitem.
        /// </summary>
        ITreeItem<TNode> RootItem { get; set; }

        /// <summary>
        /// Gets the inner displayer control
        /// </summary>
        object TreeControl { get; }

        /// <summary>
        /// Saves the node display info.
        /// </summary>
        void SynchronizeDisplayInformation(ITreeItem<TNode> oldRootItem);
    }
}