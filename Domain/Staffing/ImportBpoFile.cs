﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ImportBpoFile
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		private const string lineSeparator = "\r\n";
	
		// valid field names
		private const string source = "source";
		private const string skillgroup = "skillgroup";
		private const string startdatetime = "startdatetime";
		private const string enddatetime = "enddatetime";
		private const string resources = "resources";

		private readonly IReadOnlyCollection<string> validFieldNames =
			new List<string> { source, skillgroup, startdatetime, enddatetime, resources };

		public ImportBpoFile(ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, IUserTimeZone userTimeZone, INow now)
		{
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_userTimeZone = userTimeZone;
			_now = now;
		}

		public ImportBpoFileResult ImportFile(string fileContents, IFormatProvider importFormatProvider,char tokenSeparator = ',', char skillSeparator = '|')
		{
			var result = new ImportBpoFileResult();

			if (fileContents.IsNullOrEmpty())
			{
				result.Success = false;
				result.ErrorInformation.Add(formatGeneralLineErrorMessage(new LineWithNumber{ LineContent = "", LineNumber = 1 }, Resources.TheImportFileCannotBeEmpty));
				return result;
			}

			var lines = fileContents.Split(new[] { lineSeparator }, StringSplitOptions.None);
			var lineNumber = 1;
			var linesWithNumbers = lines.Select(line => new LineWithNumber{LineContent = line, LineNumber = lineNumber++}).ToList();
			var headerWithFieldNames = linesWithNumbers[0].LineContent.Split(tokenSeparator);
			if (headerWithFieldNames.IsNullOrEmpty() || (headerWithFieldNames.Length==1 && headerWithFieldNames.FirstOrDefault().IsNullOrEmpty()))
			{
				result.Success = false;
				result.ErrorInformation.Add(formatGeneralLineErrorMessage(linesWithNumbers[0],
					Resources.FirstLineInFileHeaderLineCannotBeEmpty));
				return result;
			}
			assertFieldNames(headerWithFieldNames, result);

			if (!result.Success) return result;

			var allSkills = _skillRepository.LoadAllSkills().ToList();

			var bpoResourceList = new List<ImportSkillCombinationResourceBpo>();
			foreach (var line in linesWithNumbers.Skip(1))
			{
				if (line.LineContent.IsNullOrEmpty())
					continue;

				line.Tokens = tokenizeSkillCombinationResourceBpo(line, headerWithFieldNames, tokenSeparator, result);
				var resourceBpo = createSkillCombinationResourceBpo(line, importFormatProvider, skillSeparator, allSkills, result);

				if (resourceBpo == null)
					continue;
				
				var validateBpoRowResult = ValidateBpoRow(resourceBpo);
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
				else if(result.Success)
					bpoResourceList.Add(resourceBpo);
			}
			if(result.Success )
				_skillCombinationResourceRepository.PersistSkillCombinationResourceBpo(bpoResourceList);
			return result;
		}

		private ImportBpoFileResult ValidateBpoRow(ImportSkillCombinationResourceBpo resourceBpo)
		{
			var result = new ImportBpoFileResult {Success = true};
			
			if (resourceBpo.StartDateTime >= resourceBpo.EndDateTime)
			{
				result.Success = false;
				result.ErrorInformation.Add("StartDateTime for the period must be a time before EndDateTime");
			}
			
			if(resourceBpo.Resources < 0)
			{
				result.Success = false;
				result.ErrorInformation.Add("Resources of imported staffing cannot be less than 0");
			}
			
			//if(_now)
			
			return result;
		}

		private void assertFieldNames(string[] fieldNames, ImportBpoFileResult result)
		{
			foreach (var fieldName in fieldNames)
			{
				if (!validFieldNames.Contains(fieldName.Trim()))
				{
					result.Success = false;
					result.ErrorInformation.Add(string.Format(Resources.InvalidFieldNameInHeader,
						fieldName, string.Join(",", validFieldNames)));
				}
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
				DateTime sd, ed;
				double d;
				
				if (!DateTime.TryParse(bpoLineTokens[startdatetime], importFormatProvider, DateTimeStyles.None, out sd))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, Resources.ImportBpoWrongDateFormat));
				}
				if (!DateTime.TryParse(bpoLineTokens[enddatetime],importFormatProvider,DateTimeStyles.None, out ed))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, Resources.ImportBpoWrongDateFormat));
				}
				if (!double.TryParse(bpoLineTokens[resources],NumberStyles.Float,importFormatProvider,out d))
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(Resources.ImportBpoWrongResourceFormat,validFieldNames.IndexOf(resources) + 1, bpoLineTokens[resources])));
				}
				if (result.Success)
				{
					var startDateTime = TimeZoneHelper.ConvertToUtc(DateTime.Parse(bpoLineTokens[startdatetime], importFormatProvider),_userTimeZone.TimeZone());
					var endDateTime = TimeZoneHelper.ConvertToUtc(DateTime.Parse(bpoLineTokens[enddatetime], importFormatProvider),_userTimeZone.TimeZone());
					resourceBpo = new ImportSkillCombinationResourceBpo
					{
						Source = bpoLineTokens[source],
						StartDateTime = startDateTime,
						EndDateTime = endDateTime,
						Resources = double.Parse(bpoLineTokens[resources], importFormatProvider),
						SkillIds = lookupSkillIds(lineWithNumber, bpoLineTokens[skillgroup], skillSeparator, allSkills, result)
					};
				}
			}
			return resourceBpo;
		}

		private List<Guid> lookupSkillIds(LineWithNumber lineWithNumber, string skillGroupString, char skillSeparator, IList<ISkill> allSkills, ImportBpoFileResult result)
		{
			var skillIds = new List<Guid>();
			var skillStringList = skillGroupString.Split(skillSeparator);
			foreach (var skillString in skillStringList)
			{
				var skillStringTrimmed = skillString.Trim();
				var skills = allSkills.Where(s => s.Name == skillStringTrimmed).ToList();
				if (skills.IsEmpty())
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(Resources.TheSkillWithNameIsNotDefinedInTheSystem, skillStringTrimmed)));
				}
				if (skills.Count == 1)
				{
					skillIds.Add(skills.First().Id.GetValueOrDefault());
				}
				else if (skills.Count > 1)
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
						string.Format(
							Resources.SkillDefinedMultipleTimesInTheSystem,
							skillStringTrimmed, skills.Count)));
				}
			}
			return skillIds; // replace with real guid
		}

		private Dictionary<string, string> tokenizeSkillCombinationResourceBpo(LineWithNumber lineWithNumber, string[] fieldNames, char tokenSeparator, ImportBpoFileResult bpoResult)
		{
			var parameters = lineWithNumber.LineContent.Split(tokenSeparator);
			var result = new Dictionary<string, string>();
			if (parameters.Length != fieldNames.Length)
			{
			bpoResult.Success = false;
				bpoResult.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber,
					string.Format(Resources.BpoLineHasIncorrectParameterCount, parameters.Length, fieldNames.Length)));
			}
			else
			{
				for (int i = 0; i < parameters.Length; i++)
					result.Add(fieldNames[i].Trim(), parameters[i].Trim());
			}
			return result;
		}

		private string formatParameterEmptyErrorMessage(LineWithNumber lineWithNumber, string emptyParameterName)
		{
			return formatGeneralLineErrorMessage(lineWithNumber,
				string.Format(Resources.ParameterCannotBeEmpty, emptyParameterName));
		}

		private string formatGeneralLineErrorMessage(LineWithNumber lineWithNumber, string specificMessage)
		{
			var lineMessage = string.Format(Resources.ErrorFoundOnLineNumberXWithContentsY, lineWithNumber.LineNumber,
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
		public bool Equals(ImportSkillCombinationResourceBpo other)
		{
			//not considering resource here 
			if (other == null) return false;
			return other.StartDateTime == StartDateTime && other.Source == Source && EndDateTime == other.EndDateTime &&
				   SkillIds.OrderBy(s=>s).SequenceEqual(other.SkillIds.OrderBy(s => s));
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
