using System;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class SaveStudentAvailabilityCellDataEventArgs : EventArgs
    {
        private readonly IStudentAvailabilityCellData _cellData;

        public SaveStudentAvailabilityCellDataEventArgs(IStudentAvailabilityCellData cellData)
        {
            _cellData = cellData;
        }

        public IStudentAvailabilityCellData CellData
        {
            get { return _cellData; }
        }
    }
}