namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IRequestDetailsView
	{
		string Subject { get; set; }
		string Message { get; set; }
		string LabelName { get; set; }
		string Status { get; set; }
	}
}