﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;
using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class MenuControllerTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_menuViewModelFactory = _mocks.DynamicMock<IMenuViewModelFactory>();
			_target = new MenuController(_menuViewModelFactory);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		#endregion

		private MenuController _target;

		private MockRepository _mocks;

		private IMenuViewModelFactory _menuViewModelFactory;

		[Test]
		public void DefaultActionShouldRedirectIfAccessOnlyToOneArea()
		{
			using (_mocks.Record())
			{
				Expect.Call(_menuViewModelFactory.CreateMenyViewModel()).Return(new[] { new ApplicationViewModel { Area = "MyTime" } });
			}
			using (_mocks.Playback())
			{
				var result = _target.Index() as RedirectToRouteResult;
				result.RouteValues["area"].Should().Be.EqualTo("MyTime");
			}
		}

		[Test]
		public void DefaultActionShouldRenderViewWhenAccessToMultipleAreas()
		{
			using (_mocks.Record())
			{
				Expect.Call(_menuViewModelFactory.CreateMenyViewModel()).Return(new[] { new ApplicationViewModel(), new ApplicationViewModel() });
			}
			using (_mocks.Playback())
			{
				var result = _target.Index() as ViewResult;
				result.ViewName.Should().Be.EqualTo(string.Empty);
			}
		}

		[Test]
		public void MenuShouldReturnDefaultView()
		{
			var result = _target.Menu();
			result.ViewName.Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void MobileMenuShouldReturnDefaultView()
		{
			var result = _target.MobileMenu();
			result.ViewName.Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldReturnAvailableApplications()
		{
			var menuViewModelFactory = MockRepository.GenerateMock<IMenuViewModelFactory>();
			var target = new MenuController(menuViewModelFactory);
			var applicationViewModel = new ApplicationViewModel();
			menuViewModelFactory.Stub(x => x.CreateMenyViewModel()).Return(new[] {applicationViewModel});

			var result = target.Applications();

			var resultApplications = result.Data as IEnumerable<ApplicationViewModel>;
			resultApplications.Single().Should().Be(applicationViewModel);
		}
		
	}
}