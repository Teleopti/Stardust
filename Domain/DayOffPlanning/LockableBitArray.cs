using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class LockableBitArray : ILockableBitArray
    {
        private readonly bool _includesWeekBefore;
        private readonly bool _includesWeekAfter;
        private readonly int? _terminalDateIndex;
        private BitArray _lockedBits;
        private BitArray _dayOffBits;
        private IList<int> _unlockedIndexes;
        private Random _rnd = new Random();
        private MinMax<int> _periodArea;

        public LockableBitArray(int bitArrayCount, bool includesWeekBefore, bool includesWeekAfter, int? terminalDateIndex)
        {
            _includesWeekBefore = includesWeekBefore;
            _includesWeekAfter = includesWeekAfter;
            _terminalDateIndex = terminalDateIndex;
            _lockedBits = new BitArray(bitArrayCount);
            _dayOffBits = new BitArray(bitArrayCount);
            if(bitArrayCount > int.MinValue)
                PeriodArea = new MinMax<int>(0, bitArrayCount - 1);
        }

        public BitArray DaysOffBitArray => _dayOffBits;

	    public int Count => _dayOffBits.Count;

	    public MinMax<int> PeriodArea
        {
            get { return _periodArea; }
            set { _periodArea = value; }
        }

        public IList<int> UnlockedIndexes
        {
            get
            {
                if(_unlockedIndexes == null)
                    createUnlockedList();

                return _unlockedIndexes;
            }
        }

        public int? TerminalDateIndex => _terminalDateIndex;

	    public void Set(int index, bool value)
        {
            if(_lockedBits[index])
                throw new ArgumentException("Locked index can not be modifyed");
            _dayOffBits.Set(index, value);
        }

        public void SetAll(bool value)
        {
            for (int i = 0; i < _dayOffBits.Count; i++)
            {
                _dayOffBits.Set(i, value);
            }
        }

	    public decimal PeriodAreaMaximum => _periodArea.Maximum;

	    public bool this[int index] => _dayOffBits[index];

	    public void Lock(int index, bool value)
        {
            _lockedBits.Set(index, value);
            _unlockedIndexes = null;
        }

        public int FindRandomUnlockedIndex()
        {
            if (UnlockedIndexes.Count == 0)
                return -1;

            int listIndex = _rnd.Next(0, UnlockedIndexes.Count - 1);
            return UnlockedIndexes[listIndex];
        }

		public decimal PeriodAreaMinimum => _periodArea.Minimum;

	    private void createUnlockedList()
        {
            _unlockedIndexes = new List<int>();
            for (int i = 0; i < _lockedBits.Count; i++)
            {
                if(!_lockedBits[i])
                    UnlockedIndexes.Add(i);
            }
        }

        public bool Get(int index)
        {
            return _dayOffBits.Get(index);
        }

        public bool IsLocked(int index, bool considerLock)
        {
            if(considerLock)
                return _lockedBits[index];

            return false;
        }

        public BitArray ToLongBitArray()
        {
            int length = _dayOffBits.Count;
            int offset = 0;
            if (!_includesWeekBefore)
            {
                length += 7;
                offset = 7;
            }
                
            if (!_includesWeekAfter)
                length += 7;
            BitArray longArray = new BitArray(length);
            for (int i = 0; i < _dayOffBits.Count; i++)
            {
                longArray.Set(i + offset, _dayOffBits[i]);
            }

            return longArray;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(_dayOffBits.Count);
            foreach (bool b in _dayOffBits)
            {
                if (b)
                    stringBuilder.Append(1);
                else
                {
                    stringBuilder.Append(0);
                }

            }
            return base.ToString() + " " + stringBuilder;
        }

        public object Clone()
        {
            LockableBitArray ret = (LockableBitArray)MemberwiseClone();
            ret._dayOffBits = (BitArray)_dayOffBits.Clone();
            ret._lockedBits = (BitArray)_lockedBits.Clone();
            ret._unlockedIndexes = null;
            ret._rnd = new Random();
            return ret;
        }

		public bool HasSameDayOffs(ILockableBitArray otherLockableBitArray)
		{
			if (_dayOffBits.Count != otherLockableBitArray.Count)
				return false;

			for (var i = 0; i < _dayOffBits.Count; i++)
			{
				if (this[i] && !otherLockableBitArray[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}