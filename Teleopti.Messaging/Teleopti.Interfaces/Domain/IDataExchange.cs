namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for general datahandling on a form.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 12/10/2007
    /// </remarks>
    public interface IDataExchange
    {

        /// <summary>
        /// Validates the user edited data in control.
        /// </summary>
        /// <param name="direction">The direction of dataflow.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        bool ValidateData(ExchangeDataOption direction);


        /// <summary>
        /// Exchances the data between the data server object and the data client object.
        /// </summary>
        /// <param name="direction">The direction of dataflow.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        void ExchangeData(ExchangeDataOption direction);
    }
}