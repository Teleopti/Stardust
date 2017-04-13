
namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    /// <summary>
    /// Represents a Parameter class that pass use to pass Parameters to Smart part .
    /// </summary>
    public class SmartPartParameter
    {
        public string ParameterName { get; set; }
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartPartParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the Parameter.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public SmartPartParameter(string parameterName,object value)
        {
            ParameterName = parameterName;
            Value = value;
        }
    }
}
