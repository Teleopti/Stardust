using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for workload repository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-24
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IWorkloadRepository : IRepository<IWorkload>
    {
        IWorkload LoadWorkload(IWorkload workload);
    }
}