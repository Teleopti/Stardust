using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class InfrastructureTestSetup
	{
		private static IPerson _person;
		private static IBusinessUnit _businessUnit;

		public static (IPerson Person, IBusinessUnit BusinessUnit) Before()
		{
			DatabaseTestSetup.Setup(context =>
			{
				BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();
				_businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
				_person = PersonFactory.CreatePerson(RandomName.Make());

				context.UpdatedByScope.OnThisThreadUse(_person);
				using (var uow = context.DataSource.Application.CreateAndOpenUnitOfWork())
				{
					var session = uow.FetchSession();

					((IDeleteTag) _person).SetDeleted();
					session.Save(_person);

					//force a insert
					var businessUntId = _businessUnit.Id.Value;
					_businessUnit.SetId(null);
					session.Save(_businessUnit, businessUntId);
					session.Flush();

					uow.PersistAll();
				}

				context.UpdatedByScope.OnThisThreadUse(null);

				return 254875;
			});

			return (_person, _businessUnit);
		}

		public static void After()
		{
		}
	}
}