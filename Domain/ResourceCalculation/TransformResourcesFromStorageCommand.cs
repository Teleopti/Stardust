namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class TransformResourcesFromStorageCommand
	{
		private readonly ResourcesFromStorage _resourcesFromStorage;

		public TransformResourcesFromStorageCommand(ResourcesFromStorage resourcesFromStorage)
		{
			_resourcesFromStorage = resourcesFromStorage;
		}

		public void Execute()
		{
			_resourcesFromStorage.Clear();
			_resourcesFromStorage.ExtractActivityRequiresSeat();
			_resourcesFromStorage.ExtractSkillCombinations();
			_resourcesFromStorage.ExtractResources();
		}
	}
}