using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Model
{
    public class SiteModel
    {
        public SiteModel(SiteDto siteDto)
        {
            if (siteDto!=null)
            {
                Id = siteDto.Id;
                Name = siteDto.DescriptionName;
            }
        }

        public Guid? Id { get; set; }

        public string Name { get; set; }
    }
}