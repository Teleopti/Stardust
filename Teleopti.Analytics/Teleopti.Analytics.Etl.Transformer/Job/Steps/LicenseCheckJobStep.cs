using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class LicenseCheckJobStep : JobStepBase
    {
        public LicenseCheckJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            // another more discreate name??????
            Name = "Licenseuse";
            JobCategory = JobCategoryType.Initial;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            var numberOfActiveAgents = 0;
            ILicenseStatusXml status = null;
            try
            {
                numberOfActiveAgents = JobParameters.Helper.Repository.NumberOfActiveAgents();

                // to get one in the database
                //var target = new LicenseStatusXml() { CheckDate = new DateTime(2012, 2, 24), LastValidDate = new DateTime(2012, 2, 25), StatusOk = true };
                //var xml = target.XmlDocument;
                //JobParameters.Helper.Repository.SaveLicenseStatus(xml.InnerXml);

                status = JobParameters.Helper.Repository.LicenseStatus;
                //throws an error if toomanyagents
                var licenseService = JobParameters.Helper.Repository.XmlLicenseService(numberOfActiveAgents);
                
                //Ok
                status.CheckDate = DateTime.Today.Date;
                status.LastValidDate = DateTime.Today.AddDays(1).Date;
                status.StatusOk = true;
                status.AlmostTooMany = licenseService.IsThisAlmostTooManyActiveAgents(numberOfActiveAgents);
                status.NumberOfActiveAgents = numberOfActiveAgents;
                JobParameters.Helper.Repository.SaveLicenseStatus(status.XmlDocument.OuterXml);
                return 1;  
            }
            catch (TooManyActiveAgentsException)
            {
                
                if (status != null)
                {
                    //Ok before  but not now
                    if (status.StatusOk)
                    {
                        status.CheckDate = DateTime.Today.Date;
                        status.LastValidDate = DateTime.Today.AddDays(30).Date;
                        status.DaysLeft = 30;
                        status.StatusOk = false;
                        status.AlmostTooMany = false;
                        status.NumberOfActiveAgents = numberOfActiveAgents;
                        JobParameters.Helper.Repository.SaveLicenseStatus(status.XmlDocument.OuterXml);
                        return 1;
                    }

                    //Not Ok before and not now either
                    if (!status.StatusOk)
                    {
                        status.NumberOfActiveAgents = numberOfActiveAgents;
                        status.CheckDate = DateTime.Today.Date;
                        status.DaysLeft = (int)(status.LastValidDate.Date - DateTime.Today.Date).TotalDays -1; 
                        JobParameters.Helper.Repository.SaveLicenseStatus(status.XmlDocument.OuterXml);
                        return 1;
                    }
                }
                
            }
            //    // catch this or just let pop up??
            //catch (Exception exception)
            //{
                
            //    throw;
            //}

            return 0;
        }
    }
}