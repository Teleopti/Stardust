using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
    public class LocalizedUpdateInfo
    {
        public string UpdatedTimeInUserPerspective(IChangeInfo entity)
        {
            string updated = string.Empty;

            if (entity.UpdatedOn.HasValue)
            {
                DateTime tempDate = entity.UpdatedOn.Value;
                DateTime localChangeDateTime = TimeZoneHelper.ConvertFromUtc(
                    tempDate, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

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
                    TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

                updatedBy = string.Concat(localizedUpdatedByText, " ", entity.UpdatedBy.Name, " ",
                                          string.Format(CultureInfo.CurrentCulture, localChangeDateTime.ToString()));
            }
            return updatedBy;
        }
    }
}
