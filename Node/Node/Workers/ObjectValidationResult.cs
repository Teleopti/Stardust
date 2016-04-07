namespace Stardust.Node.Workers
{
	public struct ObjectValidationResult
	{
		public bool IsBadRequest;
		public bool IsConflict;
		public string Message;
	}
}