using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for projections
    /// </summary>
    /// <remarks>
    /// Indirectly tested from VisualLayerProjectionTest
    /// Created by: rogerkr
    /// Created date: 2008-01-29
    /// </remarks>
    public abstract class ProjectionService : IProjectionService
    {
        private readonly IPerson _person;


        protected ProjectionService(IPerson person)
        {
            _person = person;
        }


        public abstract IVisualLayerCollection CreateProjection();


        public IPerson Person
        {
            get { return _person; }
        }
    }
}
