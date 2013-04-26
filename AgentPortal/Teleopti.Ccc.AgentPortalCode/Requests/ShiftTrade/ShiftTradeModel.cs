using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public class ShiftTradeModel
    {
        private string _personName;
        private PersonRequestDto _personRequestDto;
        private readonly ShiftTradeRequestDto _shiftTradeRequestDto;
        private readonly IList<ShiftTradeDetailModel> _shiftTradeDetailModels = new List<ShiftTradeDetailModel>();
        private readonly PersonDto _loggedOnPerson;
        private readonly DateTime _initialDate;

        public ShiftTradeModel(ITeleoptiSchedulingService sdkService, 
                                PersonRequestDto personRequestDto, 
                                PersonDto loggedOnPerson, 
                                DateTime initialDate)
        {
            SdkService = sdkService;
            _initialDate = initialDate;
            _personRequestDto = personRequestDto;
            _shiftTradeRequestDto = _personRequestDto.Request as ShiftTradeRequestDto;
            _loggedOnPerson = loggedOnPerson;

            if (_shiftTradeRequestDto != null &&
                _shiftTradeRequestDto.ShiftTradeSwapDetails.Count > 0)
            {
                PersonTo = _shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonTo;
            }
            CreateShiftTradeDetails();
        }

        public void CreateShiftTradeDetails()
        {
            ShiftTradeDetailModels.Clear();

            if (_shiftTradeRequestDto==null || _shiftTradeRequestDto.ShiftTradeSwapDetails==null) return;
            var listToSort = new List<ShiftTradeSwapDetailDto>(_shiftTradeRequestDto.ShiftTradeSwapDetails);
            listToSort.Sort((s1, s2) => s1.DateFrom.DateTime.CompareTo(s2.DateFrom.DateTime));
            foreach (var shiftTradeSwapDetail in listToSort)
            {
                ShiftTradeDetailModels.Add(new ShiftTradeDetailModel(shiftTradeSwapDetail,
                                                                     shiftTradeSwapDetail.SchedulePartFrom,
                                                                     shiftTradeSwapDetail.PersonFrom, _loggedOnPerson));
                ShiftTradeDetailModels.Add(new ShiftTradeDetailModel(shiftTradeSwapDetail,
                                                                     shiftTradeSwapDetail.SchedulePartTo,
                                                                     shiftTradeSwapDetail.PersonTo, _loggedOnPerson));
            }
        }

        public ITeleoptiSchedulingService SdkService { get; private set; }

        public DateTime InitialDate
        {
            get { return _initialDate; }
        }

        public string Message
        {
            get { return _personRequestDto.Message;}
            set { _personRequestDto.Message = value; }
        }

        public string Subject
        {
            get { return _personRequestDto.Subject;}
            set { _personRequestDto.Subject = value; }
        }

        public PersonDto PersonTo
        {
            get; private set;
        }

        public string PersonName
        {
            get
            {
                _personName = _personRequestDto.Person.Name;
                if (_shiftTradeRequestDto.ShiftTradeSwapDetails.Count>0 &&
                    LoggedOnPerson.Id != _shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonTo.Id)
                {
                    _personName = _shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonTo.Name;
                }

                return _personName;
            }
        }

        public string LabelName
        {
            get { return UserTexts.Resources.ToColon; }
        }

        public ShiftTradeStatusDto ShiftTradeStatus
        {
            get { return _shiftTradeRequestDto.ShiftTradeStatus; }
        }

        public PersonRequestDto PersonRequestDto
        {
            get { return _personRequestDto; }
            set { _personRequestDto = value; }
        }

        public PersonDto LoggedOnPerson
        {
            get { return _loggedOnPerson; }
        }

        public List<ShiftTradeSwapDetailDto> SwapDetails
        {
            get { return new List<ShiftTradeSwapDetailDto>(_shiftTradeRequestDto.ShiftTradeSwapDetails); }
        }

        public IList<ShiftTradeDetailModel> ShiftTradeDetailModels
        {
            get { return _shiftTradeDetailModels; }
        }

        public ShiftTradeSwapDetailDto FindSwapDetailByDate(DateTime dateToFind)
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = null;
            foreach (var swapDetail in SwapDetails)
            {
                if (swapDetail.DateFrom.DateTime == dateToFind)
                {
                    shiftTradeSwapDetailDto = swapDetail;
                    break;
                }
            }
            return shiftTradeSwapDetailDto;
        }

        public void DeleteDate(DateTime dateToDelete)
        {
            var shiftTradeSwapDetailDtoToRemove = FindSwapDetailByDate(dateToDelete);
            if (shiftTradeSwapDetailDtoToRemove != null)
            {
                var swapDetails = SwapDetails;
                swapDetails.Remove(shiftTradeSwapDetailDtoToRemove);
                _shiftTradeRequestDto.ShiftTradeSwapDetails.Clear();
                foreach (var shiftTradeSwapDetailDto in swapDetails)
                {
                    _shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);
                }
            }
            CreateShiftTradeDetails();
        }

        public void AddDateRange(DateTime startDate,DateTime endDate)
        {
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date<DateTime.Today) continue;

                var shiftTradeSwapDetailDto = FindSwapDetailByDate(date);
                if (shiftTradeSwapDetailDto==null)
                {
                    shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
                    shiftTradeSwapDetailDto.ChecksumFrom = 0;
                    shiftTradeSwapDetailDto.ChecksumTo = 0;
                    shiftTradeSwapDetailDto.DateFrom = new DateOnlyDto {DateTime = date};
                    shiftTradeSwapDetailDto.DateTo = new DateOnlyDto { DateTime = date };
                    shiftTradeSwapDetailDto.PersonFrom = _personRequestDto.Person;
                    shiftTradeSwapDetailDto.PersonTo = PersonTo;

                    _shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);
                }
            }

            CreateShiftTradeDetails();
        }
    }
}