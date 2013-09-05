﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    /// <summary>
    /// A wrapped collection of DifferenceCollectionItem of T:s
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-29
    /// </remarks>
    public class DifferenceCollection<T> : Collection<DifferenceCollectionItem<T>>, IDifferenceCollection<T> 
                                                    where T : class, IEntity
    {
        public DifferenceCollectionItem<T>? FindItemByOriginalId(Guid id)
        {
            foreach (var collectionItem in this)
            {
                var orgItem = collectionItem.OriginalItem;
                if (orgItem != null && orgItem.Id.Equals(id))
                    return collectionItem;
            }
            return null;
        }

        public DifferenceCollectionItem<T>? FindItemByMatchingOriginal(T match)
        {
            return (from e in this
                    where
                        e.OriginalItem != null &&
                        e.OriginalItem.Equals(match)
                    select (DifferenceCollectionItem<T>?) e
                   ).FirstOrDefault();
        }
    }
}