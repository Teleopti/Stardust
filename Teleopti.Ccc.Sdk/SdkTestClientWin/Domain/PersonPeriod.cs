using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
	public class PersonPeriod
	{
		private readonly PersonPeriodDetailDto _personPeriodDto;
		private readonly ContractDto _contractDto;
		private readonly PartTimePercentageDto _partTimePercentageDto;
		private readonly ContractScheduleDto _contractScheduleDto;
		private readonly PersonDto _personDto;

		public PersonPeriod(PersonPeriodDetailDto personPeriodDto, PersonDto persondDto, ContractDto contractDto, PartTimePercentageDto partTimePercentageDto, ContractScheduleDto contractScheduleDto)
		{
			_personPeriodDto = personPeriodDto;
			_contractDto = contractDto;
			_partTimePercentageDto = partTimePercentageDto;
			_contractScheduleDto = contractScheduleDto;
			_personDto = persondDto;
		}

		public string PersonId
		{
			get { return _personDto != null ? _personDto.Id : string.Empty; }
		}

		public string ContractDescription
		{
			get { return _contractDto != null ? _contractDto.Description : string.Empty; }
		}

		public string PartTimePercentageDescription
		{
			get { return _partTimePercentageDto != null ? _partTimePercentageDto.Description : string.Empty; }
		}

		public string ContractScheduleDescription
		{
			get { return _contractScheduleDto != null ? _contractScheduleDto.Description : string.Empty; }
		}

		public string PersonName
		{
			get { return _personDto.Name; }
		}

		public string StartDate
		{
			get { return _personPeriodDto.StartDate.DateTime.ToString(); }
		}

		public string TeamDescription
		{
			get { return _personPeriodDto.Team != null ? _personPeriodDto.Team.Description : string.Empty; }
		}

		public string Note
		{
			get { return _personPeriodDto.Note; }
		}

		public string AcdLogOnOriginalIdList
		{
			get { return CreateLogOnOriginalIdList(); }
		}

		public string AcdLogOnNameList
		{
			get { return CreateLogOnNameList(); }
		}

		private string CreateLogOnOriginalIdList()
		{
			var stringBuilder = new StringBuilder();

			var i = 0;
			foreach (var externalLogOnDto in _personPeriodDto.ExternalLogOn)
			{
				i++;
				stringBuilder.Append(externalLogOnDto.AcdLogOnOriginalId);

				if(i <= _personPeriodDto.ExternalLogOn.GetUpperBound(0))
					stringBuilder.Append(", ");
			}

			return stringBuilder.ToString();
		}

		private string CreateLogOnNameList()
		{
			var stringBuilder = new StringBuilder();
			var i = 0;
			foreach (var externalLogOnDto in _personPeriodDto.ExternalLogOn)
			{
				i++;
				stringBuilder.Append(externalLogOnDto.AcdLogOnName);

				if(i <= _personPeriodDto.ExternalLogOn.GetUpperBound(0))
					stringBuilder.Append(", ");
			}
			return stringBuilder.ToString();
		}
	}
}
