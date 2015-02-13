using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ISeniorityShiftCategoryRank
	{
		IShiftCategory ShiftCategory { get; set; }
		int Rank { get; set; }
		string Text { get; }
	}

	public class SeniorityShiftCategoryRank : ISeniorityShiftCategoryRank
	{
		private IShiftCategory _shiftCategory;
		private int _rank;


		public SeniorityShiftCategoryRank(IShiftCategory shiftCategory) : this()
		{
			_shiftCategory = shiftCategory;
			_rank = Int32.MaxValue;
		}

		protected SeniorityShiftCategoryRank()
		{

		}

		public IShiftCategory ShiftCategory
		{
			get { return _shiftCategory; }
			set { _shiftCategory = value; }
		}

		public int Rank
		{
			get { return _rank; }
			set { _rank = value; }
		}

		public string Text
		{
			get { return _shiftCategory.Description.Name; }
		}
	}
}
