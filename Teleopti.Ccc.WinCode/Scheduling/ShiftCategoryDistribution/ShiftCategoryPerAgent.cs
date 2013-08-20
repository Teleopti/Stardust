using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    public class ShiftCategoryPerAgent
    {
        public IPerson Person { get; set; }
        public string ShiftCategoryName { get; set; }
        public int Count { get; set; }

        public ShiftCategoryPerAgent(IPerson person, string shiftCategoryName, int count)
        {
            Person = person;
            ShiftCategoryName = shiftCategoryName;
            Count = count;
        }
    }
}