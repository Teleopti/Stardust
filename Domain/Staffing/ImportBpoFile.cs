using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ImportBpoFile
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IUserNow _userNow;
		private readonly ILoggedOnUser _loggedOnUser;
		private char _tokenSeparator = ',';
		
		private const string lineSeparator = "\r\n";
		private const int maxErrors = 20;
	
		// valid field names
		private const string source = "source";
		private const string skillgroup = "skillcombination";
		private const string startdatetime = "startdatetime";
		private const string enddatetime = "enddatetime";
		private const string resources = "agents";

		private readonly string[] validFieldNames =
			new [] { source, skillgroup, startdatetime, enddatetime, resources };

		private List<ISkill> _allSkills;

		public ImportBpoFile(ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, IUserTimeZone userTimeZone, IUserNow userNow, ILoggedOnUser loggedOnUser)
		{
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_userTimeZone = userTimeZone;
			_userNow = userNow;
			_loggedOnUser = loggedOnUser;
		}

		public ImportBpoFileResult ImportFile(string fileContents, IFormatProvider importFormatProvider, string fileName,
			char skillSeparator = '|')
		{
			var result = new ImportBpoFileResult();

			if (fileContents.IsNullOrEmpty())
			{
				result.Success = false;
				result.ErrorInformation.Add(
					formatGeneralLineErrorMessage(new LineWithNumber {LineContent = "", LineNumber = 1},
						Resources.ImportBpoTheImportFileCannotBeEmpty));
				return result;
			}

			var lines = fileContents.Split(new[] {lineSeparator}, StringSplitOptions.None);
			lines = removeDoubleQuotesIfNecessary(lines);
			var lineNumber = 1;

			var linesWithNumbers = lines.Select(line => new LineWithNumber {LineContent = line, LineNumber = lineNumber++})
				.ToList();

			presetTokenSeparator(linesWithNumbers[0].LineContent);

			var headerWithFieldNames = linesWithNumbers[0].LineContent.ToLower().Split(_tokenSeparator);
			if (headerWithFieldNames.IsNullOrEmpty() ||
				(headerWithFieldNames.Length == 1 && headerWithFieldNames.FirstOrDefault().IsNullOrEmpty()))
			{
				result.Success = false;
				result.ErrorInformation.Add(formatGeneralLineErrorMessage(linesWithNumbers[0],
					Resources.ImportBpoFirstLineInFileHeaderLineCannotBeEmpty));
				return result;
			}

			assertFieldNames(headerWithFieldNames, result);

			if (!result.Success) return result;

			_allSkills = _skillRepository.LoadAllSkills().ToList();

			var bpoResourceList = new List<ImportSkillCombinationResourceBpo>();
			foreach (var line in linesWithNumbers.Skip(1))
			{
				if (line.LineContent.IsNullOrEmpty())
					continue;

				line.Tokens = tokenizeSkillCombinationResourceBpo(line, headerWithFieldNames, _tokenSeparator, result);
				var resourceBpo = createSkillCombinationResourceBpo(line, importFormatProvider, skillSeparator, _allSkills, result);

				if (resourceBpo == null)
					continue;

				var validateBpoRowResult = validateBpoRow(resourceBpo);
				if (!validateBpoRowResult.Success)
				{
					result.Success = false;
					foreach (var errorInfo in validateBpoRowResult.ErrorInformation)
					{
						result.ErrorInformation.Add(formatGeneralLineErrorMessage(line,
							errorInfo));
					}

					continue;
				}

				if (bpoResourceList.Contains(resourceBpo))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(line, Resources.ImportBpoDuplicateRecord));
				}
				else if (result.Success)
				{
					resourceBpo.ImportFileName = fileName;
					resourceBpo.PersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault();
					bpoResourceList.Add(resourceBpo);
				}
			}

			if (result.Success)
				_skillCombinationResourceRepository.PersistSkillCombinationResourceBpo(bpoResourceList);


			if (result.ErrorInformation.Count > maxErrors)
			{
				var originalErrorCount = result.ErrorInformation.Count;
				var temp = result.ErrorInformation.Take(maxErrors).ToList();
				result.ErrorInformation.Clear();
				temp.ForEach(e => result.ErrorInformation.Add(e));

				result.ErrorInformation.Add(string.Format(Resources.ImportBpoShowingTheFirstXErrorMessagesOfY, maxErrors,
					originalErrorCount));
			}

			return result;
		}

		private string[] removeDoubleQuotesIfNecessary(string[] lines)
		{
			var temp = new List<string>();
			foreach (var line in lines)
			{
				var current = line;
				if (current.StartsWith("\""))
					current = current.Remove(0, 1);
				if(current.EndsWith("\""))
					current = current.Remove(current.Length - 1, 1);
				temp.Add(current);
			}
			return temp.ToArray();
		}

		private void presetTokenSeparator(string templateRow)
        {
			_tokenSeparator = templateRow.Contains(";") ? ';' : ',';
        }

		private ImportBpoFileResult validateBpoRow(ImportSkillCombinationResourceBpo resourceBpo)
		{
			var result = new ImportBpoFileResult {Success = true};
			
			if (resourceBpo.StartDateTime >= resourceBpo.EndDateTime)
			{
				result.Success = false;
				result.ErrorInformation.Add(Resources.ImportBpoStartDateTimeMustBeBeforeEndDateTime);
			}
			
			if(resourceBpo.Resources < 0)
			{
				result.Success = false;
				result.ErrorInformation.Add(Resources.ImportBpoResourcesCannotBeLessThanZero);
			}

			if (resourceBpo.StartDateTime < TimeZoneHelper.ConvertToUtc(_userNow.DateTime().Date, _userTimeZone.TimeZone()))
			{
				result.Success = false;
				result.ErrorInformation.Add(Resources.IntervalStartMustBeTodaysDateOrLater);
			}

			var resourceBpoIntervalLength = (int)resourceBpo.EndDateTime.Subtract(resourceBpo.StartDateTime).TotalMinutes;

			var resolutionsResult = validateAllDefaultResolutions(resourceBpo.SkillIds, resourceBpoIntervalLength);
			if (resolutionsResult.Success) return result;
			
			result.Success = false;
			foreach (var errorMsg in resolutionsResult.ErrorInformation)
				result.ErrorInformation.Add(errorMsg);

			return result;
		}

		private ImportBpoFileResult validateAllDefaultResolutions(IEnumerable<Guid> skillIds, int resourceBpoIntervalLength)
		{
			var result = new ImportBpoFileResult();
			foreach (var skillId in skillIds)
			{
				var skill = _allSkills.SingleOrDefault(s => s.Id == skillId);
				if (skill == null)
				{
					result.Success = false;
					result.ErrorInformation.Add(string.Format(Resources.ImportBpoSkillNotFound, skillId));
					continue;
				}
				var skillResolution = skill.DefaultResolution;
				if (skillResolution == 0)
				{
					result.Success = false;
					result.ErrorInformation.Add(string.Format(Resources.ImportBpoNoDefaultResolutionForSkill, skill.Name));
				}

				if (skillResolution == resourceBpoIntervalLength) continue;
				
				result.Success = false;
				result.ErrorInformation.Add(string.Format(Resources.ImportBpoResourceIntervalLengthDiffersFromTheSkill, 
					resourceBpoIntervalLength, skill.Name, skillResolution));
			}

			return result;
		}

		private void assertFieldNames(string[] fieldNames, ImportBpoFileResult result)
		{
			foreach (var fieldName in fieldNames)
			{
				if (validFieldNames.Contains(fieldName.Trim())) continue;
				
				result.Success = false;
				result.ErrorInformation.Add(string.Format(Resources.ImportBpoInvalidFieldNameInHeader,
					fieldName, string.Join(",", validFieldNames)));
			}
		}

		private ImportSkillCombinationResourceBpo createSkillCombinationResourceBpo(LineWithNumber lineWithNumber, IFormatProvider importFormatProvider, char skillSeparator, IList<ISkill> allSkills, ImportBpoFileResult result)
		{
			ImportSkillCombinationResourceBpo resourceBpo = null;
			var bpoLineTokens = lineWithNumber.Tokens;

			bpoLineTokens.Where(token => token.Value.IsNullOrEmpty()).
				ForEach(token => result.ErrorInformation.Add(formatParameterEmptyErrorMessage(lineWithNumber, token.Key)));

			if ((bpoLineTokens.Count==0 || bpoLineTokens.Any(token => token.Value.IsNullOrEmpty())) && result.ErrorInformation.Any())
			{
				result.Success = false;
			}
			else
			{
				if (bpoLineTokens[source].Length > 100)
				{
					result.Success = false;
					result.ErrorInformation.Add(
						formatGeneralLineErrorMessage(lineWithNumber, 
							Resources.ImportBpoSourceNameTooLong));
				}
				
				if (!DateTime.TryParseExact(bpoLineTokens[startdatetime], "yyyyMMdd H:mm", importFormatProvider, DateTimeStyles.None, out var startDateTime) &&
					!DateTime.TryParse(bpoLineTokens[startdatetime], importFormatProvider, DateTimeStyles.None, out startDateTime))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, Resources.ImportBpoWrongDateFormat));
				}
				if (!DateTime.TryParseExact(bpoLineTokens[enddatetime], "yyyyMMdd H:mm", importFormatProvider, DateTimeStyles.None, out var endDateTime) &&
					!DateTime.TryParse(bpoLineTokens[enddatetime],importFormatProvider,DateTimeStyles.None, out endDateTime))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, Resources.ImportBpoWrongDateFormat));
				}
				
				var parsedDouble = parseDouble(bpoLineTokens[resources]);
				
				if(parsedDouble.Result == false)
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(Resources.ImportBpoWrongResourceFormat,validFieldNames.IndexOf(resources) + 1, bpoLineTokens[resources])));
				}
			
				var startDateTimeUtc = TimeZoneHelper.ConvertToUtc(startDateTime, _userTimeZone.TimeZone());
				var endDateTimeUtc = TimeZoneHelper.ConvertToUtc(endDateTime, _userTimeZone.TimeZone());
				resourceBpo = new ImportSkillCombinationResourceBpo
				{
					Source = bpoLineTokens[source],
					StartDateTime = startDateTimeUtc,
					EndDateTime = endDateTimeUtc,
					Resources = parsedDouble.Value,
					SkillIds = lookupSkillIds(lineWithNumber, bpoLineTokens[skillgroup], skillSeparator, allSkills, result)
				};
			}
			return resourceBpo;
		}

		private class validatedDouble
		{
			public double Value;
			public bool Result;
		}
		
		private static validatedDouble parseDouble(string text)
		{
			var validatedDouble = new validatedDouble();
			var cultureInfo = new CultureInfo("en-US");
			
			var numberFormat = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
			const NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite |
											  NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign;
			validatedDouble.Result = double.TryParse(text, numberStyles, numberFormat, out validatedDouble.Value);
			if (validatedDouble.Result)
				return validatedDouble;
			
			numberFormat.NumberDecimalSeparator = ",";
			validatedDouble.Result = double.TryParse(text, numberStyles, numberFormat, out validatedDouble.Value);
			if (validatedDouble.Result == false)
				validatedDouble.Value = 0;

			return validatedDouble;
		}

		private List<Guid> lookupSkillIds(LineWithNumber lineWithNumber, string skillGroupString, char skillSeparator, IEnumerable<ISkill> allSkills, ImportBpoFileResult result)
		{
			var skillIds = new List<Guid>();
			var skillStringList = skillGroupString.Split(skillSeparator);
			var skillLookup = allSkills.ToLookup(s => s.Name.ToLower());
			foreach (var skillString in skillStringList)
			{
				var skillStringTrimmed = skillString.Trim().ToLower();
				var skills = skillLookup[skillStringTrimmed].ToArray();
				if (skills.IsEmpty())
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(Resources.ImportBpoTheSkillWithNameIsNotDefinedInTheSystem, skillStringTrimmed)));
				}
				if (skills.Length == 1)
				{
					var skill = skills.SingleOrDefault();
					if (skillIds.Contains(skill.Id.GetValueOrDefault()))
					{
						result.Success = false;
						result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
							string.Format(Resources.ImportBpoSkillListedMoreThanOnce, skill.Name)));
						continue;
					}
					skillIds.Add(skills.First().Id.GetValueOrDefault());
				}
				else if (skills.Length > 1)
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(
							Resources.ImportBpoSkillDefinedMultipleTimesInTheSystem,
							skillStringTrimmed, skills.Length)));
				}
			}
			return skillIds; // replace with real guid
		}

		private Dictionary<string, string> tokenizeSkillCombinationResourceBpo(LineWithNumber lineWithNumber, IReadOnlyList<string> fieldNames, char tokenSeparator, ImportBpoFileResult bpoResult)
		{
			var parameters = lineWithNumber.LineContent.Split(tokenSeparator);
			var result = new Dictionary<string, string>();
			if (parameters.Length != fieldNames.Count)
			{
			bpoResult.Success = false;
				bpoResult.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
					string.Format(Resources.ImportBpoLineHasIncorrectParameterCount, parameters.Length, fieldNames.Count)));
			}
			else
			{
				for (var i = 0; i < parameters.Length; i++)
					result.Add(fieldNames[i].Trim(), parameters[i].Trim());
			}
			return result;
		}

		private string formatParameterEmptyErrorMessage(LineWithNumber lineWithNumber, string emptyParameterName)
		{
			return formatGeneralLineErrorMessage(lineWithNumber,
				string.Format(Resources.ImportBpoParameterCannotBeEmpty, emptyParameterName));
		}

		private string formatGeneralLineErrorMessage(LineWithNumber lineWithNumber, string specificMessage)
		{
			var lineMessage = string.Format(Resources.ImportBpoErrorFoundOnLineNumberXWithContentsY, lineWithNumber.LineNumber,
				lineWithNumber.LineContent, specificMessage);
			return lineMessage;
		}
	}

	public class ImportSkillCombinationResourceBpo : IEquatable<ImportSkillCombinationResourceBpo>
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resources { get; set; }
		public List<Guid> SkillIds { get; set; }
		public string Source { get; set; }
		public string ImportFileName { get; set; }
		public Guid PersonId { get; set; }

		public bool Equals(ImportSkillCombinationResourceBpo other)
		{
			//not considering resource here 
			if (other == null) return false;
			return other.StartDateTime == StartDateTime &&
				   string.Equals(other.Source, Source, StringComparison.OrdinalIgnoreCase) && EndDateTime == other.EndDateTime &&
				   SkillIds.OrderBy(s => s).SequenceEqual(other.SkillIds.OrderBy(s => s));
		}
	}

	public class SkillCombinationResourceBpo
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resources { get; set; }
		public Guid SkillCombinationId { get; set; }
		public string Source { get; set; }
	}

	public class LineWithNumber
	{
		public string LineContent;
		public int LineNumber;
		public Dictionary<string, string> Tokens = new Dictionary<string, string>();
	}

	public class ImportBpoFileResult
	{
		public bool Success = true;
		public readonly HashSet<string> ErrorInformation = new HashSet<string>();
	}
}

