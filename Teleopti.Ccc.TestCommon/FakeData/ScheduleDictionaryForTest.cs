using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Test variation of <see cref="ScheduleDictionary"/> that makes it possible to add to the base dictionary explicitly. Use for test only.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ScheduleDictionaryForTest : ScheduleDictionary
    {
        public ScheduleDictionaryForTest(IScenario scenario, IScheduleDateTimePeriod period, IDictionary<IPerson, IScheduleRange> dictionary)
            : base(scenario, period, dictionary) { }

        /// <summary>
        /// Adds the test item to the base <see cref="IDictionary{TKey,TValue}<IPerson, IScheduleRange>"/> dictionary.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="range">The range.</param>
        public void AddTestItem(IPerson person, IScheduleRange range)
        {
            BaseDictionary.Add(person, range);
        }
    }
}
