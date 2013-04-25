using System;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class Activity
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }

        public Activity(Guid? id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}