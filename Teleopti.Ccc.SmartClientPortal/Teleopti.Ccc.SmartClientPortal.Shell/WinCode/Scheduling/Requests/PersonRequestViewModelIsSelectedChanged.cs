namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    /// <summary>
    /// Notifies when a PersonRequestViewModel IsSelected
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-11-13
    /// </remarks>
    public class PersonRequestViewModelIsSelectedChanged
    {
        public PersonRequestViewModel Model { get; private set; }

        public PersonRequestViewModelIsSelectedChanged(PersonRequestViewModel model)
        {
            Model = model;
        }

    }
}
