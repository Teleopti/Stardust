using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{

    public class ImportForecastsFileToSkill : EventWithLogOnAndInitiator
    {

        public Guid JobId { get; set; }

        public  Guid Identity
        {
            get { return JobId; }
        }

        public Guid UploadedFileId { get; set; }

        public Guid TargetSkillId { get; set; }

        public Guid OwnerPersonId { get; set; }

        public ImportForecastsMode ImportMode { get; set; }
    }

    public enum ImportForecastsMode
    {
        ImportWorkload,
        ImportStaffing,
        ImportWorkloadAndStaffing
    }
}