@WFM
@OnlyRunIfEnabled('WfmReportPortal_Basic_38825')

Feature: ReportItems
I can see reports and can navigate to the report.
Background:
	Given I am american
	And I have a role with
	| Field             | Value          |
	| Name              | Wfm Team Green |
	| Access to reports | True           |

	@ignore
Scenario: Should be able to see reports
	When  I view wfm reports	
	Then I should see all report items
