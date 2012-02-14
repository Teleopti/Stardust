using System;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests.RequestMaster
{
    [TestFixture]
    public class RequestMasterTest
    {
        private MockRepository mocks;
        private IRequestMasterView view;
        private ITeleoptiSchedulingService sdk;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            view = mocks.StrictMock<IRequestMasterView>();
            sdk = mocks.StrictMock<ITeleoptiSchedulingService>();
        }

        [Test]
        public void CanInitializePresenter()
        {
            RequestMasterModel model = createModel();
            RequestMasterPresenter target = new RequestMasterPresenter(view, model);

            using (mocks.Record())
            {
                Expect.Call(view.DataSource).PropertyBehavior();
                //= model.DataSource;
                view.RequestDateHeader = model.RequestDateHeader;
                view.RequestTypeHeader = model.RequestTypeHeader;
                view.RequestStatusHeader = model.RequestStatusHeader;
                view.DetailsHeader = model.DetailsHeader;
                view.SubjectHeader = model.SubjectHeader;
                view.MessageHeader = model.MessageHeader;
                view.LastChangedHeader = model.LastChangedHeader;
                PersonRequestDto[] requestDtos = new PersonRequestDto[1];
                requestDtos[0] = model.DataSource[0].PersonRequest; //Return same as model
                Expect.Call(sdk.GetAllRequestModifiedWithinPeriodOrPending(new PersonDto(), new DateTime(), true, new DateTime(), true)).Return(requestDtos).IgnoreArguments(); 
            }
            using (mocks.Playback())
            {
                target.Initialize();
                Assert.IsNotEmpty(model.MessageHeader);
                Assert.IsNotEmpty(model.RequestDateHeader);
                Assert.IsNotEmpty(model.RequestStatusHeader);
                Assert.IsNotEmpty(model.RequestTypeHeader);
                Assert.IsNotEmpty(model.SubjectHeader);
                Assert.IsNotEmpty(model.DetailsHeader);
                Assert.AreEqual(model.DataSource[0].LastChanged, new DateTime(2008, 12, 23, 3, 12, 0));
                Assert.AreEqual(model.DataSource[0].Message, "Apa");
                Assert.IsNotEmpty(model.DataSource[0].RequestDate); // "2009-12-02 - 2009-12-03" varies by culture
                Assert.AreEqual(model.DataSource[0].Subject, "Subject");
                Assert.IsNotEmpty(model.DataSource[0].RequestType); // varies by language , "ShiftTrade"
                Assert.IsNotEmpty(model.DataSource[0].RequestDate); // varies by culture
                Assert.IsNotNull(model.DataSource[0].RequestStatus); // varies by language
                Assert.IsNotEmpty(model.DataSource[0].Details); //This is "calculated" in the domain
            }
        }

        [Test]
        public void CanDeleteRequest()
        {
            RequestMasterModel model = createModel();
            RequestMasterPresenter target = new RequestMasterPresenter(view, model);

            using (mocks.Record())
            {
                Expect.Call(view.DataSource).PropertyBehavior();
                 //= model.DataSource;
                view.RequestDateHeader = model.RequestDateHeader;
                view.RequestTypeHeader = model.RequestTypeHeader;
                view.RequestStatusHeader = model.RequestStatusHeader;
                view.DetailsHeader = model.DetailsHeader;
                view.SubjectHeader = model.SubjectHeader;
                view.MessageHeader = model.MessageHeader;
                view.LastChangedHeader = model.LastChangedHeader;
                sdk.DeletePersonRequest(model.DataSource[0].PersonRequest);
                PersonRequestDto[] requestDtos = new PersonRequestDto[1];
                requestDtos[0] = model.DataSource[0].PersonRequest; //Return same as model
                //First call in initialize and the second when reloading after delete
                Expect.Call(sdk.GetAllRequestModifiedWithinPeriodOrPending(new PersonDto(), new DateTime(), true, new DateTime(), true)).Return(requestDtos).IgnoreArguments().Repeat.Twice(); 
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.DeletePersonRequests(model.DataSource);
            }
        }

        [Test]
        public void CanSortByColumn()
        {
            RequestMasterModel model = createModel();
            RequestMasterPresenter target = new RequestMasterPresenter(view, model);

            using (mocks.Record())
            {
                Expect.Call(view.DataSource).PropertyBehavior();
                //= model.DataSource;
                view.RequestDateHeader = model.RequestDateHeader;
                view.RequestTypeHeader = model.RequestTypeHeader;
                view.RequestStatusHeader = model.RequestStatusHeader;
                view.DetailsHeader = model.DetailsHeader;
                view.SubjectHeader = model.SubjectHeader;
                view.MessageHeader = model.MessageHeader;
                view.LastChangedHeader = model.LastChangedHeader;
                PersonRequestDto[] requestDtos = new PersonRequestDto[3];
                requestDtos[0] = model.DataSource[0].PersonRequest; //Return same as model
                requestDtos[1] = model.DataSource[1].PersonRequest; //Return same as model
                requestDtos[2] = model.DataSource[2].PersonRequest; //Return same as model

                Expect.Call(sdk.GetAllRequestModifiedWithinPeriodOrPending(new PersonDto(), new DateTime(), true, new DateTime(), true)).Return(requestDtos).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target.Initialize();
                Assert.AreEqual("Hamster", model.DataSource[0].Message);
                Assert.AreEqual("Zebra", model.DataSource[1].Message);
                Assert.AreEqual("Apa", model.DataSource[2].Message);

                target.SortByColumn("Message",ListSortDirection.Ascending);
                Assert.AreEqual("Apa", model.DataSource[0].Message);
                Assert.AreEqual("Hamster", model.DataSource[1].Message);
                Assert.AreEqual("Zebra", model.DataSource[2].Message);

                target.SortByColumn("Message", ListSortDirection.Descending);
                Assert.AreEqual("Zebra", model.DataSource[0].Message);
                Assert.AreEqual("Hamster", model.DataSource[1].Message);
                Assert.AreEqual("Apa", model.DataSource[2].Message);
            }
        }

        [Test] //dont remove this shit yet Hank
        public void CanHandleBadPropertyName()
        {
            RequestMasterModel model = createModel();
            RequestMasterPresenter target = new RequestMasterPresenter(view, model);

            using (mocks.Record())
            {
                Expect.Call(view.DataSource).PropertyBehavior();
                //= model.DataSource;
                view.RequestDateHeader = model.RequestDateHeader;
                view.RequestTypeHeader = model.RequestTypeHeader;
                view.RequestStatusHeader = model.RequestStatusHeader;
                view.DetailsHeader = model.DetailsHeader;
                view.SubjectHeader = model.SubjectHeader;
                view.MessageHeader = model.MessageHeader;
                view.LastChangedHeader = model.LastChangedHeader;
                PersonRequestDto[] requestDtos = new PersonRequestDto[3];
                requestDtos[0] = model.DataSource[0].PersonRequest; //Return same as model
                requestDtos[1] = model.DataSource[1].PersonRequest; //Return same as model
                requestDtos[2] = model.DataSource[2].PersonRequest; //Return same as model

                Expect.Call(sdk.GetAllRequestModifiedWithinPeriodOrPending(new PersonDto(), new DateTime(), true, new DateTime(), true)).Return(requestDtos).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target.Initialize();
                Assert.AreEqual("Hamster", model.DataSource[0].Message);
                Assert.AreEqual("Zebra", model.DataSource[1].Message);
                Assert.AreEqual("Apa", model.DataSource[2].Message);

                target.SortByColumn("PropertyNameNotExisting", ListSortDirection.Ascending);
                //Leave the list in initialstate
                Assert.AreEqual("Hamster", model.DataSource[0].Message);
                Assert.AreEqual("Zebra", model.DataSource[1].Message);
                Assert.AreEqual("Apa", model.DataSource[2].Message);
            }
        }

        private RequestMasterModel createModel()
        {
            BindingList<RequestDetailRow> list = new BindingList<RequestDetailRow>();
            PersonDto loggedOnPerson = new PersonDto {Id = Guid.NewGuid().ToString(), Name = "Pelle"};
            list.Add(new RequestDetailRow(getPersonRequestDto("Apa"),loggedOnPerson));
            list.Add(new RequestDetailRow(getPersonRequestDto("Zebra"),loggedOnPerson));
            list.Add(new RequestDetailRow(getPersonRequestDto("Hamster"),loggedOnPerson));
            return new RequestMasterModel(sdk, list);
        }

        private static PersonRequestDto getPersonRequestDto(string message)
        {
            PersonRequestDto personRequestDto = new PersonRequestDto();
            personRequestDto.Message = message;
            personRequestDto.Person = new PersonDto { Name = "Uffe" };
            personRequestDto.RequestedDateLocal = new DateTime(2009, 12, 23, 3, 12, 0);
            personRequestDto.RequestStatus = RequestStatusDto.Approved;
            personRequestDto.Subject = "Subject";
            personRequestDto.CanDelete = true;
            ShiftTradeSwapDetailDto[] detailDtos = new ShiftTradeSwapDetailDto[1];
            ShiftTradeSwapDetailDto detailDto = new ShiftTradeSwapDetailDto();
            detailDto.DateFrom = new DateOnlyDto {DateTime = new DateTime(2009,11,14)};
            detailDto.PersonFrom = new PersonDto {Name = "Anders"};
            detailDto.PersonTo = new PersonDto {Name = "Ralf"};
            detailDtos[0] = detailDto;

            personRequestDto.Request = new ShiftTradeRequestDto
                                           {
                                                            Id="1", Details = "Details",
                                                            ShiftTradeSwapDetails = detailDtos,
                                                            Period=new DateTimePeriodDto
                                                                       {
                                                                                         LocalStartDateTime = new DateTime(2009,12,2),
                                                                                         LocalEndDateTime = new DateTime(2009,12,3)
                                                                                     }
                                                            };
            personRequestDto.UpdatedOn = new DateTime(2008, 12, 23, 3, 12, 0);
            return personRequestDto;
        }
    }
}
