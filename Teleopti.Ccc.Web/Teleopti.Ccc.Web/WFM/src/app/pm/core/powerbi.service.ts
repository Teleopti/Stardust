import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { ReportConfig } from '../models/ReportConfig.model';

@Injectable()
export class PowerBIService {
  constructor(private http: HttpClient) { }
  
  private api_url = '../api/PowerBiReport/ReportConfig';
  private headers = new Headers({'Content-Type': 'application/json'});

  async getReportConfig(): Promise<ReportConfig> {
	return this.http.get(this.api_url)
		.toPromise()
		.catch(this.handleError);
  }

  private handleError(error: any): Promise<any> {
	console.error('An error occurred', error);
	return Promise.reject(error.message || error);
  }
}