namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class Tenant
	{
		//TODO: tenant, when we move to seperate db, we can remove default name here (and in db)
		public const string DefaultName = "Teleopti WFM";

#pragma warning disable 169
		private int id;
#pragma warning restore 169

		public virtual string Name { get; protected set; }

		//TODO: tenant
		public virtual void SetName_DoNotUseThisOneIfYouDoNotKnowWhatYouAreDoing(string name)
		{
			Name = name;
		}
	}
}