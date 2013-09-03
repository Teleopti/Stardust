namespace Teleopti.Ccc.Domain.Security
{
    ///<summary>
    /// Useer permission type enumeration
    ///</summary>
    public enum PermissionTypeOption
    {
        ///<summary>
        /// Person only has rights to the given function if the function is in the list
        ///</summary>
        /// <remarks>
        /// This option is used for an pessimistic scenario when the system only let the person
        /// do the method that is permitted. If for example the person has no rights for opening the
        /// application, then the person will not be able to run any method in the application even if has got 
        /// rights to the method itself. This is the Windows Standard 
        /// </remarks>
        Tight,
        ///<summary>
        /// The person has rights to the given function if there is a function in the 
        /// functions list that starts with the function name.
        ///</summary>
        /// <remarks>
        /// This option is used for an optimistic scenario when the system makes sure that the person
        /// can actually get rights to really use the method. For example if the method is a button
        /// initiated command on a form, then the system let the person open the application, open the
        /// form thus making the command available. 
        /// </remarks>
        Generous
    }
}