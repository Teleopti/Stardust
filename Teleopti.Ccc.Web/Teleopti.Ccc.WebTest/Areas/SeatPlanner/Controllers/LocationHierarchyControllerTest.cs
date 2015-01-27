using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.WebTest.Areas.SeatPlanner.Controllers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design","CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	internal class LocationHierarchyControllerTest
	{
		private LocationHierarchyController target;
		private readonly LocationHierarchyProvider locationHierarchyProvider = new LocationHierarchyProvider();

		[SetUp]
		public void Setup()
		{
			target = new LocationHierarchyController (locationHierarchyProvider);
		}

		[Test]
		public void ShouldGetLocationsFromTextFile()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			request.Stub(x => x.MapPath("")).IgnoreArguments().Return(
				Path.GetFullPath(@"..\..\..\Teleopti.Ccc.Web\Areas\SeatPlanner\Content\Temp\Locations.txt"));

			var result = target.Get() as dynamic;
			var locationViewModel = result.Data as LocationViewModel;
			locationViewModel.Should().Not.Be.Null();
			locationViewModel.Name.Should().Be ("China");
			locationViewModel.Children.Should().Not.Be.Empty();
			
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}
	}
}	

