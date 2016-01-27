﻿namespace NodeTest.JobHandlers
{
    public class FailingJobParams
    {
        public FailingJobParams(string error)
        {
            Error = error;
        }

        public string Error { get; private set; }
    }
}