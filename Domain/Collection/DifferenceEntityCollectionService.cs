using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    /// <summary>
    /// Calculates the difference between two collections of IEntity data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-29
    /// </remarks>
    public class DifferenceEntityCollectionService<T> : IDifferenceCollectionService<T> where T : class, IEntity
    {
        public IDifferenceCollection<T> Difference(IList<T> original, IList<T> actual)
        {
            var res = new DifferenceCollection<T>();
            foreach (var originalItem in original)
            {
                if(actual.Contains(originalItem))
                {
                    var actualItem = actual[actual.IndexOf(originalItem)];
                    if(actualItem!=originalItem)
                        res.Add(new DifferenceCollectionItem<T>(originalItem, actualItem));
                }
                else
                {
                    res.Add(new DifferenceCollectionItem<T>(originalItem, null));
                }
            }
            foreach (var actualItem in actual)
            {
                if(!original.Contains(actualItem))
                {
                    res.Add(new DifferenceCollectionItem<T>(null, actualItem));
                }
            }
            return res;
        }
    }
}