namespace Teleopti.Ccc.WinCode.Common.Interop
{
    public interface IHostViewModel
    {
        object ModelHeader { get; }
        object ModelContent { get; }
        void UpdateItem(object header, object content);
    }
}
