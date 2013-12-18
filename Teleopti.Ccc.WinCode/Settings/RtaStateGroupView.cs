using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
	public class RtaStateGroupView : EntityContainer<IRtaStateGroup>
	{
		public string Name
		{
			get { return ContainedEntity.Name; }
			set { ContainedEntity.Name = value; }
		}

		public bool DefaultStateGroup
		{
			get { return ContainedEntity.DefaultStateGroup; }
			set { ContainedEntity.DefaultStateGroup = value; }
		}

		public bool Available
		{
			get { return ContainedEntity.Available; }
			set { ContainedEntity.Available = value; }
		}

		public ReadOnlyCollection<IRtaState> StateCollection
		{
			get { return ContainedEntity.StateCollection; }
		}

		public bool IsLogOutState
		{
			get { return ContainedEntity.IsLogOutState; }
			set { ContainedEntity.IsLogOutState = value; }
		}

		public bool InContractTime
		{
			get { return ContainedEntity.InContractTime; }
			set { ContainedEntity.InContractTime = value; }
		}

		public IPayload UnderlyingPayload
		{
			get { return ContainedEntity.UnderlyingPayload; }
		}

		public bool IsDeleted
		{
			get { return ContainedEntity.IsDeleted; }
		}

		public IBusinessUnit BusinessUnit
		{
			get { return ContainedEntity.BusinessUnit; }
		}

		public string CreatedBy
		{
			get { return ContainedEntity.CreatedBy != null ? ContainedEntity.CreatedBy.Name.ToString() : string.Empty; }
		}

		public DateTime? CreatedOn
		{
			get { return ContainedEntity.CreatedOn; }
		}

		public string UpdatedBy
		{
			get { return ContainedEntity.UpdatedBy != null ? ContainedEntity.UpdatedBy.Name.ToString() : string.Empty; }
		}

		public DateTime? UpdatedOn
		{
			get { return ContainedEntity.UpdatedOn; }
		}

		public RtaStateGroupView(IRtaStateGroup stateGroup)
		{
			setContainedEntity(stateGroup);
		}

		private void setContainedEntity(IRtaStateGroup stateGroup)
		{
			ContainedEntity = stateGroup.EntityClone();
			ContainedOriginalEntity = stateGroup;
		}

		public IRtaStateGroup ContainedOriginalEntity { get; private set; }

		public void UpdateAfterMerge(IRtaStateGroup updatedStateGroup)
		{
			setContainedEntity(updatedStateGroup);
		}

		public void ResetRtaStateGroupState(IRtaStateGroup stateGroup)
		{
			setContainedEntity(stateGroup ?? ContainedOriginalEntity);
		}
	}
}
