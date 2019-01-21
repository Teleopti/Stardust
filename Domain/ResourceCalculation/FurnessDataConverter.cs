using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Converts the divided activity data into FurnessData, and - provided that you keep a 
    /// reference on the created FurnessData throughout the further optimalization process
    /// - converts the FurnessData back to divided activity data.
    /// </summary>
    public class FurnessDataConverter
    {
        private IFurnessData _furnessData;
        private readonly DividedActivityData _dividedActivityData;

        private IDictionary<ISkill, int> _skillIndexRegister = new Dictionary<ISkill, int>();
        private IDictionary<DoubleGuidCombinationKey, int> _personIndexRegister = new Dictionary<DoubleGuidCombinationKey, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FurnessDataConverter"/> class.
        /// </summary>
        /// <param name="dividedActivityData">The divided activity data.</param>
        public FurnessDataConverter(
            DividedActivityData dividedActivityData)
        {
            _dividedActivityData = dividedActivityData;
        }

        /// <summary>
        /// Converts the inner resource structure to <see cref="FurnessData"/>.
		/// </summary>
        public IFurnessData ConvertDividedActivityToFurnessData()
        {
            _dividedActivityData.CalculatePersonResourcesSummaForFurnessInitialization();
            _furnessData = new FurnessData(_dividedActivityData.PersonResources.Count, _dividedActivityData.TargetDemands.Count);
            CreateSkillIndexRegister();
            CreatePersonIndexRegister();
            ConvertTargetDemands();
            ConvertAbsolutPersonResources();
            convertMatrixData();
            return _furnessData;
        }

        /// <summary>
        /// Converts the furness data result back to the divided activity classes.
        /// </summary>
        /// <returns></returns>
        public DividedActivityData ConvertFurnessDataBackToActivity()
        {
            convertPersonSkillResourceMatrixBack();
            _dividedActivityData.CalculatePersonResourcesSummaForReadingResultFromFurness();
            return _dividedActivityData;
        }

        /// <summary>
        /// Creates a dictionary that keeps track on skill index in the FurnessData arrays.
        /// </summary>
        private void CreateSkillIndexRegister()
		{
			_skillIndexRegister = _dividedActivityData.TargetDemands.Keys.Select((k, i) => new {k, i})
				.ToDictionary(k => k.k, v => v.i);
        }

        /// <summary>
        /// Creates a dictionary that keeps track on person index in the FurnessData arrays.
        /// </summary>
        private void CreatePersonIndexRegister()
        {
            _personIndexRegister = _dividedActivityData.PersonResources.Keys.Select((k, i) => new { k, i })
				.ToDictionary(k => k.k, v => v.i);
		}

        /// <summary>
        /// Converts the target demands to furness data ProductionDemand.
        /// </summary>
        private void ConvertTargetDemands()
		{
			var productionDemand = _furnessData.ProductionDemand();
            foreach (var keyPair in _dividedActivityData.TargetDemands)
            {
	            productionDemand[_skillIndexRegister[keyPair.Key]] = keyPair.Value;
            }
        }

        /// <summary>
        /// Converts the absolut person resources to furness data ProducerResources.
        /// </summary>
        private void ConvertAbsolutPersonResources()
		{
			var producerResources = _furnessData.ProducerResources();
            foreach (var keyPair in _dividedActivityData.PersonResources)
            {
	            producerResources[_personIndexRegister[keyPair.Key]] = keyPair.Value;
            }
        }

        private void convertMatrixData()
		{
			var productivityMatrix = _furnessData.ProductivityMatrix();
			var resourceMatrix = _furnessData.ResourceMatrix();
            foreach (var personKey in _dividedActivityData.PersonResources.Keys)
            {
				int producerIndex = _personIndexRegister[personKey];
				var weightedRelativeKeyedSkillResourceResource = _dividedActivityData.WeightedRelativeKeyedSkillResourceResources[personKey];
				var keyedSkillResourceEfficiency = _dividedActivityData.KeyedSkillResourceEfficiencies[personKey];
                foreach (ISkill skillKey in _dividedActivityData.TargetDemands.Keys)
                {
                    int productIndex = _skillIndexRegister[skillKey];
					if (!weightedRelativeKeyedSkillResourceResource.TryGetValue(skillKey, out var weightedSkillValue))
                        weightedSkillValue = 0;
	                resourceMatrix[producerIndex][productIndex] = weightedSkillValue;

	                double value = keyedSkillResourceEfficiency.ContainsKey(skillKey) ? 1 : 0;
	                productivityMatrix[producerIndex][productIndex] = value;
                }
            }
        }

        /// <summary>
        /// Converts the person skill resource matrix back from FurnessData.
        /// </summary>
        private void convertPersonSkillResourceMatrixBack()
		{
			var resourceMatrix = _furnessData.ResourceMatrix();
            foreach (var personKey in _personIndexRegister)
            {
                int producerIndex = personKey.Value;
				if (!_dividedActivityData.WeightedRelativeKeyedSkillResourceResources.TryGetValue(personKey.Key, out var skillValues)) continue;
                foreach (var skillKeyPair in _skillIndexRegister)
                {
                    if (skillValues.ContainsKey(skillKeyPair.Key))
					{
						int productIndex = skillKeyPair.Value;
	                    skillValues[skillKeyPair.Key] = resourceMatrix[producerIndex][productIndex];
                    }
                }
            }
        }
    }
}