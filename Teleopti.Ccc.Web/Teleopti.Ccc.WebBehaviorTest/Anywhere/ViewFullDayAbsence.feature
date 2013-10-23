Feature: View full day absence
	In order to keep track of scheduled full day absences for a person in my team
	As a team leader
	I want to see the scheduled absences for the person
	
Background:
	Given there is a team with
	| Field | Value      |
	| Name  | Team green |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |	
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |

Scenario: View full day absence in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |
	When I view schedules for '2013-10-22'
	Then I should see 'Pierre Baldi' with absence 
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
#	| Description | Vacation |

Scenario: View full day absence for person
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-10-22'
	Then I should see a shift layer with
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
#	| Description | Vacation |
	And I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Color      | Red              |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |

@ignore
Scenario: View full day absence in team schedule Prototype
	Given I have the role 'Anywhere Team Green'
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	| Color | Black   |
	And there is an absence with
	| Field | Value     |
	| Name  | Vacation2 |
	| Color | LightPink |
	And there is a day off named 'Day off'
	And there is a shift category named 'Day'

	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Pierre Baldi' has a shift with
	| Field | Value    |
	| Shift category | Day              |
	| Start time     | 2013-10-22 07:00 |
	| End time       | 2013-10-22 14:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation2        |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |

	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Ashley Andeen' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Start time       | 2013-10-22 10:00 |
	| End time         | 2013-10-22 18:00 |
	| Lunch start time | 2013-10-22 12:00 |
	| Lunch end time   | 2013-10-22 13:00 |
	And 'Ashley Andeen' has an absence with
	| Field      | Value            |
	| Name       | Illness          |
	| Start time | 2013-10-22 17:00 |
	| End time   | 2013-10-22 18:00 |

	And 'Mathias Stenbom' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Mathias Stenbom' has an absence with
	| Field      | Value            |
	| Name       | Illness          |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |

	And 'Martin Fowler' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Martin Fowler' has a day off named 'Day off' on '2013-10-22'

	And 'Tamas Balog' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Tamas Balog' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Start time       | 2013-10-22 06:00 |
	| End time         | 2013-10-22 15:00 |
	| Lunch start time | 2013-10-22 10:00 |
	| Lunch end time   | 2013-10-22 11:00 |

	And 'Kunning Mao' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-21 |
	| Team       | Team green |
	And 'Kunning Mao' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Start time       | 2013-10-21 22:00 |
	| End time         | 2013-10-22 06:00 |
	And 'Kunning Mao' has a day off named 'Day off' on '2013-10-22'

	And 'Tina Hammarström' has a person period with
	| Field      | Value      |
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And 'Tina Hammarström' has a day off named 'Day off' on '2013-10-22'
	And 'Tina Hammarström' has an absence with
	| Field      | Value            |
	| Name       | Illness          |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |

	When I view schedules for '2013-10-22'
	Then I should see 'Pierre Baldi' with absence 
	| Field       | Value     |
	| Start time  | 08:00     |
	| End time    | 16:00     |
	| Color       | Red       |
	| Description | Vacation2 |
