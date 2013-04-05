namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IBlockFinderTypeHelper
    {
        string Name { get; set; }
        string Key { get; set; }
    }

    public class BlockFinderTypeHelper : IBlockFinderTypeHelper
    {
        public string Name { get; set; }
        public string Key { get; set; }
    }
}