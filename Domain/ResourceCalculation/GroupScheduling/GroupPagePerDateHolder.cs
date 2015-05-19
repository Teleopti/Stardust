namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
    public interface IGroupPagePerDateHolder
    {
        IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }
    }

    public class GroupPagePerDateHolder : IGroupPagePerDateHolder
    {
    	public IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }		
    }
}