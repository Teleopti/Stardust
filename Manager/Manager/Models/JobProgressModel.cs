using System;

namespace Stardust.Manager.Models
{
    public class JobProgressModel
    {
        public Guid JobId { get; set; }
        public string ProgressDetail { get; set; }
    }
}