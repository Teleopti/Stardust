using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    /// <summary>
    /// Converts the divided activity data into FurnessData, and - provided that you keep a 
    /// reference on the created FurnessData throughout the further optimalization process
    /// - converts the FurnessData back to divided activity data.
    /// </summary>
    public class FurnessDataConverter
    {
        private IFurnessData _furnessData;
        private readonly IDividedActivityData _dividedActivityData;

        private readonly IDictionary<ISkill, int> _skillIndexRegister = new Dictionary<ISkill, int>();
        private readonly IDictionary<string, int> _personIndexRegister = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FurnessDataConverter"/> class.
        /// </summary>
        /// <param name="dividedActivityData">The divided activity data.</param>
        public FurnessDataConverter(
            IDividedActivityData dividedActivityData)
        {
            _dividedActivityData = dividedActivityData;
        }

        /// <summary>
        /// Converts the inner resource structure to <see cref="FurnessData"/>.
		/// </summary>
		[CLSCompliant(false)]
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
        public IDividedActivityData ConvertFurnessDataBackToActivity()
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
            _skillIndexRegister.Clear();
            int currentIndex = 0;
            foreach (ISkill key in _dividedActivityData.TargetDemands.Keys)
            {
                _skillIndexRegister.Add(key, currentIndex);
                currentIndex++;
            }
        }

        /// <summary>
        /// Creates a dictionary that keeps track on person index in the FurnessData arrays.
        /// </summary>
        private void CreatePersonIndexRegister()
        {
            _personIndexRegister.Clear();
            int currentIndex = 0;
            foreach (var key in _dividedActivityData.PersonResources.Keys)
            {
                _personIndexRegister.Add(key, currentIndex);
                currentIndex++;
            }
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
                foreach (ISkill skillKey in _dividedActivityData.TargetDemands.Keys)
                {
                    int productIndex = _skillIndexRegister[skillKey];
                    double weightedSkillValue;
                    if (!_dividedActivityData.WeightedRelativeKeyedSkillResourceResources[personKey].TryGetValue(skillKey, out weightedSkillValue))
                        weightedSkillValue = 0;
	                resourceMatrix[producerIndex, productIndex] = weightedSkillValue;

                    double value = _dividedActivityData.KeyedSkillResourceEfficiencies[personKey].ContainsKey(skillKey) ? 1 : 0;
	                productivityMatrix[producerIndex, productIndex] = value;
                }
            }
        }

        /// <summary>
        /// Converts the person skill resource matrix back from FurnessData.
        /// </summary>
        private void convertPersonSkillResourceMatrixBack()
		{
			var resourceMatrix = _furnessData.ResourceMatrix();
            foreach (var personKey in _personIndexRegister.Keys)
            {
                int producerIndex = _personIndexRegister[personKey];
	            Dictionary<ISkill, double> skillValues;
	            if (!_dividedActivityData.WeightedRelativeKeyedSkillResourceResources.TryGetValue(personKey, out skillValues)) continue;
                foreach (var skillKeyPair in _skillIndexRegister)
                {
                    if (skillValues.ContainsKey(skillKeyPair.Key))
					{
						int productIndex = skillKeyPair.Value;
	                    skillValues[skillKeyPair.Key] = resourceMatrix[producerIndex, productIndex];
                    }
                }
            }
        }
    }
}