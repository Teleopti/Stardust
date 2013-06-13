using System;
using System.Collections.Generic;
using System.Drawing;
using System.ServiceModel;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests.ShiftTrade
{
    [TestFixture, SetUICulture("en-GB")]
    public class ShiftTradeTest
    {
        private MockRepository _mocks;
        private IShiftTradeView _view;
        private ITeleoptiSchedulingService _sdk;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<IShiftTradeView>();
            _sdk = _mocks.StrictMock<ITeleoptiSchedulingService>();
        }

        private ShiftTradeModel createModelWherePersonCreatedRequest(ShiftTradeStatusDto statusDto,
                                                                     RequestStatusDto totalStatus)
        {
            return createModel(statusDto, true, totalStatus);
        }

        private ShiftTradeModel createModelWherePersonGotRequest(ShiftTradeStatusDto statusDto,
                                                                 RequestStatusDto totalStatus)
        {
            return createModel(statusDto, false, totalStatus);
        }

        private ShiftTradeModel createModel(ShiftTradeStatusDto statusDto, bool personRequested,
                                            RequestStatusDto totalStatus)
        {
            PersonDto tradeFrom = createTradePersonDto("Sura Roger");
            PersonDto tradeWith = createTradePersonDto("Coola Peter");
            var dateTime = new DateTime(2009, 12, 27);
            PersonRequestDto personRequestDto = createPersonRequestDto(tradeFrom, tradeWith, personRequested,
                                                                       new List<DateTime> {dateTime}, totalStatus,
                                                                       statusDto);
            if (personRequested)
                return new ShiftTradeModel(_sdk, personRequestDto, tradeFrom, DateTime.Now);
            return new ShiftTradeModel(_sdk, personRequestDto, tradeWith, DateTime.Now);
        }

        private static PersonRequestDto createPersonRequestDto(PersonDto tradeFrom, PersonDto tradeWith,
                                                               bool personRequested, IList<DateTime> dateTimes,
                                                               RequestStatusDto totalStatus,
                                                               ShiftTradeStatusDto statusDto)
        {
            var personRequestDto = new PersonRequestDto {Message = "Message", Subject = "ShiftTrade"};
            var shiftTradeRequestDto = new ShiftTradeRequestDto();
            personRequestDto.Request = shiftTradeRequestDto;
            shiftTradeRequestDto.ShiftTradeStatus = statusDto;
            personRequestDto.RequestStatus = totalStatus;

            if (((statusDto == ShiftTradeStatusDto.OkByMe || statusDto == ShiftTradeStatusDto.Referred) && personRequested) &&
                (totalStatus == RequestStatusDto.Pending || totalStatus == RequestStatusDto.New))
                //This is done by the domain object on the serverside IRL.
                personRequestDto.CanDelete = true;

            personRequestDto.Person = tradeFrom;
            for (int i = 0; i < dateTimes.Count; i++)
            {
                shiftTradeRequestDto.ShiftTradeSwapDetails.Add(new ShiftTradeSwapDetailDto
                                                                    {
                                                                        DateFrom = new DateOnlyDto {DateTime = dateTimes[i]},
                                                                        DateTo = new DateOnlyDto {DateTime = dateTimes[i]},
                                                                        PersonFrom = tradeFrom,
                                                                        PersonTo = tradeWith
                                                                    });
            }
            personRequestDto.CreatedDate = shiftTradeRequestDto.ShiftTradeSwapDetails[0].DateFrom.DateTime;
            return personRequestDto;
        }

        private static PersonDto createTradePersonDto(string name)
        {
            var tradeFrom = new PersonDto {Name = name, Id = Guid.NewGuid()};
            return tradeFrom;
        }

        [Test]
        public void CanInitializeInUpdateStateAndNotPending()
        {
            PersonDto tradeFrom = createTradePersonDto("Sura Roger");
            PersonDto tradeWith = createTradePersonDto("Coola Peter");
            var dateTime = new DateTime(2009, 12, 27);

            PersonRequestDto personRequestDto = createPersonRequestDto(tradeFrom, tradeWith, true,
                                                                       new List<DateTime> {dateTime},
                                                                       RequestStatusDto.Denied,
                                                                       ShiftTradeStatusDto.OkByMe);
            var model = new ShiftTradeModel(_sdk, personRequestDto, tradeFrom, DateTime.Now);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.Message = personRequestDto.Message;
                _view.Subject = personRequestDto.Subject;
                _view.SetOkButtonVisible(false);
                _view.SetResponseTabEnabled(false);
                _view.SetLabelName(Resources.ToColon);
                _view.SetStatus(LanguageResourceHelper.TranslateEnumValue(RequestStatusDto.Denied));
            }
            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void CanInitializePresenter()
        {
            PersonDto tradeFrom = createTradePersonDto("Sura Roger");
            PersonDto tradeWith = createTradePersonDto("Coola Peter");
            var dateTime = new DateTime(2009, 12, 27);

            PersonRequestDto personRequestDto = createPersonRequestDto(tradeFrom, tradeWith, true,
                                                                       new List<DateTime> {dateTime},
                                                                       RequestStatusDto.Pending,
                                                                       ShiftTradeStatusDto.OkByMe);
            var model = new ShiftTradeModel(_sdk, personRequestDto, tradeFrom, DateTime.Now);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.Message = personRequestDto.Message;
                _view.Subject = personRequestDto.Subject;
                _view.SetResponseTabEnabled(true);
                _view.SetLabelName(Resources.ToColon);
                _view.SetStatus(
                    LanguageResourceHelper.TranslateEnumValue(
                        ((ShiftTradeRequestDto) personRequestDto.Request).ShiftTradeStatus));
            }
            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequestedPersonAtOkByBothParts()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(false);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequestedPersonAtOkByMe()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(true);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequestedPersonAtReferred()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.Referred,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(true);
                _view.SetAcceptButtonEnabled(true);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(true);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequesterAtOkByBothParts()
        {
            ShiftTradeModel model = createModelWherePersonGotRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                     RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(true);
                _view.SetDeleteButtonEnabled(false);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequesterAtOkByMe()
        {
            ShiftTradeModel model = createModelWherePersonGotRequest(ShiftTradeStatusDto.OkByMe,
                                                                     RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(true);
                _view.SetDenyButtonEnabled(true);
                _view.SetDeleteButtonEnabled(false);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonStatesForTradeRequesterAtReferred()
        {
            ShiftTradeModel model = createModelWherePersonGotRequest(ShiftTradeStatusDto.Referred,
                                                                     RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(true);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(false);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonsDisabledOnApprovedBySupervisor()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                         RequestStatusDto.Approved);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(false);
                _view.SetStatus(Resources.GrantedBySupervisor);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyButtonsDisabledOnDeniedBySupervisor()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                         RequestStatusDto.Denied);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.SetReasonMessageVisibility(false);
                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(false);
                _view.SetStatus(Resources.Denied);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyCanAccept()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.AcceptShiftTradeRequest(model.PersonRequestDto))
                    .Return(model.PersonRequestDto);

                _view.Close();
                _view.SetDialogResult(DialogResult.Yes);
            }

            using (_mocks.Playback())
            {
                target.Accept();
            }
        }

        [Test]
        public void VerifyCanAddDateRange()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.SetShiftTradeRequest(model.PersonRequestDto, string.Empty, string.Empty,
                                                                  new ShiftTradeSwapDetailDto[0]))
                    .IgnoreArguments().Return(model.PersonRequestDto);
                Expect.Call(_view.Subject).Return("The Subject goes here...");
                Expect.Call(_view.Message).Return("The Message goes here...");
                _view.RefreshVisualView();
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(1, model.SwapDetails.Count);
                Assert.AreEqual(2, model.ShiftTradeDetailModels.Count);
                target.AddDateRange(new DateOnlyDto {DateTime = model.InitialDate.AddDays(2)},
                                    new DateOnlyDto {DateTime = model.InitialDate.AddDays(3)});
                Assert.AreEqual(3, model.SwapDetails.Count);
                Assert.AreEqual(6, model.ShiftTradeDetailModels.Count);
            }
        }

        [Test]
        public void VerifyCanDelete()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                model.SdkService.DeletePersonRequest(model.PersonRequestDto);
                _view.SetDialogResult(DialogResult.Ignore);
            }

            using (_mocks.Playback())
            {
				target.Delete().Should().Be.True();
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "asdf"), Test]
		public void ShouldHandleDeleteError()
		{
			ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
																		 RequestStatusDto.Pending);
			var target = new ShiftTradePresenter(_view, model);

			using (_mocks.Record())
			{
                Expect.Call(() => model.SdkService.DeletePersonRequest(model.PersonRequestDto)).Throw(new FaultException("asdf"));
			}

			using (_mocks.Playback())
			{
				target.Delete().Should().Be.False();
			}
		}

        [Test]
        public void VerifyCanDeny()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.DenyShiftTradeRequest(model.PersonRequestDto))
                    .Return(model.PersonRequestDto);

                _view.Close();
                _view.SetDialogResult(DialogResult.No);
                //view.Action = PersonRequestActionType.Update;
            }

            using (_mocks.Playback())
            {
                target.Deny();
            }
        }

        [Test]
        public void VerifyCanFailAccept()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.AcceptShiftTradeRequest(model.PersonRequestDto)).Throw(new FaultException());

                Expect.Call(() => _view.ShowErrorMessage("", "")).IgnoreArguments();
                _view.Close();
                _view.SetDialogResult(DialogResult.Yes);
            }

            using (_mocks.Playback())
            {
                target.Accept();
            }
        }

        [Test]
        public void VerifyCanRemoveDays()
        {
            PersonDto tradeFrom = createTradePersonDto("Sura Roger");
            PersonDto tradeWith = createTradePersonDto("Coola Peter");
            var dateTime = new DateTime(2009, 12, 27);

            PersonRequestDto personRequestDto = createPersonRequestDto(tradeFrom, tradeWith, true,
                                                                       new List<DateTime>
                                                                           {
                                                                               dateTime,
                                                                               dateTime.AddDays(1),
                                                                               dateTime.AddDays(2)
                                                                           },
                                                                       RequestStatusDto.Pending,
                                                                       ShiftTradeStatusDto.OkByMe);

            var model = new ShiftTradeModel(_sdk, personRequestDto, tradeFrom, DateTime.Now);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.SetShiftTradeRequest(model.PersonRequestDto, string.Empty, string.Empty,
                                                                  new ShiftTradeSwapDetailDto[0]))
                    .IgnoreArguments().Return(model.PersonRequestDto);
                Expect.Call(_view.Subject).Return("The Subject goes here...");
                Expect.Call(_view.Message).Return("The Message goes here...");
                Expect.Call(_view.SelectedDates).Return(new List<DateTime>
                                                            {
                                                                dateTime.AddDays(1),
                                                                dateTime.AddDays(2),
                                                                dateTime.AddDays(3)
                                                            });
                _view.RefreshVisualView();
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(3, model.SwapDetails.Count);
                Assert.AreEqual(6, model.ShiftTradeDetailModels.Count);
                target.DeleteSelectedDates();
                Assert.AreEqual(1, model.SwapDetails.Count);
                Assert.AreEqual(2, model.ShiftTradeDetailModels.Count);
            }
        }

        [Test]
        public void VerifyCancel()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.Close();
                _view.SetDialogResult(DialogResult.Cancel);
            }

            using (_mocks.Playback())
            {
                target.Cancel();
            }
        }

        [Test]
        public void VerifyCannotRemoveLastDay()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                Expect.Call(model.SdkService.SetShiftTradeRequest(model.PersonRequestDto, string.Empty, string.Empty,
                                                                  new ShiftTradeSwapDetailDto[0]))
                    .IgnoreArguments().Return(model.PersonRequestDto);
                Expect.Call(_view.Subject).Return("The Subject goes here...");
                Expect.Call(_view.Message).Return("The Message goes here...");
                Expect.Call(_view.SelectedDates).Return(new List<DateTime> {model.SwapDetails[0].DateFrom.DateTime});
                _view.ShowErrorMessage(Resources.YouMustHaveAtLeastOneDateInShiftTradeRequest,
                                       Resources.DeleteDateParenthesisS);
                _view.RefreshVisualView();
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(1, model.SwapDetails.Count);
                Assert.AreEqual(2, model.ShiftTradeDetailModels.Count);
                target.DeleteSelectedDates();
                Assert.AreEqual(1, model.SwapDetails.Count);
                Assert.AreEqual(2, model.ShiftTradeDetailModels.Count);
            }
        }

        [Test]
        public void VerifyOk()
        {
            ShiftTradeModel model = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByBothParts,
                                                                         RequestStatusDto.Pending);
            var target = new ShiftTradePresenter(_view, model);

            using (_mocks.Record())
            {
                _view.Close();
                Expect.Call(_view.Subject).Return("The Subject goes here...").Repeat.Any();
                Expect.Call(_view.Message).Return("The Message goes here...").Repeat.Any();
                _view.SetDialogResult(DialogResult.OK);
            }

            using (_mocks.Playback())
            {
                target.Initialize();
                target.Ok(false);
                target.Ok(true);
            }
            Assert.AreEqual(model.Subject, "The Subject goes here...");
            Assert.AreEqual(model.Message, "The Message goes here...");
        }

		[Test]
		public void ShouldShowErrorMessageAndNotCloseWhenAbsenceInProjection()
		{
			var shiftTradeModel = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe, RequestStatusDto.Pending);

			var dateTimePeriodDto = new DateTimePeriodDto { LocalStartDateTime = new DateTime(2009, 11, 18, 02, 0, 0), LocalEndDateTime = new DateTime(2009, 11, 18, 03, 0, 0) };
			var schedulePartDto = new SchedulePartDto { Date = new DateOnlyDto { DateTime = new DateTime(2009, 11, 17) } };
            schedulePartDto.ProjectedLayerCollection.Add(new ProjectedLayerDto { IsAbsence = true, Description = "Act3", DisplayColor = new ColorDto(Color.DodgerBlue), Period = dateTimePeriodDto });
			var personDto = new PersonDto();
			var shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto { DateFrom = new DateOnlyDto { DateTime = new DateTime(2009, 11, 17) } };
			shiftTradeSwapDetailDto.DateTo = shiftTradeSwapDetailDto.DateFrom;
			shiftTradeSwapDetailDto.SchedulePartFrom = schedulePartDto;
			shiftTradeSwapDetailDto.SchedulePartTo = schedulePartDto;
			var shiftTradeDetailModel = new ShiftTradeDetailModel(shiftTradeSwapDetailDto, schedulePartDto, personDto, personDto);
			shiftTradeModel.ShiftTradeDetailModels.Add(shiftTradeDetailModel);

			var target = new ShiftTradePresenter(_view, shiftTradeModel);

			using (_mocks.Record())
			{
				Expect.Call(() => _view.ShowErrorMessage(Resources.ShiftTradeAbsenceDenyReason, Resources.ShiftTradeRequest)).IgnoreArguments();
			}

			using (_mocks.Playback())
			{
				target.Ok(false);
			}
		}

        [Test]
        public void ShouldShowErrorMessageAndNotCloseWhenNoSubject()
        {
            var shiftTradeModel = createModelWherePersonCreatedRequest(ShiftTradeStatusDto.OkByMe, RequestStatusDto.Pending);
            shiftTradeModel.Subject = string.Empty;

            var target = new ShiftTradePresenter(_view, shiftTradeModel);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.ShowErrorMessage(Resources.PersonRequestEmptySubjectError, Resources.ShiftTradeRequest));
            }

            using (_mocks.Playback())
            {
                target.Ok(false);
            }
        }
    }
}