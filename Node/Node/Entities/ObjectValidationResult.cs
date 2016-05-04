namespace Stardust.Node.Entities
{
	public struct ObjectValidationResult
	{
		public bool IsBadRequest;
		public bool IsConflict;
		public string Message;
	}
}