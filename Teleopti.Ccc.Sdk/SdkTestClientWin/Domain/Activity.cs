using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class Activity
    {

        private ActivityDto _dto;

        public Activity(ActivityDto dto)
        {
            _dto = dto;
        }

        public ActivityDto Dto
        {
            get { return _dto; }
        }
    }
}
