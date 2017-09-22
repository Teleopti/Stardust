using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule
{
	[TestFixture, DomainTest]
	public class FavoriteSearchProviderTest: ISetup
	{
		public IFavoriteSearchProvider Target;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeFavoriteSearchRepository FavoriteSearchRepository;

		private IPerson me;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FavoriteSearchProvider>().For<IFavoriteSearchProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeFavoriteSearchRepository>().For<IFavoriteSearchRepository>();
		}

		[Test]
		public void ShouldGetSortedFavoritesCreatedByUserInGivenArea()
		{
			me = PersonFactory.CreatePerson("ashley").WithId();
			var john = PersonFactory.CreatePerson("john").WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var fav1 = new FavoriteSearch("ashley1").WithId();
			fav1.Creator = me;
			fav1.WfmArea = WfmArea.Teams;
			var fav2 = new FavoriteSearch("john").WithId();
			fav2.Creator = john;
			fav2.WfmArea = WfmArea.Teams;
			var fav3 = new FavoriteSearch("ashley0").WithId();
			fav3.Creator = me;
			fav3.WfmArea = WfmArea.Teams;

			FavoriteSearchRepository.Add(fav1);
			FavoriteSearchRepository.Add(fav2);
			FavoriteSearchRepository.Add(fav3);

			var results = Target.GetAllForCurrentUser(WfmArea.Teams);

			Assert.AreEqual(2, results.Count());
			results[0].Name.Should().Be.EqualTo("ashley0");
		}

	}
}
