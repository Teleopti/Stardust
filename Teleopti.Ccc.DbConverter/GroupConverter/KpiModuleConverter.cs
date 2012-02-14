using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.DBConverter.GroupConverter;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class KpiModuleConverter :ModuleConverter
    {
        //private readonly DefaultAggregateRoot _defaultAggregateRoot;

        public KpiModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
            //_defaultAggregateRoot = defaultAggregateRoot;
        }

        protected override string ModuleName
        {
            get { return "Security module"; }
        }

        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new List<Type>();
            }
        }

        /// <summary>
        /// Group convertion.
        /// </summary>
        protected override void GroupConvert()
        {
            AddKPIs();

        }

        private static void AddKPIs()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                KpiRepository rep = new KpiRepository(uow);

                KeyPerformanceIndicator kpiAnswered = new KeyPerformanceIndicator(
                    "Answered Calls per Scheduled Phone Hour",
                    "KpiAnsweredCallsPerScheduledPhoneHour",
                    EnumTargetValueType.TargetValueTypeNumber,
                    25,0,10,
                    Color.FromArgb(-256),Color.FromArgb(-65536),Color.FromArgb(-16744448)
                    );
                rep.Add(kpiAnswered);


                KeyPerformanceIndicator kpiAfterCall = new KeyPerformanceIndicator(
                    "Average After Call Work (s)",
                    "KpiAverageAfterCallWork",
                    EnumTargetValueType.TargetValueTypeNumber,
                    20, 0, 40,
                    Color.FromArgb(-16744448), Color.FromArgb(-16744448), Color.FromArgb(-65536)
                    );
                rep.Add(kpiAfterCall);

                KeyPerformanceIndicator kpiTalkTime = new KeyPerformanceIndicator(
                    "Average Talk Time (s)",
                    "KpiAverageTalkTime",
                    EnumTargetValueType.TargetValueTypeNumber,
                    120, 30, 160,
                    Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536)
                    );
                rep.Add(kpiTalkTime);

                KeyPerformanceIndicator kpiHandleTime = new KeyPerformanceIndicator(
                    "Average Handle Time (s)",
                    "KpiAverageHandleTime",
                    EnumTargetValueType.TargetValueTypeNumber,
                    140, 30, 180,
                    Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536)
                    );
                rep.Add(kpiHandleTime);

                KeyPerformanceIndicator kpiAdherence = new KeyPerformanceIndicator(
                    "Adherence (%)",
                    "KpiAdherence",
                    EnumTargetValueType.TargetValueTypePercent,
                    80, 75, 80,
                    Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448)
                    );
                rep.Add(kpiAdherence);

                KeyPerformanceIndicator kpiReadiness = new KeyPerformanceIndicator(
                    "Readiness (%)",
                    "KpiReadiness",
                    EnumTargetValueType.TargetValueTypePercent,
                    80, 75, 80,
                    Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448)
                    );
                rep.Add(kpiReadiness);

                KeyPerformanceIndicator kpiAbsenteeism = new KeyPerformanceIndicator(
                    "Absenteeism (%)",
                    "KpiAbsenteeism",
                    EnumTargetValueType.TargetValueTypePercent,
                    5, 4, 6,
                    Color.FromArgb(-256), Color.FromArgb(-16744448), Color.FromArgb(-65536)
                    );
                rep.Add(kpiAbsenteeism);

                uow.PersistAll();
            }
        }
    }
}
