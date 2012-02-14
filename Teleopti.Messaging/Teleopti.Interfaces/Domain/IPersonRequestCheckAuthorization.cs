namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Provides a way to check permissions for person requests
    ///</summary>
    public interface IPersonRequestCheckAuthorization
    {
        ///<summary>
        ///Verifies if the current user has permission to edit the request.
        ///</summary>
        void VerifyEditRequestPermission(IPersonRequest personRequest);

        ///<summary>
        /// Determines if the current user has permission to edit the given request
        ///</summary>
        ///<param name="personRequest"></param>
        ///<returns></returns>
        bool HasEditRequestPermission(IPersonRequest personRequest);

        ///<summary>
        ///</summary>
        ///<param name="personRequest"></param>
        ///<returns></returns>
        bool HasViewRequestPermission(IPersonRequest personRequest);
    }
}