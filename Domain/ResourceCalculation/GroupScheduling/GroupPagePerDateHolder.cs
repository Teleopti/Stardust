namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
    public interface IGroupPagePerDateHolder
    {
        IGroupPagePerDate ShiftCategoryFairnessGroupPagePerDate { get; set; }
        IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }
    }

    public class GroupPagePerDateHolder : IGroupPagePerDateHolder
    {
        private IGroupPagePerDate _shiftCategoryFairnessGroupPagePerDate;
        public IGroupPagePerDate ShiftCategoryFairnessGroupPagePerDate
        {
            get { return _shiftCategoryFairnessGroupPagePerDate; }
            set { _shiftCategoryFairnessGroupPagePerDate = value; }
        }

        public IGroupPagePerDate GroupPersonGroupPagePerDate { get; set; }
    }
}