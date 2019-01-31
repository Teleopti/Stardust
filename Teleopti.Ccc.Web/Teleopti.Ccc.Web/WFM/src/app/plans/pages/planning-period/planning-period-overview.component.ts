import {Component, Inject} from '@angular/core';
import {PlanningPeriodService} from "../../shared";
import {IStateService} from "angular-ui-router";
@Component({
	selector: 'plans-period-overview',
	templateUrl: './planning-period-overview.component.html',
	styleUrls: ['./planning-period-overview.component.scss'],
	providers: []
})
export class PlanningPeriodOverviewComponent {

	ppId: string;
	valLoading: boolean = true;

	dayNodes;

	valData = {
		totalValNum: 0,
		totalPreValNum: 0,
		totalLastValNum: 0,
		scheduleIssues: [],
		preValidation: []
	};

	constructor(private planningPeriodService: PlanningPeriodService, @Inject('$state') private $state: IStateService ) {
		this.ppId = $state.params.ppId.trim();
	}

	ngOnInit(){
		this.planningPeriodService.lastJobResult(this.ppId).subscribe(data=>{
			let fullSchedulingResult = data.FullSchedulingResult;

			if (!fullSchedulingResult) return;
			this.dayNodes = fullSchedulingResult.SkillResultList ? fullSchedulingResult.SkillResultList : undefined;
		});
	}
	
	public launchSchedule(){
		this.planningPeriodService.launchScheduling(this.ppId).subscribe(()=>{
			this.checkProgress();
		});
	}
	
	private checkProgress(){
		
	}
}
