﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job
{
    public class JobParameters : IJobParameters
    {
        public JobParameters(IJobMultipleDate jobCategoryDates, int dataSource, string timeZone, int intervalLengthMinutes, string cubeConnectionString, string pmInstall, CultureInfo currentCulture, IEtlToggleManager toggleManager)
        {
            DataSource = dataSource;
        	CurrentCulture = currentCulture;
        	DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            IntervalsPerDay = 1440 / intervalLengthMinutes;
            StateHolder = new CommonStateHolder(this);
            JobCategoryDates = jobCategoryDates ?? new JobMultipleDate(DefaultTimeZone);
            setOlapServerAndDatabase(cubeConnectionString);
            IsPmInstalled = checkPmInstall(pmInstall);
	        EtlToggleManager = toggleManager;
        }

        public int DataSource { get; set; }

        public IJobHelper Helper { get; set; }

        public TimeZoneInfo DefaultTimeZone { get; private set; }

        public int IntervalsPerDay { get; private set; }

        public ICommonStateHolder StateHolder { get; set; }

        public IJobMultipleDate JobCategoryDates { get; private set; }

        public string OlapServer { get; private set; }

        public string OlapDatabase { get; private set; }

		public DateTime? NowForTestPurpose { get;  set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<TimeZoneInfo> TimeZonesUsedByDataSources { get; set; }

        public bool IsPmInstalled { get; private set; }

		public CultureInfo CurrentCulture { get; private set; }

		public IEtlToggleManager EtlToggleManager { get; private set; }

    	private void setOlapServerAndDatabase(string cubeConnectionsString)
        {
            if (!string.IsNullOrEmpty(cubeConnectionsString))
            {
                string[] splittedString1 = cubeConnectionsString.Split(";".ToCharArray());
                foreach (string stringPart in splittedString1)
                {
                    string[] splittedString2 = stringPart.Split("=".ToCharArray());
                    if (splittedString2[0].ToUpperInvariant() == "DATA SOURCE")
                    {
                        OlapServer = splittedString2[1];
                    }
                    if (splittedString2[0].ToUpperInvariant() == "INITIAL CATALOG")
                    {
                        OlapDatabase = splittedString2[1];
                    }
                }
            }
        }

        private static bool checkPmInstall(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;

            if (flag.ToUpper(CultureInfo.InvariantCulture) == "TRUE")
                return true;

            return false;
        }
    }
}