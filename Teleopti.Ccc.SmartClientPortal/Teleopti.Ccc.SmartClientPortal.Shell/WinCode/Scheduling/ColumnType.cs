namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public enum ColumnType
    {
        None,
        RowHeaderColumn,

        CurrentContractTimeColumn,
        CurrentDayOffColumn,

        StartTargetColumns,
        TargetContractTimeColumn = StartTargetColumns ,
        TargetDayOffColumn,
        
        StartScheduleColumns,
    }
}