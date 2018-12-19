using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
    /// <summary>
    /// Stateless unit of work. Doesn't keep track of changed objects.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-17
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IStatelessUnitOfWork : IDisposable
    {
        //add insert and update here later if needed...
    }
}