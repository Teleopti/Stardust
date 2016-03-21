namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeNoLoadBalancer : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeNoLoadBalancer() : base(1, 1, false)
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