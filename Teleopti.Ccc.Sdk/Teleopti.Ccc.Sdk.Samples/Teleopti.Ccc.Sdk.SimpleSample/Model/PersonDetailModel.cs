using System;

namespace Teleopti.Ccc.Sdk.SimpleSample.Model
{
    public class PersonDetailModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Team { get; set; }
        public string Site { get; set; }
        public string BusinessUnit { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public string Skills { get; set; }
        public string ExternalLogOns { get; set; }
        public string Contract { get; set; }
        public string ContractSchedule { get; set; }
        public string PartTimePercentageName { get; set; }
        public string PartTimePercentageValue { get; set; }
        public string Note { get; set; }
        public DateTime? LeavingDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}