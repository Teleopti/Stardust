using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference
{
	[TestFixture]
	public class RuleSetProjectionServiceForMultiSessionCachingTest
	{
		[Test]
		public void ShouldCallOriginalService()
		{
			var service = MockRepository.GenerateMock<IRuleSetProjectionService>();
			var target = new RuleSetProjectionServiceForMultiSessionCaching(service, MockRepository.GenerateMock<ILazyLoadingManager>());

			target.ProjectionCollection(null);

			service.AssertWasCalled(x => x.ProjectionCollection(null));
		}

		[Test]
		public void ShouldLoadDataForCaching()
		{
			var service = MockRepository.GenerateMock<IRuleSetProjectionService>();
			var lazyLoadingManager = MockRepository.GenerateMock<ILazyLoadingManager>();
			var target = new RuleSetProjectionServiceForMultiSessionCaching(service, lazyLoadingManager);
			var visualLayer = new VisualLayer(new Activity(" "), new DateTimePeriod(), new Activity(" "));
			var layers = new List<IVisualLayer>(new[] {visualLayer});
			var data = new WorkShiftVisualLayerInfo(new WorkShift(new ShiftCategory(" ")), new VisualLayerCollection(null, layers, new ProjectionPayloadMerger()));
			service.Stub(x => x.ProjectionCollection(null)).Return(new[] { data });

			target.ProjectionCollection(null);

			lazyLoadingManager.AssertWasCalled(x => x.Initialize(data.WorkShift));
			lazyLoadingManager.AssertWasCalled(x => x.Initialize(data.VisualLayerCollection));
			lazyLoadingManager.AssertWasCalled(x => x.Initialize(data.WorkShift.ShiftCategory));
			lazyLoadingManager.AssertWasCalled(x => x.Initialize(visualLayer.HighestPriorityActivity));
		}
	}

}
