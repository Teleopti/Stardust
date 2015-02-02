using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using RequestContext = System.ServiceModel.Channels.RequestContext;

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

		//Robtodo : Reenable test once prototype is over, 
		// and we actually have repositories to test against.
		[Test, Ignore]
		public void ShouldGetLocationsFromTextFile()
		{
			//target.Request = new HttpRequestMessage();
			// (was mocking a text file path)
			//var result = target.Get() as dynamic;
			//var locationViewModel = result.Data as LocationViewModel;
			//locationViewModel.Should().Not.Be.Null();
			//locationViewModel.Name.Should().Be ("China");
			//locationViewModel.Children.Should().Not.Be.Empty();
			
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}
	}
}	

