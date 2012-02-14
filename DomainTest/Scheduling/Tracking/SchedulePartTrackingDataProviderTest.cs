using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    [TestFixture]
    public class SchedulePartTrackingDataProviderTest
    {
        private ITrackingDataProvider _target;
        private MockRepository _mockRep;
        private IScheduleDay _schedulePart;
       

        [SetUp]
        public void Setup()
        {
            _mockRep = new MockRepository();
            _schedulePart = _mockRep.StrictMock<IScheduleDay>();
            _target = new SchedulePartTrackingDataProvider(_schedulePart);
        }


        [Test]
        public void VerifyCreatesProjection()
        {
            IProjectionService proj = _mockRep.StrictMock<IProjectionService>();
            IVisualLayerCollection retCol = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            using(_mockRep.Record())
            {
                Expect.Call(_schedulePart.ProjectionService()).Return(proj);
                Expect.Call(proj.CreateProjection()).Return(retCol);
            }
            Assert.AreEqual(retCol,_target.CreateVisualLayerCollection());
        }
    }
}
