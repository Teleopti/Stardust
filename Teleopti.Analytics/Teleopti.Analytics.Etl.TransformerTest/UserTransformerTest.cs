using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class UserTransformerTest
	{
		private UserTransformer _target;

		[SetUp]
		public void Setup()
		{
			_target = new UserTransformer();
		}

		private DataRowCollection TransformPersonsToUsersRows(IPerson person)
		{
			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				UserInfrastructure.AddColumnsToDataTable(table);

				_target.Transform(new List<IPerson> { person }, table);

				return table.Rows;
			}
		}

		[Test]
		public void VerifyUser()
		{
			IPerson personUser = FakeData.UserFactory.CreatePersonUser();
			var rows = TransformPersonsToUsersRows(personUser);
			
			Assert.AreEqual(1, rows.Count);
			DataRow row = rows[0];

			Assert.AreEqual(personUser.Id, row["person_code"]);
			Assert.AreEqual(personUser.Name.FirstName, row["person_first_name"]);
			Assert.AreEqual(personUser.Name.LastName, row["person_last_name"]);
			Assert.AreEqual(personUser.ApplicationAuthenticationInfo.ApplicationLogOnName,
							row["application_logon_name"]);
			var windowsIdentity = personUser.AuthenticationInfo.Identity.Split('\\');
			Assert.AreEqual(windowsIdentity[1],
							row["windows_logon_name"]);
			Assert.AreEqual(windowsIdentity[0],
							row["windows_domain_name"]);
			Assert.AreEqual(personUser.ApplicationAuthenticationInfo.Password, row["password"]);
			Assert.AreEqual(personUser.Email, row["email"]);
			Assert.AreEqual(personUser.PermissionInformation.UICultureLCID().GetValueOrDefault(-1), row["language_id"]);
			Assert.AreEqual(System.DBNull.Value, row["language_name"]);
			Assert.AreEqual(personUser.PermissionInformation.CultureLCID().GetValueOrDefault(-1), row["culture"]);
			Assert.AreEqual(personUser.UpdatedOn, row["datasource_update_date"]);
		}

		[Test]
		public void VerifyUserWithoutCultureSet()
		{
			IPerson personUser = FakeData.UserFactory.CreatePersonUserWithNoCultureSet();
			var rows = TransformPersonsToUsersRows(personUser);
			DataRow row = rows[0];

			Assert.AreEqual(personUser.PermissionInformation.UICultureLCID().GetValueOrDefault(-1), row["language_id"]);
			Assert.AreEqual(System.DBNull.Value, row["language_name"]);
			Assert.AreEqual(personUser.PermissionInformation.CultureLCID().GetValueOrDefault(-1), row["culture"]);
		}

		[Test]
		public void ShouldTransformWindowsLogOnAsEmptyString()
		{
			IPerson personUser = FakeData.UserFactory.CreatePersonUserWithNoWindowsAuthentication();
			var rows = TransformPersonsToUsersRows(personUser);
			DataRow row = rows[0];

			Assert.AreEqual(string.Empty, row["windows_logon_name"]);
			Assert.AreEqual(string.Empty, row["windows_domain_name"]);
		}

		[Test]
		public void ShouldTransformApplicationLogOnAsEmptyString()
		{
			IPerson personUser = FakeData.UserFactory.CreatePersonUserWithNoApplicationAuthentication();
			var rows = TransformPersonsToUsersRows(personUser);
			DataRow row = rows[0];

			Assert.AreEqual(string.Empty, row["application_logon_name"]);
			Assert.AreEqual(string.Empty, row["password"]);
		}
	}
}
