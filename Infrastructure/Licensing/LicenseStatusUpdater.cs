using System;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    

    public class LicenseStatusUpdater : ILicenseStatusUpdater
    {
        private readonly ILicenseStatusRepositories _licenseStatusRepositories;

        public LicenseStatusUpdater(ILicenseStatusRepositories licenseStatusRepositories)
        {
            _licenseStatusRepositories = licenseStatusRepositories;
        }

        public int RunCheck()
        {
            var numberOfActiveAgents = 0;
            ILicenseStatusXml status = null;
            try
            {
                numberOfActiveAgents = _licenseStatusRepositories.NumberOfActiveAgents();

                // to get one in the database
                //var target = new LicenseStatusXml() { CheckDate = new DateTime(2012, 2, 24), LastValidDate = new DateTime(2012, 2, 25), StatusOk = false, DaysLeft = 0, NumberOfActiveAgents = 8000};
                //var xml = target.XmlDocument;
                //_licenseStatusRepositories.SaveLicenseStatus(xml.InnerXml);
                //return 1;

                status = _licenseStatusRepositories.LicenseStatus;
                //throws an error if toomanyagents
                var licenseService = _licenseStatusRepositories.XmlLicenseService(numberOfActiveAgents);

                //Ok
                status.CheckDate = DateTime.Today.Date;
                status.LastValidDate = DateTime.Today.AddDays(1).Date;
                status.StatusOk = true;
                status.AlmostTooMany = licenseService.IsThisAlmostTooManyActiveAgents(numberOfActiveAgents);
                status.NumberOfActiveAgents = numberOfActiveAgents;
                _licenseStatusRepositories.SaveLicenseStatus(status.GetNewStatusDocument().OuterXml);
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
                        _licenseStatusRepositories.SaveLicenseStatus(status.GetNewStatusDocument().OuterXml);
                        return 1;
                    }

                    //Not Ok before and not now either
                    if (!status.StatusOk)
                    {
                        status.NumberOfActiveAgents = numberOfActiveAgents;
                        status.CheckDate = DateTime.Today.Date;
                        status.DaysLeft = (int)(status.LastValidDate.Date - DateTime.Today.Date).TotalDays - 1;
                        _licenseStatusRepositories.SaveLicenseStatus(status.GetNewStatusDocument().OuterXml);
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