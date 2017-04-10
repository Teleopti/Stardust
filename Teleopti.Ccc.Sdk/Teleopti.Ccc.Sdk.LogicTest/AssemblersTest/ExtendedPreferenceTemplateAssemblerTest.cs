using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ExtendedPreferenceTemplateAssemblerTest
    {
	    [Test]
	    public void VerifyDomainEntityAndDto()
	    {
		    var restrictionAssembler =
			    new RestrictionAssembler
				    <IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto, IActivityRestrictionTemplate>(
				    new PreferenceRestrictionTemplateConstructor(), new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					    new DayOffAssembler(new FakeDayOffTemplateRepository()),
					    new ActivityRestrictionAssembler<IActivityRestrictionTemplate>(
						    new ActivityRestrictionTemplateDomainObjectCreator(), new ActivityAssembler(new FakeActivityRepository())),
					    new AbsenceAssembler(new FakeAbsenceRepository()));
		    var person = PersonFactory.CreatePerson().WithId();
		    var target = new ExtendedPreferenceTemplateAssembler(person, restrictionAssembler);
		    var template = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "temptest",
			    Color.SteelBlue);

		    var dto = target.DomainEntityToDto(template);

		    Assert.AreEqual(dto.Name, template.Name);

		    Assert.AreEqual(dto.DisplayColor.Blue, template.DisplayColor.B);
		    Assert.AreEqual(dto.DisplayColor.Red, template.DisplayColor.R);
		    Assert.AreEqual(dto.DisplayColor.Green, template.DisplayColor.G);
		    Assert.AreEqual(dto.DisplayColor.Alpha, template.DisplayColor.A);
	    }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var restrictionAssembler =
			    new RestrictionAssembler
				    <IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto, IActivityRestrictionTemplate>(
				    new PreferenceRestrictionTemplateConstructor(), new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					    new DayOffAssembler(new FakeDayOffTemplateRepository()),
					    new ActivityRestrictionAssembler<IActivityRestrictionTemplate>(
						    new ActivityRestrictionTemplateDomainObjectCreator(), new ActivityAssembler(new FakeActivityRepository())),
					    new AbsenceAssembler(new FakeAbsenceRepository()));
		    var person = PersonFactory.CreatePerson().WithId();
		    var target = new ExtendedPreferenceTemplateAssembler(person, restrictionAssembler);
		    var templateDto = new ExtendedPreferenceTemplateDto();
		    
		    templateDto.DisplayColor = new ColorDto(Color.DodgerBlue);
		    templateDto.Name = "Kanin";
		    var template = target.DtoToDomainEntity(templateDto);

		    Assert.AreEqual("Kanin", template.Name);
		    Assert.AreEqual(Color.DodgerBlue.B, template.DisplayColor.B);
		    Assert.AreEqual(person, template.Person);
	    }
    }
}
