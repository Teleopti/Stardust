using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class SitePresenterTest
	{
		private SitePresenter _target;
		private MockRepository _mocks;
		private ISiteRepository _siteRepository;
		private ISiteView _siteView;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_siteView = _mocks.StrictMock<ISiteView>();
			_siteRepository = _mocks.StrictMock<ISiteRepository>();
			_target = new SitePresenter(_siteView, _siteRepository);
		}

		[Test]
		public void ShouldLoadAllSitesFromRepository()
		{
			Expect.Call(_siteRepository.LoadAll()).Return(new List<ISite>());
			Expect.Call(() => _siteView.LoadSiteGrid(new List<ISite>()));
			_mocks.ReplayAll();
			_target.OnPageLoad();
 			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotReturnDeletedSite()
		{
			var site = new Site("sitedeleted");
			var site2 = new Site("site2");
			site.SetDeleted();
			var sites = new List<ISite>{site2,site};
			Expect.Call(_siteRepository.LoadAll()).Return(sites);
			Expect.Call(() => _siteView.LoadSiteGrid(new List<ISite> { site2}));
			_mocks.ReplayAll();
			_target.OnPageLoad();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldContainNewSiteOnRefresh()
		{
			var site = new Site("siteNew");
			var site2 = new Site("site2");
			var sites = new List<ISite> { site2 };
			Expect.Call(_siteRepository.LoadAll()).Return(sites);
			Expect.Call(() => _siteView.LoadSiteGrid(sites));
			Expect.Call(_siteRepository.LoadAll()).Return(new List<ISite> { site2, site });
			Expect.Call(() => _siteView.LoadSiteGrid(new List<ISite> { site2, site }));
			_mocks.ReplayAll();
			_target.OnPageLoad();
			_target.OnPageLoad();
			_mocks.VerifyAll();

		}

		//[Test]
		//public void ShouldNotContainADeletedSiteOnRefresh()
		//{
		//    var site = new Site("sitedeleted");
		//    var sites = new List<ISite> { new Site("site1"), site };
		//    Expect.Call(_siteRepository.LoadAll()).Return(sites);
			
		//    Expect.Call(_siteRepository.LoadAll()).Return(new List<ISite> { new Site("site1"), site });
		//    _mocks.ReplayAll();
		//    Assert.That(_target.AllNotDeletedSites.Contains(site), Is.True);
		//    site.SetDeleted();
		//    Assert.That(_target.AllNotDeletedSites.Contains(site), Is.False);
		//    _mocks.VerifyAll();
		//}
	}

	
}