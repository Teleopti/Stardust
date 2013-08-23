namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public interface IColumnDisableCondition<T>
    {
        bool IsColumnDisable(T dataItem, string bindingProperty);
    }
}