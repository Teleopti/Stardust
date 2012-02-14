namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Interface to use when loading persons fast
    ///</summary>
    public interface ILightPerson
    {
        ///<summary>
        ///</summary>
        string FirstName { get; set; }
        ///<summary>
        ///</summary>
        string LastName { get; set; }
        ///<summary>
        ///</summary>
        string EmploymentNumber { get; set; }
    }
}