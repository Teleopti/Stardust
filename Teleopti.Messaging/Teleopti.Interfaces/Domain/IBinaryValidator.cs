using System.Collections;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Validate a <see cref="BitArray"/>
    /// </summary>
    public interface IBinaryValidator
    {
        /// <summary>
        /// Validates the specified array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        bool Validate(BitArray array);
    }
}