namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
    public interface IGroupPagePerDateHolder
    {
        IGroupPagePerDate ShiftCategoryFairnessGroupPagePerDate { get; set; }
        IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }
    }

    public class GroupPagePerDateHolder : IGroupPagePerDateHolder
    {
    	public IGroupPagePerDate ShiftCategoryFairnessGroupPagePerDate { get; set; }

    	public IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }
    }
}