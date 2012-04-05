﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class DetailedJobHistoryResultModel
    {
        public Guid? JobId { get; set; }
        public string Message { get; set; }
        public virtual string ExceptionStackTrace { get; set; }
        public virtual string ExceptionMessage { get; set; }
        public virtual string InnerExceptionStackTrace { get; set; }
        public virtual string InnerExceptionMessage { get; set; }
        public virtual DateTime TimeStamp { get; set; }
    }
}
