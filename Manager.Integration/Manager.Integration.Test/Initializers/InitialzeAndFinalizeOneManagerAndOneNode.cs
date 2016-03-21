namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNode : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndOneNode() : base(1, 1, true)
		{
		}

		protected override void SetUp()
		{
			// Do nothing.
		}

		protected override void TearDown()
		{
			// Do nothing.
		}
	}
}