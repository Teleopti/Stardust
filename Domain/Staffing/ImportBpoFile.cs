using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Staffing
{
	//public class ImportSkillCombinationResourceBpo
	//{
	//	public DateTime StartDateTime { get; set; }
	//	public DateTime EndDateTime { get; set; }
	//	public double Resources { get; set; }
	//	public List<Guid> SkillIds { get; set; }
	//	public string Source { get; set; }
	//}

	//public class SkillCombinationResourceBpo
	//{
	//	public DateTime StartDateTime { get; set; }
	//	public DateTime EndDateTime { get; set; }
	//	public double Resources { get; set; }
	//	public Guid SkillCombinationId { get; set; }
	//	public string Source { get; set; }
	//}

	//public class LineWithNumber
	//{
	//	public string LineContent;
	//	public int LineNumber;
	//	public Dictionary<string, string> Tokens = new Dictionary<string, string>();
	//}

	//public class ImportBpoFileResult
	//{
	//	public bool Success = true;
	//	public HashSet<string> ErrorInformation = new HashSet<string>();
	//	public string SuccessInformation;
	//}

	public class ImportBpoFile
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		private const string lineSeparator = "\r\n";
	
		// valid field names
		private const string source = "source";
		private const string skillgroup = "skillgroup";
		private const string startdatetime = "startdatetime";
		private const string enddatetime = "enddatetime";
		private const string resources = "resources";

		private readonly IReadOnlyCollection<string> validFieldNames =
			new List<string> { source, skillgroup, startdatetime, enddatetime, resources };

		public ImportBpoFile(ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public ImportBpoFileResult ImportFile(string fileContents, IFormatProvider importFormatProvider,char tokenSeparator = ',', char skillSeparator = '|')
		{
			var result = new ImportBpoFileResult();

			if (fileContents.IsNullOrEmpty())
			{
				result.Success = false;
				result.ErrorInformation.Add(formatGeneralLineErrorMessage(new LineWithNumber{ LineContent = "", LineNumber = 1 }, "The import file cannot be empty."));
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
					"First line in file (header line) cannot be empty."));
				return result;
			}
			assertFieldNames(headerWithFieldNames, result);

			if (!result.Success) return result;

			var allSkills = _skillRepository.LoadAllSkills().ToList();

			var bpoResourceList = new List<ImportSkillCombinationResourceBpo>();
			foreach (var line in linesWithNumbers.Skip(1))
			{
				line.Tokens = tokenizeSkillCombinationResourceBpo(line, headerWithFieldNames, tokenSeparator, result);
				if(line.Tokens.IsEmpty()) continue;
				var resourceBpo = createSkillCombinationResourceBpo(line, importFormatProvider, skillSeparator, allSkills, result);
				bpoResourceList.Add(resourceBpo);
			}
			if(result.Success )
				_skillCombinationResourceRepository.PersistSkillCombinationResourceBpo(bpoResourceList);
			return result;
		}

		private void assertFieldNames(string[] fieldNames, ImportBpoFileResult result)
		{
			foreach (var fieldName in fieldNames)
			{
				if (!validFieldNames.Contains(fieldName.Trim()))
				{
					result.Success = false;
					result.ErrorInformation.Add($"Invalid field name in header: '{fieldName}'. Valid field names are: ({string.Join(",", validFieldNames)})");
				}
			}
		}

		private ImportSkillCombinationResourceBpo createSkillCombinationResourceBpo(LineWithNumber lineWithNumber, IFormatProvider importFormatProvider, char skillSeparator, IEnumerable<ISkill> allSkills, ImportBpoFileResult result)
		{
			var bpoLineTokens = lineWithNumber.Tokens;

			bpoLineTokens.Where(token => token.Value.IsNullOrEmpty()).
				ForEach(token => result.ErrorInformation.Add(formatParameterEmptyErrorMessage(lineWithNumber, token.Key)));

			if (result.ErrorInformation.Any())
			{
				result.Success = false;
				return null;
			}

			var resourceBpo = new ImportSkillCombinationResourceBpo
			{
				Source = bpoLineTokens[source],
				StartDateTime = DateTime.Parse(bpoLineTokens[startdatetime], importFormatProvider),
				EndDateTime = DateTime.Parse(bpoLineTokens[enddatetime], importFormatProvider),
				Resources = double.Parse(bpoLineTokens[resources], importFormatProvider),
				SkillIds = lookupSkillIds(lineWithNumber, bpoLineTokens[skillgroup], skillSeparator, allSkills, result)
			};
			return resourceBpo;
		}

		private List<Guid> lookupSkillIds(LineWithNumber lineWithNumber, string skillGroupString, char skillSeparator, IEnumerable<ISkill> allSkills, ImportBpoFileResult result)
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
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, $"The skill with name {skillStringTrimmed} is not defined in the system."));
				}
				if (skills.Count == 1)
				{
					skillIds.Add(skills.First().Id.GetValueOrDefault());
				}
				else if (skills.Count > 1)
				{
					result.Success = false;
					result.ErrorInformation.Add(formatGeneralLineErrorMessage(lineWithNumber, $"The skill with name {skillStringTrimmed} is defined {skills.Count} times in the system. Only once is allowed when using this function."));
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
					$"Line has {parameters.Length} parameters but expected {fieldNames.Length} parameters."));

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
			return formatGeneralLineErrorMessage(lineWithNumber, $"Parameter {emptyParameterName} cannot be empty");
		}

		private string formatGeneralLineErrorMessage(LineWithNumber lineWithNumber, string specificMessage)
		{
			var lineMessage = $"Error found on line number {lineWithNumber.LineNumber} with contents:{lineWithNumber.LineContent} {specificMessage}";
			return lineMessage;
		}
	}

	public class ImportSkillCombinationResourceBpo
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resources { get; set; }
		public List<Guid> SkillIds { get; set; }
		public string Source { get; set; }
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
		public HashSet<string> ErrorInformation = new HashSet<string>();
	}
}
