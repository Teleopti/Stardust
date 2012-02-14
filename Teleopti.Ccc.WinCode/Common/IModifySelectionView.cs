
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface IModifySelectionView
    {
        double Sum { get; set; }
        double ChosenAmount { get; set; }
        double Average { get; set; }
        double StandardDev { get; set; }
        double ModifiedSum { get; set; }
        string InputPercent { get; set; }
        string InputSmoothValue { get; set; }
        string InputType { get; set; }
        int InputState { get; set; }
        void Close();
        void SetDialogResult(DialogResult value);
    }
}
