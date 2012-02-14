namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class Activity
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Activity(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}