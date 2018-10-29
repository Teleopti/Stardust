UPDATE rta.Events SET [Type] = 'ShiftStart' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonShiftStartEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'ShiftEnd' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonShiftEndEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'ArrivedLateForWork' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonArrivedLateForWorkEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'ApprovedPeriodRemoved' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.ApprovedPeriodRemovedEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'RuleChanged' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonRuleChangedEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'StateChanged' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonStateChangedEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'ApprovedAsInAdherence' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PeriodApprovedAsInAdherenceEvent, Teleopti.Wfm.Adherence'
UPDATE rta.Events SET [Type] = 'AdherenceDayStart' WHERE [Type] = 'Teleopti.Wfm.Adherence.Domain.Events.PersonAdherenceDayStartEvent, Teleopti.Wfm.Adherence'
GO

UPDATE rta.Events SET [Type] = 'ShiftStart' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonShiftStartEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'ShiftEnd' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonShiftEndEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'ArrivedLateForWork' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonArrivedLateForWorkEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'ApprovedPeriodRemoved' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.ApprovedPeriodRemovedEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'RuleChanged' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonRuleChangedEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'StateChanged' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonStateChangedEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'ApprovedAsInAdherence' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PeriodApprovedAsInAdherenceEvent, Teleopti.Ccc.Domain'
UPDATE rta.Events SET [Type] = 'AdherenceDayStart' WHERE [Type] = 'Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events.PersonAdherenceDayStartEvent, Teleopti.Ccc.Domain'
GO
