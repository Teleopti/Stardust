using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class SearchPath : ISearchPath
    {
        public string Path
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }
    }
}