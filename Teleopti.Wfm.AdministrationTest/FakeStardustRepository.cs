﻿using System;
using System.Collections.Generic;
using System.Linq;
using IStardustRepository = Teleopti.Ccc.Infrastructure.Repositories.Stardust.IStardustRepository;
using Job = Teleopti.Ccc.Infrastructure.Repositories.Stardust.Job;
using JobDetail = Teleopti.Ccc.Infrastructure.Repositories.Stardust.JobDetail;
using JobFilterModel = Teleopti.Ccc.Infrastructure.Repositories.Stardust.JobFilterModel;
using WorkerNode = Teleopti.Ccc.Infrastructure.Repositories.Stardust.WorkerNode;

namespace Teleopti.Wfm.AdministrationTest
{
	public class FakeStardustRepository : IStardustRepository
	{
		private readonly List<WorkerNode> _nodes;
		private readonly List<Job> _jobs;

		public FakeStardustRepository()
		{
			_nodes = new List<WorkerNode>();
			_jobs = new List<Job>();
		}

		public void Has(WorkerNode node)
		{
			_nodes.Add(node);
		}

		public void Has(Job job)
		{
			job.Started = DateTime.UtcNow;
			job.Ended = DateTime.UtcNow.AddSeconds(1); //Cheating
			job.Result = "Success";
			_jobs.Add(job);
		}

		public void Clear()
		{
			_nodes.Clear();
			_jobs.Clear();
		}

		public void DeleteQueuedJobs(Guid[] jobIds)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllFailedJobs(int @from, int to)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllFailedJobs(JobFilterModel filter)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllJobs(int @from, int to)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllJobs(JobFilterModel filter)
		{
			throw new NotImplementedException();
		}

		public List<Job> GetAllQueuedJobs(int @from, int to)
		{
			return new List<Job>();
		}

		public IList<Job> GetAllQueuedJobs(JobFilterModel filter)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllRunningJobs()
		{
			throw new NotImplementedException();
		}

		public List<WorkerNode> GetAllWorkerNodes()
		{
			return _nodes;
		}

		public Job GetJobByJobId(Guid jobId)
		{
			return !_jobs.Any() ? null : _jobs.FirstOrDefault(x => x.JobId == jobId);
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetJobsByNodeId(Guid nodeId, int @from, int to)
		{
			throw new NotImplementedException();
		}

		public Job GetQueuedJob(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public List<Guid> SelectAllBus(string connString)
		{
			throw new NotImplementedException();
		}

		public List<Guid> SelectAllTenants()
		{
			throw new NotImplementedException();
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			return _nodes.FirstOrDefault(x => x.Id == nodeId);
		}

		public List<string> GetAllTypes()
		{
			throw new NotImplementedException();
		}

		public List<string> GetAllTypesInQueue()
		{
			throw new NotImplementedException();
		}
	}
}