using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class ShiftCategoryLimitationCombination
    {
        private PropertyState _weekly;
        private int? _maxNumberOf;
        private bool _maxNumberInitialized;
        private IShiftCategory _shiftCategory;
        private PropertyState _limit;
        private PropertyState _period;
        //private bool _initialized;

        private enum PropertyState
        {
            UnKnown,
            False,
            True,
            UnDeterment
        }

        private ShiftCategoryLimitationCombination(){}

        public ShiftCategoryLimitationCombination(IShiftCategory shiftCategory) : this()
        {
            _maxNumberOf = 0;
            _limit = PropertyState.UnKnown;
            _weekly = PropertyState.UnKnown;
            _period = PropertyState.UnKnown;
            _shiftCategory = shiftCategory;
        }

        public bool? Weekly
        {
            get { return propertyStateToNullableBool(_weekly); }
        }

        public int? MaxNumberOf
        {
            get
            {
                if (!_maxNumberInitialized)
                    return -1;

                return _maxNumberOf;
            }
        }

        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
        }

        public bool? Limit
        {
            get { return propertyStateToNullableBool(_limit); }
        }

        public bool? Period
        {
            get { return propertyStateToNullableBool(_period); }
        }

        public void CombineLimitations(IShiftCategoryLimitation limitation)
        {
            if (limitation.ShiftCategory.Id != ShiftCategory.Id)
                throw new ArgumentException("Shift category of the limitation must match this shift category",
                                            "limitation");

            if (_limit == PropertyState.UnKnown)
            {
                _limit = PropertyState.True;
                //_maxNumberInitialized = true;
                //_maxNumberOf = 0;
            }
            else
            {
                if (_limit == PropertyState.False)
                    _limit = PropertyState.UnDeterment;
            }

            if (_weekly == PropertyState.UnKnown)
            {
                if (limitation.Weekly)
                {
                    _weekly = PropertyState.True;
                    _period = PropertyState.False;
                }
                else
                {
                    _weekly = PropertyState.False;
                    _period = PropertyState.True;
                }
            }
            else
            {
                if (_weekly == PropertyState.False && limitation.Weekly)
                {
                    _period = _weekly = PropertyState.UnDeterment;
                }
                else
                {
                    if(_weekly == PropertyState.True && !limitation.Weekly)
                        _period = _weekly = PropertyState.UnDeterment;
                }
            }

            if(!_maxNumberInitialized)
            {
                _maxNumberInitialized = true;
                _maxNumberOf = limitation.MaxNumberOf;
            }
            else
            {
                if (_maxNumberOf.HasValue)
                {
                    if (_maxNumberOf != limitation.MaxNumberOf)
                    {
                        _maxNumberOf = null;
                    }
                }
            }            
        }

        public void CombineWithNoLimitation(IShiftCategory shiftCategory)
        {
            if (shiftCategory != ShiftCategory)
                throw new ArgumentException("Shift category must match this shift category",
                                            "shiftCategory");

            if (_limit == PropertyState.UnKnown)
            {
                _limit = PropertyState.False;
            }
            else
            {
                if (_limit == PropertyState.True)
                    _limit = PropertyState.UnDeterment;
            }
           
        }

        public void Clear()
        {
            _maxNumberOf = 0;
            _maxNumberInitialized = false;
            _limit = PropertyState.UnKnown;
            _weekly = PropertyState.UnKnown;
            _period = PropertyState.UnKnown;
            //_initialized = false;
        }

        private static bool? propertyStateToNullableBool(PropertyState state)
        {
            bool? ret = true;

            switch (state)
            {
                case PropertyState.UnKnown:
                    ret = false;
                    break;
                case PropertyState.UnDeterment:
                    ret = null;
                    break;
                case PropertyState.False:
                    ret = false;
                    break;
                case PropertyState.True:
                    ret = true;
                    break;
            }
            return ret;
        }
    }
}
