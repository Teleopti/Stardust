using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromRequestStepDefinitions 
	{
		[Given(@"I have an approved absence request")]
		public void GivenIHaveAnApprovedAbsenceRequest()
		{
			UserFactory.User().Setup(new ExistingApprovedAbsenceRequest());
		}



		[When(@"I click the absence request's delete button")]
		public void WhenIClickTheAbsenceRequestSDeleteButton()
		{
			var requestId = UserFactory.User().UserData<ExistingAbsenceRequest>().PersonRequest.Id.Value;
			Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId).EventualClick();
		}



		[Then(@"I should not be able to input values for absence request")]
		public void ThenIShouldNotBeAbleToInputValuesForAbsenceRequest()
		{
			const string disabledAttr = "disabled";
			const string readonlyAttr = "readonly";
			var detailForm = Pages.Pages.CurrentEditRequestPage;
			EventualAssert.That(() => detailForm.RequestDetailFromDateTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "TextRequestDetailFromDateInput");
			EventualAssert.That(() => detailForm.RequestDetailFromTimeTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailFromTimeTextField");
			EventualAssert.That(() => detailForm.RequestDetailSubjectInput.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailSubjectInput");
			EventualAssert.That(() => detailForm.RequestDetailToDateTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailToDateTextField");
			EventualAssert.That(() => detailForm.RequestDetailToTimeTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailToTimeTextField");
			EventualAssert.That(() => detailForm.RequestDetailMessageTextField.GetAttributeValue(readonlyAttr), Is.EqualTo("True"), "RequestDetailMessageTextField");
			EventualAssert.That(() => detailForm.AbsenceTypesTextField.GetAttributeValue(readonlyAttr), Is.EqualTo("True"), "AbsenceTypesTextField");
			EventualAssert.That(() => detailForm.FulldayCheck.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "FulldayCheck");
		}

		[Then(@"I should see the absence request in the list")]
		public void ThenIShouldSeeTheAbsenceRequestInTheList()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.InnerHtml, Is.StringContaining("Vacation"));
		}

		[Then(@"I should see the absence request's details form")]
		public void ThenIShouldSeeTheAbsenceRequestSDetailsForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the absence request's values")]
		public void ThenIShouldSeeTheAbsenceRequestSValues()
		{
			var request = UserFactory.User().UserData<ExistingAbsenceRequest>();

			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.Date));
			EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.TimeOfDay));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value,
																		Is.EqualTo(request.PersonRequest.GetMessage(new NoFormatting())));

			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceTypesTextField.Value,
																		Is.EqualTo(request.AbsenceRequest.Absence.Description.Name));

			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value,
																		Is.EqualTo(request.PersonRequest.GetSubject(new NoFormatting())));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.Date));
			EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.TimeOfDay));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailEntityId.Value, Is.EqualTo(request.PersonRequest.Id.ToString()));
		}

		[Then(@"I should not see the text request tab \(invisible\)")]
		public void ThenIShouldNotSeeTheTextRequestTabInvisible()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestTab.DisplayHidden(), Is.True);
		}
	}
}
