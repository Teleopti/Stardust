using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class JobCollectionFactory
	{
		private readonly IBaseConfiguration _baseConfiguration;

		public JobCollectionFactory(IBaseConfiguration baseConfiguration)
		{
			_baseConfiguration = baseConfiguration;
		}

		public ObservableCollection<IJob> JobCollection
		{
			get
			{
				var jobParameters = new JobParameters(
					null, 1,
					_baseConfiguration.TimeZoneCode,
					_baseConfiguration.IntervalLength.Value,
					ConfigurationManager.AppSettings["cube"],
					ConfigurationManager.AppSettings["pmInstallation"],
					CultureInfo.CurrentCulture, 
					_baseConfiguration.ToggleManager,
					_baseConfiguration.RunIndexMaintenance
					);

				jobParameters.Helper = new JobHelper();

				var jobCollection = new JobCollection(jobParameters);

				var jobs = new ObservableCollection<IJob>(jobCollection);

				return jobs;
			}
		}
	}
}
