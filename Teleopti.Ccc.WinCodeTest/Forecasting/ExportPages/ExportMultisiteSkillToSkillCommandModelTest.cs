using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
	[TestFixture]
	public class ExportMultisiteSkillToSkillCommandModelTest
	{
		[Test]
		public void ShouldHaveChildSkillMappings()
		{
			var target = new ExportMultisiteSkillToSkillCommandModel();
			var multisiteSkillGuid = Guid.NewGuid();
			var sourceSkillGuid = Guid.NewGuid();
			var targetSkillGuid = Guid.NewGuid();
			var period = new DateOnlyPeriod(2010, 12, 13, 2010, 12, 22);

			var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel();

			multisiteSkillSelectionModel.MultisiteSkillModel = new MultisiteSkillModel(multisiteSkillGuid);
			var selected = new Skill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillTypePhone()).WithId(targetSkillGuid);
			var childSkillMappingModel = new ChildSkillMappingModel(sourceSkillGuid, selected.Id.GetValueOrDefault(),
				selected.GetOrFillWithBusinessUnit_DONTUSE().Name, selected.Name);

			multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel);

			target.MultisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);
			target.Period = period;

			target.HasChildSkillMappings.Should().Be.True();

			target.MultisiteSkillSelectionModels.Clear();

			target.HasChildSkillMappings.Should().Be.False();
		}
	}
}
