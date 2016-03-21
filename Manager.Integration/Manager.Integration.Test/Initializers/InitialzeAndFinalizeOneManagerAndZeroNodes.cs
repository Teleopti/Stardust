namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndZeroNodes : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndZeroNodes() : base(0, 1, true)
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