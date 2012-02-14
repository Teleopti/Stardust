namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Used for roots that should not
    /// be deleted from db but only marked as
    /// deleted.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-13
    /// </remarks>
    public interface IDeleteTag
    {
        /// <summary>
        /// Gets a value indicating whether root is deleted or not.
        /// </summary>
        /// <value><c>true</c> if root is deleted; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-13
        /// </remarks>
        bool IsDeleted { get; }

        /// <summary>
        /// Sets the deleted flag.
        /// Note: Should not be used from client side. Use [repository].remove instead!
        /// Note: Should therefore be explicitly implemented on object
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-13
        /// </remarks>
        void SetDeleted();
    }
}