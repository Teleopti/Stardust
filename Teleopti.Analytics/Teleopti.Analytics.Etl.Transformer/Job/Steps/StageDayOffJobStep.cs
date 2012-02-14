using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    //public class StageDayOffJobStep : JobStepBase
    public class StageDayOffJobStep
    {
        //public StageDayOffJobStep(IJobParameters jobParameters)
        //    : base(jobParameters)
        //{
        //    Name = "stg_day_off";
        //}

        //protected override int RunStep()
        //{
        //    //Get data from Raptor
        //    IList<IDayOffTemplate> rootList = _jobParameters.Helper.Repository.LoadDayOff();

        //    //Transform data from Raptor to Matrix format
        //    DayOffTransformer raptorTransformer = new DayOffTransformer();
        //    DataTable bulkTable = raptorTransformer.Transform(rootList, DateTime.Now);

        //    //Truncate staging table & Bulk insert data to staging database
        //    return _jobParameters.Helper.Repository.PersistDayOff(bulkTable);

        //}
    }
}