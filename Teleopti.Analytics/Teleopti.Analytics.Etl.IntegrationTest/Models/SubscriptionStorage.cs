using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class SubscriptionStorage
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public byte[] Value { get; set; }
    }
}
