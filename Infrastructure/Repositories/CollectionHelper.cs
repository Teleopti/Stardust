using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public static class CollectionHelper
    {
        public static ICollection<T> ToDistinctGenericCollection<T>(object queryResult)
        {
            //maybe nicer to only accept an IList as inparameter. Lot of casting needed
            //in that case though. Think this is nicer. For now.
            IList castedQueryResult = queryResult as IList;
            if(castedQueryResult==null)
                throw new ArgumentException("queryResult must be an IList");

            return castedQueryResult.OfType<T>().Distinct().ToArray();
        }
    }
}
