using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class SkillTransformerTest
	{
		[SetUp]
		public void Setup()
		{
			_skillCollection = SkillFactory.CreateSkillList();
			var activityTransformer = new SkillTransformer(_insertDateTime);
			_table = new DataTable();
			_table.Locale = Thread.CurrentThread.CurrentCulture;
			SkillInfrastructure.AddColumnsToDataTable(_table);
			activityTransformer.Transform(_skillCollection, _table);
		}

		private IList<ISkill> _skillCollection;
		private readonly DateTime _insertDateTime = DateTime.Now;
		private DataTable _table;

		[Test]
		public void VerifySkill()
		{
			Assert.AreEqual(2, _table.Rows.Count);
			Assert.AreEqual(_skillCollection[0].Id, _table.Rows[0]["skill_code"]);
			Assert.AreEqual(_skillCollection[0].Name, _table.Rows[0]["skill_name"]);
			Assert.AreEqual(_skillCollection[0].SkillType.Id, _table.Rows[0]["forecast_method_code"]);
		}

		[Test]
		public void VerifyAggregateRoot()
		{
			//BusinessUnit
			Assert.AreEqual(_skillCollection[0].BusinessUnit.Id, _table.Rows[0]["business_unit_code"]);
			Assert.AreEqual(_skillCollection[1].BusinessUnit.Description.Name, _table.Rows[1]["business_unit_name"]);
			//UpdatedOn
			Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_skillCollection[0]),
								 _table.Rows[0]["datasource_update_date"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _table.Rows[0]["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[0]["insert_date"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[1]["update_date"]);
		}

		[Test]
		public void VerifyIsDeleted()
		{
			Assert.IsFalse((bool)_table.Rows[0]["is_deleted"]);
			Assert.IsTrue((bool)_table.Rows[1]["is_deleted"]);
		}
	}
}