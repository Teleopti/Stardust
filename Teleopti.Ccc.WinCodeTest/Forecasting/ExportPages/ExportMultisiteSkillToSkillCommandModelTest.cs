using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
	[TestFixture]
	public class ExportMultisiteSkillToSkillCommandModelTest
	{
		private ExportMultisiteSkillToSkillCommandModel target;
		private Guid multisiteSkillGuid;
		private Guid? sourceSkillGuid;
		private Guid? targetSkillGuid;
		private DateOnlyPeriod period;


		[SetUp]
		public void Setup()
		{
			target = new ExportMultisiteSkillToSkillCommandModel();
			multisiteSkillGuid = Guid.NewGuid();
			sourceSkillGuid = Guid.NewGuid();
			targetSkillGuid = Guid.NewGuid();
			period = new DateOnlyPeriod
				(
					new DateOnly(2010, 12, 13),
					new DateOnly(2010, 12, 22)
				);

			var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel();

			multisiteSkillSelectionModel.MultisiteSkillModel = new MultisiteSkillModel(multisiteSkillGuid);
			IChildSkill source = new ChildSkill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillType());
			source.SetId(sourceSkillGuid);
			ISkill selected = new Skill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillType());
			selected.SetId(targetSkillGuid);
			var childSkillMappingModel = new ChildSkillMappingModel(sourceSkillGuid.GetValueOrDefault(),
																					  selected.Id.GetValueOrDefault(),
																					  selected.BusinessUnit.Name, selected.Name);

			multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel);

			target.MultisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);
			target.Period = period;
		}


		[Test]
		public void ShouldHaveChildSkillMappings()
		{
			target.HasChildSkillMappings.Should().Be.True();

			target.MultisiteSkillSelectionModels.Clear();

			target.HasChildSkillMappings.Should().Be.False();
		}
	}
}
