namespace Manager.Integration.Test.Initializers
{
	public class InitialzeAndFinalizeOneManagerAndOneNodeNoLadBalancer : InitializeAndFinalizeBase
	{
		public InitialzeAndFinalizeOneManagerAndOneNodeNoLadBalancer() : base(1, 1, false)
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