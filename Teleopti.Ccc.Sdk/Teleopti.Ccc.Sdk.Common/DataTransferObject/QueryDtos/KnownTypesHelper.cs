using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    public static class KnownTypesHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IEnumerable<Type> GetKnownTypes<T>()
        {
            var list = new List<Type>();
            var queryTypes = typeof(T).Assembly.GetExportedTypes()
                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsGenericTypeDefinition);

            list.AddRange(queryTypes);
            return list;
        }
    }
}
