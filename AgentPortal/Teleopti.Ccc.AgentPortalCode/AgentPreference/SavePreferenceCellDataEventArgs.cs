using System;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public class SavePreferenceCellDataEventArgs : EventArgs
    {
        private readonly IPreferenceCellData _cellData;

        public SavePreferenceCellDataEventArgs(IPreferenceCellData cellData)
        {
            _cellData = cellData;
        }

        public IPreferenceCellData CellData
        {
            get { return _cellData; }
        }
    }
}