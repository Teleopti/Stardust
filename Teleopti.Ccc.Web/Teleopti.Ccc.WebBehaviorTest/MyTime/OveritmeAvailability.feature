Feature: Overtime availability
	In order to submit when I am available for overtime work
	As an agent 
	I want to volunteer for overtime

Background:
	Given there is a role with
	| Field                           | Value                           |
	| Name                            | Access to overtime availability |
	| Access to overtime availability | true                            |
	And there is a role with
	| Field                           | Value                              |
	| Name                            | No access to overtime availability |
	| Access to overtime availability | False                              |
	And there are shift categories
	| Name  |
	| Day   |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2023-08-28         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |

Scenario: Cannot add overtime availability if no permission
	Given I have the role 'No access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	Then I should not see add overtime availability button

Scenario: Default overtime availability values on shift
	Given I have the role 'Access to overtime availability'
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-08-20 09:00 |
	| End time         | 2013-08-20 18:00 |
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	Then I should see add overtime availability form with
	| Field      | Value      |
	| Start date | 2013-08-20 |
	| End date   | 2013-08-20 |
	| Start time | 18:00      |
	| End time   | 19:00      |

Scenario: Default overtime availability values on empty day
	Given I have the role 'Access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	Then I should see add overtime availability form with
	| Field      | Value      |
	| Start date | 2013-08-20 |
	| End date   | 2013-08-20 |
	| Start time | 08:00      |
	| End time   | 17:00      |

Scenario: Default overtime availability values on dayoff
	Given I have the role 'Access to overtime availability'
	And I have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2013-08-20 |
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	Then I should see add overtime availability form with
	| Field      | Value      |
	| Start date | 2013-08-20 |
	| End date   | 2013-08-20 |
	| Start time | 08:00      |
	| End time   | 17:00      |

Scenario: Submit overtime availability
	Given I have the role 'Access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	And I input overtime availability with
	| Field             | Value |
	| Start time        | 16:30 |
	| End time          | 03:00 |
	| End time next day | true  |
	And I click submit button
	Then I should see an overtime availability symbol with tooltip
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 16:30      |
	| End time   | 03:00 +1   |

Scenario: Cancel adding overtime availability
	Given I have the role 'Access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	And I input overtime availability with
	| Field             | Value |
	| Start time        | 16:30 |
	| End time          | 03:00 |
	| End time next day | true  |
	And I click the cancel button
	Then I should not see an overtime availability symbol for date '2013-08-20'

Scenario: Add invalid overtime availability
	Given I have the role 'Access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	And I input overtime availability with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 11:00 |
	And I click submit button
	Then I should see the 'overtime availability error' 'End time'
	And I should not see an overtime availability symbol for date '2013-08-20'

Scenario: See existing overtime availability
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field             | Value      |
	| Date              | 2013-08-20 |
	| Start time        | 16:30      |
	| End time          | 17:30      |
	When I view my week schedule for date '2013-08-20'
	Then I should see an overtime availability symbol with tooltip
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 16:30      |
	| End time   | 17:30      |
	And I should see overtime availability bar with
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 16:30      |
	| End time   | 17:30      |

Scenario: See existing overtime availability over midnight
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field             | Value      |
	| Date              | 2013-08-21 |
	| Start time        | 16:30      |
	| End time          | 03:15      |
	| End time next day | true       |
	When I view my week schedule for date '2013-08-20'
	Then I should see an overtime availability symbol with tooltip
	| Field      | Value      |
	| Date       | 2013-08-21 |
	| Start time | 16:30      |
	| End time   | 03:15 +1   |
	And I should see overtime availability bar with
	| Field      | Value      |
	| Date       | 2013-08-21 |
	| Start time | 16:30      |
	| End time   | 24:00      |
	And I should see overtime availability bar with
	| Field      | Value      |
	| Date       | 2013-08-22 |
	| Start time | 00:00      |
	| End time   | 03:15      |

Scenario: Click overtime availability bar
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field             | Value      |
	| Date              | 2013-08-20 |
	| Start time        | 16:30      |
	| End time          | 03:15      |
	| End time next day | true       |
	When I view my week schedule for date '2013-08-20'
	And I click overtime availability bar
	Then I should see add overtime availability form with
	| Field             | Value      |
	| Start date        | 2013-08-20 |
	| End date          | 2013-08-21 |
	| Start time        | 16:30      |
	| End time          | 03:15      |
	| End time next day | true       |

Scenario: Click overtime availability bar span to next day
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field             | Value      |
	| Date              | 2013-08-25 |
	| Start time        | 16:30      |
	| End time          | 03:15      |
	| End time next day | true       |
	When I view my week schedule for date '2013-08-26'
	And I click overtime availability bar
	Then I should see add overtime availability form with
	| Field             | Value      |
	| Start date        | 2013-08-25 |
	| End date          | 2013-08-26 |
	| Start time        | 16:30      |
	| End time          | 03:15      |
	| End time next day | true       |

Scenario: Default values on existing overtime availability
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field             | Value      |
	| Date              | 2013-08-20 |
	| Start time        | 16:30      |
	| End time          | 18:00      |
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	Then I should see add overtime availability form with
	| Field             | Value      |
	| Start date        | 2013-08-20 |
	| End date          | 2013-08-20 |
	| Start time        | 16:30      |
	| End time          | 18:00      |

Scenario: Replace overtime availability
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 15:30      |
	| End time   | 22:45      |
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click add new overtime availability
	And I input overtime availability with
	| Field      | Value |
	| Start time | 16:30 |
	| End time   | 23:45 |
	And I click submit button
	Then I should see an overtime availability symbol with tooltip
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 16:30      |
	| End time   | 23:45      |

Scenario: Delete overtime availability
	Given I have the role 'Access to overtime availability'
	And I have an overtime availability with
	| Field      | Value      |
	| Date       | 2013-08-20 |
	| Start time | 16:30      |
	| End time   | 17:30      |
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click overtime availability
	And I click delete button
	Then I should not see an overtime availability symbol for date '2013-08-20'

Scenario: Cannot see delete button when no existing overtime availability
	Given I have the role 'Access to overtime availability'
	And I view my week schedule for date '2013-08-20'
	When I click on the day summary for date '2013-08-20'
	And I click overtime availability
	Then I should not see delete button
