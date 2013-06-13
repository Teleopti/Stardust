using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

        private IDictionary<ISkill, int> _skillIndexRegister;
        private IDictionary<string, int> _personIndexRegister;

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
            _skillIndexRegister = new Dictionary<ISkill, int>();
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
            _personIndexRegister = new Dictionary<string, int>();
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
            foreach (ISkill key in _dividedActivityData.TargetDemands.Keys)
            {
                _furnessData.ProductionDemand()[_skillIndexRegister[key]] = _dividedActivityData.TargetDemands[key];
            }
        }

        /// <summary>
        /// Converts the absolut person resources to furness data ProducerResources.
        /// </summary>
        private void ConvertAbsolutPersonResources()
        {
            foreach (var key in _dividedActivityData.PersonResources.Keys)
            {
                _furnessData.ProducerResources()[_personIndexRegister[key]] = _dividedActivityData.PersonResources[key];

            }
        }

        private void convertMatrixData()
        {
            foreach (var personKey in _dividedActivityData.PersonResources.Keys)
            {
                int producerIndex = _personIndexRegister[personKey];
                foreach (ISkill skillKey in _dividedActivityData.TargetDemands.Keys)
                {
                    int productIndex = _skillIndexRegister[skillKey];
                    double weightedSkillValue;
                    if (!_dividedActivityData.WeightedRelativeKeyedSkillResourceResources[personKey].TryGetValue(skillKey, out weightedSkillValue))
                        weightedSkillValue = 0;
                    _furnessData.ResourceMatrix()[producerIndex, productIndex] = weightedSkillValue;

                    double value = _dividedActivityData.KeyedSkillResourceEfficiencies[personKey].ContainsKey(skillKey) ? 1 : 0;
                    _furnessData.ProductivityMatrix()[producerIndex, productIndex] = value;
                }
            }
        }

        /// <summary>
        /// Converts the person skill resource matrix back from FurnessData.
        /// </summary>
        private void convertPersonSkillResourceMatrixBack()
        {
            foreach (var personKey in _personIndexRegister.Keys)
            {
                int producerIndex = _personIndexRegister[personKey];
                foreach (ISkill skillKey in _skillIndexRegister.Keys)
                {
                    int productIndex = _skillIndexRegister[skillKey];
                    if (_dividedActivityData.WeightedRelativeKeyedSkillResourceResources[personKey].ContainsKey(skillKey))
                        _dividedActivityData.WeightedRelativeKeyedSkillResourceResources[personKey][skillKey] = _furnessData.ResourceMatrix()[producerIndex, productIndex];
                }
            }
        }
    }
}