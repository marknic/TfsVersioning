using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuild.Versioning.Activities.Tests.Stubs
{
    public class BuildDefinitionStub : IBuildDefinition 
    {
        #region IBuildDefinition Members

        IRetentionPolicy IBuildDefinition.AddRetentionPolicy(BuildReason reason, BuildStatus status, int numberToKeep, DeleteOptions deleteOptions)
        {
            throw new NotImplementedException();
        }

        ISchedule IBuildDefinition.AddSchedule()
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IBuildDefinition buildDefinition)
        {
            throw new NotImplementedException();
        }

        IBuildController IBuildDefinition.BuildController
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Uri IBuildDefinition.BuildControllerUri
        {
            get { throw new NotImplementedException(); }
        }

        IBuildServer IBuildDefinition.BuildServer
        {
            get { throw new NotImplementedException(); }
        }

        //string IBuildDefinition.ConfigurationFolderPath
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public DateTime DateCreated { get; set; }

        int IBuildDefinition.ContinuousIntegrationQuietPeriod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        ContinuousIntegrationType IBuildDefinition.ContinuousIntegrationType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IBuildRequest IBuildDefinition.CreateBuildRequest()
        {
            throw new NotImplementedException();
        }

        IBuildDetail IBuildDefinition.CreateManualBuild(string buildNumber, string dropLocation, BuildStatus buildStatus, IBuildController controller, string requestedFor)
        {
            throw new NotImplementedException();
        }

        IBuildDetail IBuildDefinition.CreateManualBuild(string buildNumber, string dropLocation)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail CreateManualBuild(string buildNumber)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method has been deprecated. Please remove all references.", true)]
        public IProjectFile CreateProjectFile()
        {
            throw new NotImplementedException();
        }

        IBuildDefinitionSpec IBuildDefinition.CreateSpec()
        {
            throw new NotImplementedException();
        }

        //IBuildAgent IBuildDefinition.DefaultBuildAgent
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //Uri IBuildDefinition.DefaultBuildAgentUri
        //{
        //    get { throw new NotImplementedException(); }
        //}

        string IBuildDefinition.DefaultDropLocation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IBuildDefinition.Delete()
        {
            throw new NotImplementedException();
        }

        string IBuildDefinition.Description
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool IBuildDefinition.Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DefinitionQueueStatus QueueStatus { get; set; }

        string IBuildDefinition.Id
        {
            get { return "My TestBuildDefinition ID"; }
        }

        Uri IBuildDefinition.LastBuildUri
        {
            get { throw new NotImplementedException(); }
        }

        string IBuildDefinition.LastGoodBuildLabel
        {
            get { throw new NotImplementedException(); }
        }

        Uri IBuildDefinition.LastGoodBuildUri
        {
            get { throw new NotImplementedException(); }
        }

        IProcessTemplate IBuildDefinition.Process
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IBuildDefinition.ProcessParameters
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IDictionary<string, object> AttachedProperties { get; set; }
        public DefinitionTriggerType TriggerType { get; set; }

        IBuildDetail[] IBuildDefinition.QueryBuilds()
        {
            throw new NotImplementedException();
        }

        public void Refresh(string[] propertyNameFilters, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public int BatchSize { get; set; }

        //Dictionary<BuildStatus, IRetentionPolicy> IBuildDefinition.RetentionPolicies
        //{
        //    get { throw new NotImplementedException(); }
        //}

        List<IRetentionPolicy> IBuildDefinition.RetentionPolicyList
        {
            get { throw new NotImplementedException(); }
        }

        void IBuildDefinition.Save()
        {
            throw new NotImplementedException();
        }

        List<ISchedule> IBuildDefinition.Schedules
        {
            get { throw new NotImplementedException(); }
        }

        public List<IBuildDefinitionSourceProvider> SourceProviders { get; private set; }

        IWorkspaceTemplate IBuildDefinition.Workspace
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IBuildGroupItem Members

        string IBuildGroupItem.FullPath
        {
            get { throw new NotImplementedException(); }
        }

        string IBuildGroupItem.Name
        {
            get { return "Build Name"; }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IBuildGroupItem.Refresh()
        {
            throw new NotImplementedException();
        }

        string IBuildGroupItem.TeamProject
        {
            get { throw new NotImplementedException(); }
        }

        Uri IBuildGroupItem.Uri
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
