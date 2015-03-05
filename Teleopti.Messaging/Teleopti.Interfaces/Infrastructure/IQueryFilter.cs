using System;

namespace Teleopti.Interfaces.Infrastructure
{
    public interface IQueryFilter
    {
        string Name { get; }
	    void Enable(object session, object payload);
    }
}
