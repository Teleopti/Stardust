using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class JobResult : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IJobResult
    {
        private readonly IPerson _owner;
        private readonly DateTime _timestamp;
        private readonly string _jobCategory;
        private DateOnlyPeriod _period;
        private readonly IList<IJobResultDetail> _details = new List<IJobResultDetail>();
        private bool _finishedOk;
		private IList<JobResultArtifact> _artifacts = new List<JobResultArtifact>();

        protected JobResult(){}

        public JobResult(string jobCategory, DateOnlyPeriod period, IPerson owner, DateTime timestamp)
        {
            _jobCategory = jobCategory;
            _period = period;
            _owner = owner;
            _timestamp = timestamp;
        }

        public virtual string JobCategory
        {
            get { return _jobCategory; }
        }


        public virtual DateOnlyPeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public virtual DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public virtual IPerson Owner
        {
            get { return _owner; }
        }

        public virtual IEnumerable<IJobResultDetail> Details
        {
            get { return _details; }
        }

        public virtual void AddDetail(IJobResultDetail jobResultDetail)
        {
            jobResultDetail.SetParent(this);
            _details.Add(jobResultDetail);
        }

        public virtual bool HasError()
        {
            return DetailHasError() || ResultTimeOut();
        }

        public virtual bool IsWorking()
        {
            return !_finishedOk && !HasError();
        }

        public virtual bool FinishedOk
        {
            get { return _finishedOk; }
            set { _finishedOk = value; }
        }

	    public virtual IList<JobResultArtifact> Artifacts
	    {
		    get { return _artifacts;}			
	    }

	    public virtual void AddArtifact(JobResultArtifact artifact)
	    {
		    artifact.SetParent(this);
		    _artifacts.Add(artifact);
	    }

        private bool ResultTimeOut()
        {
            return !_finishedOk && _timestamp < DateTime.UtcNow.AddHours(-12);
        }

        private bool DetailHasError()
        {
            return _details.Any(d => d.DetailLevel == DetailLevel.Error);
        }
    }

    public static class JobCategory
    {
        public const string MultisiteExport = "MultisiteExport";
        public const string QuickForecast = "QuickForecast";
        public const string ForecastsImport = "ForecastsImport";
        public const string CopySchedule = "CopySchedule";
        public const string ImportSchedule = "ImportSchedule";
        public const string WebSchedule = "WebSchedule";
		public const string WebClearSchedule = "WebClearSchedule";
        public const string WebIntradayOptimization = "WebIntradayOptimization";
	    public const string WebImportAgent = "WebImportAgent";
	    public const string WebImportExternalGamification = "WebImportExternalGamification";
		public const string WebRecalculateBadge = "WebRecalculateBadge";

	}
}