using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IStandardDeviationData
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<DateOnly, double?> Data { get; }
        void Add(DateOnly day, double? standardDeviation);
    }

    public class StandardDeviationData : IStandardDeviationData
    {
        public IDictionary<DateOnly, double?> Data { get; private set; }

        public StandardDeviationData()
        {
            Data = new Dictionary<DateOnly, double?>();
        }

        public void Add(DateOnly day, double? standardDeviation)
        {
            Data.Add(day, standardDeviation);
        }
    }
}