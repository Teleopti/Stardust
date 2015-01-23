using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class SiteProviderTest
    {
        private MockRepository _mocks;
        private ISiteProvider _target;
        private ISiteRepository _siteRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _siteRepository = _mocks.StrictMock<ISiteRepository>();
            _target = new SiteProvider(_siteRepository);
        }

        [Test]
        public void VerifyAllSitesItemIsAvailable()
        {
            Assert.AreEqual(UserTexts.Resources.AllSelection,_target.AllSitesItem.Description.Name);
        }

        [Test]
        public void VerifyCanGetAllSites()
        {
            var result = new List<ISite> { _mocks.StrictMock<ISite>() };
            using (_mocks.Record())
            {
                Expect.Call(_siteRepository.LoadAll()).Return(result);
                Expect.Call(result[0].Description).Return(new Description("a"));
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(result, _target.GetSitesAllSitesItemNotIncluded());
            }
        }

        [Test]
        public void VerifyCanGetAllSitesWithAllSitesItemIncluded()
        {
            _target = new SiteProvider(_siteRepository);
            var result = new List<ISite> { _mocks.StrictMock<ISite>() };
            using (_mocks.Record())
            {
                Expect.Call(_siteRepository.LoadAll()).Return(result);
                Expect.Call(result[0].Description).Return(new Description("a"));
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.GetSitesAllSitesItemIncluded().Contains(_target.AllSitesItem));
            }
        }

        [Test]
        public void VerifyUpdate()
        {
            var oldSite = _mocks.StrictMock<ISite>();
            var newSite = _mocks.StrictMock<ISite>();
            var siteList = new List<ISite> {oldSite};
            var siteId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_siteRepository.LoadAll()).Return(siteList);
                Expect.Call(_siteRepository.Get(siteId)).Return(newSite);
                Expect.Call(oldSite.Id).Return(siteId);
                Expect.Call(oldSite.Description).Return(new Description("a"));
                Expect.Call(newSite.Description).Return(new Description("a1"));
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(siteId,DomainUpdateType.Update);
                Assert.AreEqual(1,_target.GetSitesAllSitesItemNotIncluded().Count);
                Assert.IsTrue(_target.GetSitesAllSitesItemNotIncluded().Contains(newSite));
            }
        }

        [Test]
        public void VerifyDelete()
        {
            var oldSite = _mocks.StrictMock<ISite>();
            var siteList = new List<ISite> { oldSite };
            var siteId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_siteRepository.LoadAll()).Return(siteList);
                Expect.Call(oldSite.Id).Return(siteId);
                Expect.Call(oldSite.Description).Return(new Description("a"));
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(siteId, DomainUpdateType.Delete);
                Assert.AreEqual(0, _target.GetSitesAllSitesItemNotIncluded().Count);
                Assert.IsFalse(_target.GetSitesAllSitesItemNotIncluded().Contains(oldSite));
            }
        }


        [Test]
        public void VerifyInsert()
        {
            var newSite = _mocks.StrictMock<ISite>();
            var siteList = new List<ISite>();
            var siteId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_siteRepository.LoadAll()).Return(siteList);
                Expect.Call(_siteRepository.Get(siteId)).Return(newSite);
                Expect.Call(newSite.Description).Return(new Description("a1"));
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(siteId, DomainUpdateType.Insert);
				Assert.AreEqual(1, _target.GetSitesAllSitesItemNotIncluded().Count);
				Assert.IsTrue(_target.GetSitesAllSitesItemNotIncluded().Contains(newSite));
            }
        }
    }
}
