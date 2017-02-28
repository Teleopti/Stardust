namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Direction of data move.
    /// </summary>
    public enum ExchangeDataOption
    {
        /// <summary>
        /// Move data from the server to the client.
        /// </summary>
        ServerToClient = 0,
        /// <summary>
        /// Move data from the client to the server.
        /// </summary>
        ClientToServer = 1,
        /// <summary>
        /// Move data from the datasource to the controls.
        /// </summary>
        DataSourceToControls = 0,
        /// <summary>
        /// Move data from the controls to the datasource.
        /// </summary>
        ControlsToDataSource = 1
    }
}