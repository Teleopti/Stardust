using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using System.Drawing;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// The concrete implementation of IDayOff
    /// </summary>
    /// <remarks>
    /// Created by: shirang
    /// Created date: 2008-10-28
    /// </remarks>
    public class DayOffTemplate : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IDayOffTemplate, IDeleteTag, ICloneableEntity<DayOffTemplate>, IAggregateRoot_Events
    {
        private Description _description;
        private TimeSpan _flexibility;
        private TimeSpan _targetLength;
        private TimeSpan _anchor;
        private Color _displayColor = Color.Gray;
        private bool _isDeleted;
    	private string _payrollCode;


	    public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
		    if (operation == DomainUpdateType.Update || operation == DomainUpdateType.Insert)
			    AddEvent(new DayOffTemplateChangedEvent
			    {
				    DayOffTemplateId = Id.GetValueOrDefault(),
			    });
	    }
		
	    public virtual Description Description
	    {
		    get { return _description; }
	    }

	    public virtual void ChangeDescription(string name, string shortName)
	    {
		    _description = new Description(name, shortName);
	    }

	    /// <summary>
	    /// Gets or sets the flexibility.
	    /// </summary>
	    /// <remarks>
	    /// Created by: shirang
	    /// Created date: 2008-10-28
	    /// </remarks>
	    public virtual TimeSpan Flexibility
	    {
		    get { return _flexibility; }
		    protected set
		    {
			    if (value.TotalMinutes > _targetLength.TotalMinutes/2d)
				    value = TimeSpan.FromMinutes(_targetLength.TotalMinutes/2);
			    _flexibility = value;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the length (duration).
	    /// </summary>
	    /// <remarks>
	    /// Created by: shirang
	    /// Created date: 2008-10-28
	    /// </remarks>
	    public virtual TimeSpan TargetLength
	    {
		    get { return _targetLength; }
		    protected set
		    {
			    _targetLength = value;
			    Flexibility = _flexibility;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the anchor point relative to the start of the parent interval.
	    /// </summary>
	    /// <remarks>
	    /// Created by: shirang
	    /// Created date: 2008-10-28
	    /// </remarks>
	    public virtual TimeSpan Anchor
	    {
		    get { return _anchor; }
		    set { _anchor = value; }
	    }

	    /// <summary>
	    /// Gets or sets the display color
	    /// </summary>
	    public virtual Color DisplayColor
	    {
		    get { return _displayColor; }
		    set { _displayColor = value; }
	    }

	    public virtual string PayrollCode
	    {
		    get { return _payrollCode; }
		    set { _payrollCode = value; }
	    }

	    public virtual bool IsDeleted
	    {
		    get { return _isDeleted; }
	    }

	    public DayOffTemplate(Description description)
	    {
		    _description = description;
	    }

	    public DayOffTemplate()
	    {
	    }

	    /// <summary>
	    /// Overloaded ToString method to return the Description's name
	    /// </summary>
	    /// <returns>The name of the DayOff</returns>
	    /// <remarks>
	    /// Created by: shirang
	    /// Created date: 2008-10-28
	    /// </remarks>
	    public override string ToString()
	    {
		    return _description.Name;
	    }

	    /// <summary>
	    /// Creates a new object that is a copy of the current instance.
	    /// </summary>
	    /// <returns>
	    /// A new object that is a copy of this instance.
	    /// </returns>
	    /// <filterpriority>2</filterpriority>
	    public virtual object Clone()
	    {
		    return EntityClone();
	    }

	    public virtual void SetTargetAndFlexibility(TimeSpan targetLength, TimeSpan flexibility)
	    {
		    TargetLength = targetLength;
		    Flexibility = flexibility;
	    }

	    public virtual void SetDeleted()
	    {
		    _isDeleted = true;
	    }

	    /// <summary>
	    /// Returns a clone of this T with IEntitiy.Id set to null.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2008-05-27
	    /// </remarks>
	    public virtual DayOffTemplate NoneEntityClone()
	    {
		    var clone = (DayOffTemplate) MemberwiseClone();
		    clone.SetId(null);

		    return clone;
	    }

	    /// <summary>
	    /// Returns a clone of this T with IEntitiy.Id as this T.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2008-05-27
	    /// </remarks>
	    public virtual DayOffTemplate EntityClone()
	    {
		    return (DayOffTemplate) MemberwiseClone();
	    }
    }
}