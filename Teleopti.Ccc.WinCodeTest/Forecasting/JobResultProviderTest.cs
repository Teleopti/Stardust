using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class JobResultProviderTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IJobResultRepository _jobResultRepository;
        private JobResultProvider _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _jobResultRepository = _mocks.StrictMock<IJobResultRepository>();
            _target = new JobResultProvider(_unitOfWorkFactory,_jobResultRepository);
        }

        [Test]
        public void ShouldGetJobResults()
        {
            var pageDetails = new PagingDetail();
            
            using(_mocks.Record())
            {
                Expect.Call(_jobResultRepository.LoadHistoryWithPaging(pageDetails, JobCategory.QuickForecast,
                                                                       JobCategory.MultisiteExport,
                                                                       JobCategory.ForecastsImport)).Return(
                                                                           new List<IJobResult>
                                                                               {
                                                                                   new JobResult(string.Empty,
                                                                                                 new DateOnlyPeriod(),
                                                                                                 PersonFactory.
                                                                                                     CreatePerson("Bill"),
                                                                                                 new DateTime())
                                                                               });
            }
            using(_mocks.Playback())
            {
                _target.GetJobResults(pageDetails);
            }
        }

        [Test]
        public void ShouldGetJobResultsWithError()
        {
            var pageDetails = new PagingDetail();
            var jobResult = new JobResult(string.Empty,
                                          new DateOnlyPeriod(),
                                          PersonFactory.
                                              CreatePerson("Bill"),
                                          new DateTime());
            jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, string.Empty, new DateTime(), null));
            using (_mocks.Record())
            {
                Expect.Call(_jobResultRepository.LoadHistoryWithPaging(pageDetails, JobCategory.QuickForecast,
                                                                       JobCategory.MultisiteExport,
                                                                       JobCategory.ForecastsImport)).Return(
                                                                           new List<IJobResult>
                                                                               {
                                                                                 jobResult 
                                                                               });
            }
            using (_mocks.Playback())
            {
                var jobResults = _target.GetJobResults(pageDetails);

                Assert.That(jobResults.First().Status, Is.EqualTo(UserTexts.Resources.Error));
            }
        }

        [Test]
        public void ShouldGetJobResultsWithWarning()
        {
            var pageDetails = new PagingDetail();
            var jobResult = new JobResult(string.Empty,
                                          new DateOnlyPeriod(),
                                          PersonFactory.
                                              CreatePerson("Bill"),
                                          DateTime.Now);
            jobResult.FinishedOk = false;
            jobResult.AddDetail(new JobResultDetail(DetailLevel.Warning, string.Empty,DateTime.Now, null));
            using (_mocks.Record())
            {
                Expect.Call(_jobResultRepository.LoadHistoryWithPaging(pageDetails, JobCategory.QuickForecast,
                                                                       JobCategory.MultisiteExport,
                                                                       JobCategory.ForecastsImport)).Return(
                                                                           new List<IJobResult>
                                                                               {
                                                                                 jobResult 
                                                                               });
            }
            using (_mocks.Playback())
            {
                var jobResults = _target.GetJobResults(pageDetails);
                Assert.That(jobResults.First().Status, Is.EqualTo(UserTexts.Resources.WorkingThreeDots));
            }
        }


        [Test]
        public void ShouldGetJobResultsWithInfo()
        {
            var pageDetails = new PagingDetail();
            var jobResult = new JobResult(string.Empty,
                                          new DateOnlyPeriod(),
                                          PersonFactory.
                                              CreatePerson("Bill"),
                                          new DateTime());
            jobResult.FinishedOk = true;
            jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, string.Empty, new DateTime(), null));
            using (_mocks.Record())
            {
                Expect.Call(_jobResultRepository.LoadHistoryWithPaging(pageDetails, JobCategory.QuickForecast,
                                                                       JobCategory.MultisiteExport,
                                                                       JobCategory.ForecastsImport)).Return(
                                                                           new List<IJobResult>
                                                                               {
                                                                                 jobResult 
                                                                               });
            }
            using (_mocks.Playback())
            {
                var jobResults = _target.GetJobResults(pageDetails);
                Assert.That(jobResults.First().Status, Is.EqualTo(UserTexts.Resources.Done));
            }
        }
    }

}
