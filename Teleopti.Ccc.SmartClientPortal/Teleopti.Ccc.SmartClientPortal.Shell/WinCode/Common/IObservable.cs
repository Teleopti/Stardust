namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{

    /// <summary>
    /// Notifies by calling Notify with parameter T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// Notifies the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void Notify(T item);
    }
}
