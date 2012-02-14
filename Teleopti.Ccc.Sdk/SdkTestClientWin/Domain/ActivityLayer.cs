using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class ActivityLayer
    {
        private ActivityLayerDto _dto;

        public ActivityLayer(ActivityLayerDto activityLayerDto)
        {
            _dto = activityLayerDto;
        }

        public ActivityLayerDto Dto
        {
            get { return _dto; }
        }
    }
}
