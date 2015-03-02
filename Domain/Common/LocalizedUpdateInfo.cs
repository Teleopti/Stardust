using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    //rk - cut/paste from aggregateroot.
    public interface ILocalizedUpdateInfo
    {
       string UpdatedTimeInUserPerspective(IChangeInfo entity);
       string UpdatedByText(IChangeInfo entity, string localizedUpdatedByText);
    }

    public class LocalizedUpdateInfo : ILocalizedUpdateInfo
    {
        public string UpdatedTimeInUserPerspective(IChangeInfo entity)
        {
            string updated = string.Empty;

            if (entity.UpdatedOn.HasValue)
            {
                DateTime tempDate = entity.UpdatedOn.Value;
                DateTime localChangeDateTime = TimeZoneHelper.ConvertFromUtc(
                    tempDate, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

                updated = string.Format(CultureInfo.CurrentCulture, localChangeDateTime.ToString());
            }
            return updated;
        }

        public string UpdatedByText(IChangeInfo entity, string localizedUpdatedByText)
        {
            string updatedBy = string.Empty;

            if (entity.UpdatedOn.HasValue)
            {
                DateTime tempDate = entity.UpdatedOn.Value;
                DateTime localChangeDateTime = TimeZoneHelper.ConvertFromUtc(
                    tempDate,
                    TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

                updatedBy = string.Concat(localizedUpdatedByText, " ", entity.UpdatedBy.Name, " ",
                                          string.Format(CultureInfo.CurrentCulture, localChangeDateTime.ToString()));
            }
            return updatedBy;
        }
    }
}
