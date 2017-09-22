using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class SiteProviderTest
    {
        [Test]
        public void VerifyAllSitesItemIsAvailable()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new SiteProvider(siteRepository);
            Assert.AreEqual(UserTexts.Resources.AllSelection,target.AllSitesItem.Description.Name);
        }

        [Test]
        public void VerifyCanGetAllSitesWithAllSitesItemIncluded()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
	        var target = new SiteProvider(siteRepository);
            var result = new List<ISite> { SiteFactory.CreateSimpleSite("a") };
            
			siteRepository.Stub(x => x.LoadAll()).Return(result);

	        Assert.IsTrue(target.GetSitesAllSitesItemIncluded().Contains(target.AllSitesItem));
		}

	    [Test]
	    public void VerifyUpdate()
	    {
		    var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
		    var target = new SiteProvider(siteRepository);

		    var oldSite = SiteFactory.CreateSimpleSite("a");
		    oldSite.SetId(Guid.NewGuid());
		    var updatedSite = SiteFactory.CreateSimpleSite("a1");
		    updatedSite.SetId(oldSite.Id.GetValueOrDefault());

			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite> { oldSite });
		    siteRepository.Stub(x => x.Get(updatedSite.Id.GetValueOrDefault())).Return(updatedSite);

		    target.HandleMessageBrokerEvent(updatedSite.Id.GetValueOrDefault(), DomainUpdateType.Update);
		    Assert.AreEqual(2, target.GetSitesAllSitesItemIncluded().Count);
		    Assert.IsTrue(target.GetSitesAllSitesItemIncluded().Contains(updatedSite));
	    }

	    [Test]
	    public void VerifyDelete()
	    {
		    var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
		    var target = new SiteProvider(siteRepository);

		    var oldSite = SiteFactory.CreateSimpleSite("a");
		    oldSite.SetId(Guid.NewGuid());

		    siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite> {oldSite});

		    target.HandleMessageBrokerEvent(oldSite.Id.GetValueOrDefault(), DomainUpdateType.Delete);
		    Assert.AreEqual(1, target.GetSitesAllSitesItemIncluded().Count);
		    Assert.IsFalse(target.GetSitesAllSitesItemIncluded().Contains(oldSite));
	    }

	    [Test]
	    public void VerifyInsert()
	    {
		    var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
		    var target = new SiteProvider(siteRepository);

		    var newSite = SiteFactory.CreateSimpleSite("a1");
		    newSite.SetId(Guid.NewGuid());

		    siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite>());
		    siteRepository.Stub(x => x.Get(newSite.Id.GetValueOrDefault())).Return(newSite);

		    target.HandleMessageBrokerEvent(newSite.Id.GetValueOrDefault(), DomainUpdateType.Insert);
		    Assert.AreEqual(2, target.GetSitesAllSitesItemIncluded().Count);
		    Assert.IsTrue(target.GetSitesAllSitesItemIncluded().Contains(newSite));
	    }
    }
}
