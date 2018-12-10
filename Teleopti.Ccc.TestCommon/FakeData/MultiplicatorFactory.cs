using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Multiplicator domain object
    /// </summary>
    public static class MultiplicatorFactory
    {
        /// <summary>
        /// Creates an absence aggregate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shortName">The short name.</param>
        /// <param name="color">The color.</param>
        /// <param name="multiplicatorType">Type of the multiplicator.</param>
        /// <param name="multiplicatorValue">The multiplicator value.</param>
        /// <returns></returns>
        public static IMultiplicator CreateMultiplicator(string name, string shortName,
                                            Color color, 
                                            MultiplicatorType multiplicatorType, 
                                            double multiplicatorValue)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("shortName", shortName);
            InParameter.NotNull("color", color);
            InParameter.NotNull("multiplicatorType", multiplicatorType);
            IMultiplicator multi = new Multiplicator(multiplicatorType);
            multi.Description = new Description(name, shortName);
            multi.DisplayColor = color;
            multi.MultiplicatorValue = multiplicatorValue;
            multi.ExportCode = "MC";
            return multi;
        }
    }
}