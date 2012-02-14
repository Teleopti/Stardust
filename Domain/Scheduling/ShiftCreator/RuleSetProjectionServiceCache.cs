using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class RuleSetProjectionServiceCache : IRuleSetProjectionService
    {
        private readonly IRuleSetProjectionService _inner;
        private readonly ICustomDataCache<IEnumerable<IVisualLayerCollection>> _cache;

        public RuleSetProjectionServiceCache(IRuleSetProjectionService inner, 
                                            ICustomDataCache<IEnumerable<IVisualLayerCollection>> cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public IEnumerable<IVisualLayerCollection> ProjectionCollection(IWorkShiftRuleSet ruleSet)
        {
            var key = realKey(ruleSet.Id.HasValue ?
            ruleSet.Id.Value.ToString("N", CultureInfo.InvariantCulture) : ruleSet.GetHashCode().ToString(CultureInfo.InvariantCulture));
            var cachedValue = _cache.Get(key);
            if (cachedValue != null)
                return cachedValue;
            var res = _inner.ProjectionCollection(ruleSet);
            _cache.Put(key, res);
            return res;
        }

        private static string realKey(string key)
        {
            //note: normally this is not needed when ICustomDataCache is used
            //special case here because there might be multiple caches of IVisualLayerCollection
            //caching different things
            return "WorkShiftProj" + key;
        }
    }
}
