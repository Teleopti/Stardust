namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Reversed from FilteredCollection
    /// Shows Only the typ/types in the filter 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-09-09
    /// </remarks>
    public class ShowOnlyCollection<T> : FilteredCollection<T>
    {
        protected override bool FilterOutType(object item)
        {
            return !base.FilterOutType(item);
        }
    }
}
