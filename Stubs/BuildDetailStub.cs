using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuild.Versioning.Activities.Tests.Stubs
{
    public class BuildDetailStub : IBuildDetail
    {
        public BuildDetailStub(int revisionNo)
        {
            BuildNumber = String.Format("{0}_{1}.{2}.{3}.{4}",
                    Environment.MachineName,
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    Convert.ToInt16(Math.Abs(revisionNo)));
        }

        public Guid RequestIntermediateLogs()
        {
            throw new NotImplementedException();
        }

        public string BuildNumber { get; set; }

        private readonly IBuildDefinition _buildDefinition = new BuildDefinitionStub();

        #region Implementation of IBuildDetail

        public void Connect(int pollingInterval, int timeout, ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public void Connect(int pollingInterval, ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult Delete()
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult Delete(DeleteOptions options)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void FinalizeStatus()
        {
            throw new NotImplementedException();
        }

        public void FinalizeStatus(BuildStatus status)
        {
            throw new NotImplementedException();
        }

        public void RefreshMinimalDetails()
        {
            throw new NotImplementedException();
        }

        public void RefreshAllDetails()
        {
            throw new NotImplementedException();
        }

        public void Refresh(string[] informationTypes, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Wait()
        {
            throw new NotImplementedException();
        }

        public bool Wait(TimeSpan pollingInterval, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool Wait(TimeSpan pollingInterval, TimeSpan timeout, ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public BuildPhaseStatus CompilationStatus
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string ConfigurationFolderPath
        {
            get { throw new NotImplementedException(); }
        }

        public string DropLocation
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string DropLocationRoot
        {
            get { throw new NotImplementedException(); }
        }

        public string LabelName
        {
            get { return "My Label Name"; }
            set { throw new NotImplementedException(); }
        }

        public bool KeepForever
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string LogLocation
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Quality
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public BuildStatus Status
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public BuildPhaseStatus TestStatus
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IBuildAgent BuildAgent
        {
            get { throw new NotImplementedException(); }
        }

        public Uri BuildAgentUri
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildController BuildController
        {
            get { throw new NotImplementedException(); }
        }

        public Uri BuildControllerUri
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildDefinition BuildDefinition
        {
            get { return _buildDefinition; }
        }

        public Uri BuildDefinitionUri
        {
            get { throw new NotImplementedException(); }
        }

        public bool BuildFinished
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildServer BuildServer
        {
            get { throw new NotImplementedException(); }
        }

        public string CommandLineArguments
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildInformation Information
        {
            get { throw new NotImplementedException(); }
        }

        public Uri ConfigurationFolderUri
        {
            get { throw new NotImplementedException(); }
        }

        public string LastChangedBy
        {
            get { throw new NotImplementedException(); }
        }

        public string LastChangedByDisplayName { get; private set; }

        public DateTime LastChangedOn
        {
            get { throw new NotImplementedException(); }
        }

        public string ProcessParameters
        {
            get { throw new NotImplementedException(); }
        }

        public BuildReason Reason
        {
            get { throw new NotImplementedException(); }
        }

        public ReadOnlyCollection<int> RequestIds { get; private set; }
        public ReadOnlyCollection<IQueuedBuild> Requests { get; private set; }

        public string RequestedBy
        {
            get { return "domain\\marknic"; }
        }

        public string RequestedFor
        {
            get { throw new NotImplementedException(); }
        }

        public string ShelvesetName
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDeleted
        {
            get { throw new NotImplementedException(); }
        }

        public string SourceGetVersion
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DateTime StartTime
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime FinishTime
        {
            get { throw new NotImplementedException(); }
        }

        public Uri Uri
        {
            get { throw new NotImplementedException(); }
        }

        public string TeamProject
        {
            get { return "Team Project Name"; }
        }

#pragma warning disable 67
        public event StatusChangedEventHandler StatusChanging;
        public event StatusChangedEventHandler StatusChanged;
        public event PollingCompletedEventHandler PollingCompleted;
#pragma warning restore 67

        #endregion
    }
}