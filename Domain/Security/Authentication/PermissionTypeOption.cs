namespace Teleopti.Ccc.Domain.Security.Authentication
{
    ///<summary>
    /// Useer permission type enumeration
    ///</summary>
    public enum UnitOfWorkFactoryValidationResult
    {
        /// <summary>
        /// Unit of work is not validated. Value = 0.
        /// </summary>
        NotValidated,
        ///<summary>
        /// Database in unit of work factory is not available. Value = 1.
        ///</summary>
        /// <remarks>
        /// This option indicates that the database that the unit of work factory represents is currently unavailable or the 
        /// connection string to the database is fake. 
        /// </remarks>
        DatabaseNotAvailable,
        ///<summary>
        /// Person with the specified authentication info can not be found in the database. Value = 2.
        ///</summary>
        /// <remarks>
        /// This option indicates that the database can be open, but no person can be found in the database with the specified
        /// authentication info, either with username - password or domain name - username.
        /// </remarks>
        NoPersonFound,
        /// <summary>
        /// Unit of work is valid. Value = 3.
        /// </summary>
        Valid
    }
}