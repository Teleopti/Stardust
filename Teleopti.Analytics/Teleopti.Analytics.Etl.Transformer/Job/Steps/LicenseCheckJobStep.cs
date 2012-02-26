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
            Name = "Licenseuse";
            JobCategory = JobCategoryType.Initial;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            ILicenseStatusXml status = null;
            try
            {
                var numberOfActiveAgents = JobParameters.Helper.Repository.NumberOfActiveAgents();

                // to get one in the database
                //var target = new LicenseStatusXml() { CheckDate = new DateTime(2012, 2, 24), LastValidDate = new DateTime(2012, 2, 25), StatusOk = true };
                //var xml = target.XmlDocument;
                //JobParameters.Helper.Repository.SaveLicensStatus(xml.InnerXml);

                status = JobParameters.Helper.Repository.LicenseStatus;
                //throws an error if toomanyagents
                var licenseService = JobParameters.Helper.Repository.XmlLicenseService(numberOfActiveAgents);
                
                //Ok
                status.CheckDate = DateTime.Today;
                status.LastValidDate = DateTime.Today.AddDays(1);
                status.StatusOk = true;
                status.AlmostTooMany = licenseService.IsThisAlmostTooManyActiveAgents(numberOfActiveAgents);
                JobParameters.Helper.Repository.SaveLicensStatus(status.XmlDocument.OuterXml);
                return 1;  
            }
            catch (TooManyActiveAgentsException)
            {
                
                if (status != null)
                {
                    //Ok before and but not now
                    if (status.StatusOk)
                    {
                        status.CheckDate = DateTime.Today;
                        status.LastValidDate = DateTime.Today.AddDays(30);
                        status.StatusOk = false;
                        status.AlmostTooMany = false;
                        JobParameters.Helper.Repository.SaveLicensStatus(status.XmlDocument.OuterXml);
                        return 1;
                    }

                    //Not Ok before and not now either
                    if (!status.StatusOk)
                    {
                        status.CheckDate = DateTime.Today;
                        JobParameters.Helper.Repository.SaveLicensStatus(status.XmlDocument.OuterXml);
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