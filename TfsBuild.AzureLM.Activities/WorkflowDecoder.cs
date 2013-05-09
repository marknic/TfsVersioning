using System;
using System.Activities;
using System.Drawing;
using Microsoft.TeamFoundation.Build.Client;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities
{
    [ToolboxBitmap(typeof(WorkflowDecoder), "Resources.azurelm.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class WorkflowDecoder : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> WorkflowName { get; set; }

        public OutArgument<string> DecodedWorkflowName { get; set; } 

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var workflowName = WorkflowName.Get(context);

            if (string.IsNullOrWhiteSpace(workflowName))
            {
                throw new ArgumentException("Workflow must not be null or empty to be decoded.");
            }

            var decodedWorkflowName = Execute(workflowName);

            DecodedWorkflowName.Set(context, decodedWorkflowName);
        }

        public const string Staging = "staging";
        public const string Production = "production";
        public const string Stag = "stag";
        public const string Prod = "prod";
        public const string Demote = "demote";
        public const string Delete = "delete";
        public const string NoDelete = "no";
        public const string Upload = "upload";


        public string Execute(string workflowNameToDecode)
        {
            var staging = false;
            var production = false;
            var demote = false;
            var delete = false;
            string decodedWorkflow;

            if (workflowNameToDecode == null)
            {
                return null;
            }

            var textToDecodeLower = workflowNameToDecode.ToLower();

            if (textToDecodeLower.Contains(Upload))
            {
                return "UploadOnly";
            }

            if (textToDecodeLower.Contains(Staging))
            {
                staging = true;
            } 
            else if (textToDecodeLower.Contains(Production))
            {
                production = true;

                if (textToDecodeLower.Contains(Demote))
                {
                    demote = true;
                }
            }

            if ((staging == false) && (production == false))
            {
                if (textToDecodeLower.Contains(Stag))
                {
                    staging = true;
                }
                else if (textToDecodeLower.Contains(Prod))
                {
                    production = true;

                    if (textToDecodeLower.Contains(Demote))
                    {
                        demote = true;
                    }
                }
            }

            if ((staging == false) && (production == false))
            {
                return null;
            }

            if (!textToDecodeLower.Contains(NoDelete) && textToDecodeLower.Contains(Delete))
            {
                delete = true;
            }
            
            if (staging)
            {
                decodedWorkflow = "Staging";
            } 
            else
            {
                decodedWorkflow = "Production";

                if (demote)
                {
                    decodedWorkflow += "Demote";
                }
            }

            if (delete)
            {
                decodedWorkflow += "Delete";
            }
            else
            {
                decodedWorkflow += "NoDelete";
            }

            return decodedWorkflow;
        }
    }
}
