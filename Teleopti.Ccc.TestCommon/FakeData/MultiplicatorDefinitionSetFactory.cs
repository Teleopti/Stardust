using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for MultiplicatorDefinitionSet domain object
    /// </summary>
    public static class MultiplicatorDefinitionSetFactory
    {
        /// <summary>
        /// Creates an multiplicator definition set aggregate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="multiplicatorType">Type of the multiplicator.</param>
        /// <returns></returns>
        public static IMultiplicatorDefinitionSet CreateMultiplicatorDefinitionSet(string name, MultiplicatorType multiplicatorType)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("multiplicatorType", multiplicatorType);
            IMultiplicatorDefinitionSet multi = new MultiplicatorDefinitionSet(name, multiplicatorType);
            return multi;
        }
    }
}