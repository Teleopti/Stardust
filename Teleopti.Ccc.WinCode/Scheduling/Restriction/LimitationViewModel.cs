using System;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    /// <summary>
    /// ViewModel for Limitations
    /// </summary>
    /// <remarks>
    /// Fix for making the viewmodel holding valid/invalid state
    /// If InvalidStatePossible is set to true, the viewmodel can be set in an invalid state, wich is different from the actual model
    /// If set to false when in invalid state, the viewmodel will be set to the last valid state
    /// This is to make the models behave the same way as they do in people/options
    /// Created by: henrika
    /// Created date: 2009-01-27
    /// </remarks>
    public class LimitationViewModel : ILimitationViewModel
    {
        private ILimitation _limitation;
        private bool _editable = true;
        private bool _enabled = true;
        private bool _editableStartTime = true;
        private bool _editableEndTime = true;
        private bool _invalidStatePossible;
        private TimeSpan? _invalidStartTime;
        private TimeSpan? _invalidEndTime;
        private bool _invalid;

        public LimitationViewModel(ILimitation limitation)
        {
            Limitation = limitation;
        }

        public LimitationViewModel(ILimitation limitation, bool startTimeIsEditable, bool endTimeIsEditable)
            : this(limitation)
        {
            EditableStartTime = startTimeIsEditable;
            EditableEndTime = endTimeIsEditable;
        }

        public ILimitation Limitation
        {
            get
            {
                //We must be in validstate to return the correct limitation
                
               if(InvalidStatePossible)
               {
                   //TODO: Refactor the whole ValidState 
                   InvalidStatePossible = false;
                   InvalidStatePossible = true;
               }
                return _limitation;

            }
            private set
            {
                _limitation = value;
                _invalidStartTime = _limitation.StartTime;
                _invalidEndTime = _limitation.EndTime;
            }
        }

        public string StartTime
        {
            get {  return InvalidStatePossible ? _limitation.StringFromTimeSpan(_invalidStartTime) : _limitation.StartTimeString;}
         
            set
            {
                if (Editable && StartTime != value)
                {
                    try
                    {
                        string temp = EndTime;
                        if (InvalidStatePossible)
                        {
                            _invalidStartTime = _limitation.TimeSpanFromString(value);
                            SetValidState();
                        }
                        else
                        {
                        	_limitation = recreateLimitation(_limitation.TimeSpanFromString(value), _limitation.EndTime);
                            _invalidStartTime = _limitation.EndTime;
                        }
                        if (temp!=EndTime) 
                            NotifyPropertyChanged("EndTime"); 

                        NotifyPropertyChanged("StartTime");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Keep the old value
                    }                   
                }
            }
        }

		 private ILimitation recreateLimitation(TimeSpan? startTime, TimeSpan? endTime)
		 {
		 	//ugly hack - move to ILimitation if needed later
			 //im in the middle of a huge refactoring phase... cant do it right now
			 if(_limitation is StartTimeLimitation)
				 return new StartTimeLimitation(startTime, endTime);
			 if(_limitation is EndTimeLimitation)
				 return new EndTimeLimitation(startTime, endTime);
			 if(_limitation is WorkTimeLimitation)
				 return new WorkTimeLimitation(startTime, endTime);
			 throw new NotImplementedException("Unkown ILimitation type");
		 }

        public string EndTime
        {
            get{ return InvalidStatePossible ? _limitation.StringFromTimeSpan(_invalidEndTime) : _limitation.EndTimeString;}

            set
            {
                if (Editable && value != EndTime)
                {
                    try
                    {
                        string temp = StartTime;
                        if (InvalidStatePossible)
                        {
                            _invalidEndTime = _limitation.TimeSpanFromString(value);
                            SetValidState();
                        }
                        else
                        {
									_limitation = recreateLimitation(_limitation.StartTime, _limitation.TimeSpanFromString(value));
                            _invalidEndTime = _limitation.EndTime;
                        }
                        if (temp!=StartTime) 
                            NotifyPropertyChanged("StartTime"); 
                        NotifyPropertyChanged("EndTime");
                    }

                    catch (ArgumentOutOfRangeException)
                    {
                        //Keep old value
                    }
                }
            }
        }

        public bool Editable
        {
            get { return _editable; }
            set
            {
                if (_editable != value)
                {
                    _editable = value;
                    NotifyPropertyChanged("Editable");
                }
            }
        }

        //Henrik: No need for propertychanged for now, this is set only once
        public bool EditableStartTime
        {
            get { return (_editableStartTime && Editable); }
            private set
            {
                _editableStartTime = value;
            }
        }

        //Henrik: No need for propertychanged for now, this is set only once
        public bool EditableEndTime
        {
            get { return (_editableEndTime && Editable); }
            private set
            {
                _editableEndTime = value;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        //Henrik: No need for propertychanged for now, this is set only once
        public bool InvalidStatePossible
        {
            get { return _invalidStatePossible; }
            set
            {
                if (!Invalid)
                {
                    _invalidStatePossible = value;
                    StartTime = _limitation.StringFromTimeSpan(_invalidStartTime);
                    EndTime = _limitation.StringFromTimeSpan(_invalidEndTime);
                    SetValidState();
                }
                else
                {
                    _invalidStatePossible = value;
                    SetValidState();
                }
            }
        }

        private void NotifyPropertyChanged(string property)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}
        }

        public bool Invalid
        {
            get
            {
               
                return _invalid;
            }
            private set
            {
                if (_invalid!=value)
                {
                    _invalid = value;
                    NotifyPropertyChanged("Invalid");
                }
            }
        }

        private void SetValidState()
        {
            Invalid = (InvalidStatePossible) ? _invalidStartTime > _invalidEndTime : false;    
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
