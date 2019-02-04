using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class FavoriteSearch : AggregateRoot_Events_ChangeInfo_BusinessUnit, IFavoriteSearch
	{
		private string _name;
		private string _teamIds;
		private string _searchTerm;
		private FavoriteSearchStatus _status = FavoriteSearchStatus.Normal;
		private IPerson _createdBy;
		private WfmArea _wfmArea;

		public FavoriteSearch(string name) : this()
		{
			_name = name;
		}

		public FavoriteSearch()
		{
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual string TeamIds
		{
			get { return _teamIds; }
			set { _teamIds = value; }
		}

		public virtual string SearchTerm
		{
			get { return _searchTerm; }
			set { _searchTerm = value; }
		}

		public virtual FavoriteSearchStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual IFavoriteSearch NoneEntityClone()
		{
			return (IFavoriteSearch)MemberwiseClone();
		}

		public virtual IFavoriteSearch EntityClone()
		{
			return (IFavoriteSearch)MemberwiseClone();
		}

		public virtual IPerson Creator
		{
			get { return _createdBy; }
			set { _createdBy = value; }
		}

		public virtual WfmArea WfmArea
		{
			get { return _wfmArea; }
			set { _wfmArea = value; }
		}
	}

	[Flags]
	public enum FavoriteSearchStatus
	{
		Normal = 0,
		Default = 1
	}

	[Flags]
	public enum WfmArea
	{		
		Teams = 1,
		Requests = 2
	}
}