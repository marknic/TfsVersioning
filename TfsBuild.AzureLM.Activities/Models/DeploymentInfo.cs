using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TfsBuild.AzureLm.Activities.Enumerations;
using TfsBuild.AzureLm.Activities.Utility;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Models
{
    public class DeploymentInfo
    {
        public DeploymentSlots Slot { get; private set; }
        public string Name { get; private set; }
        public string Label { get; private set; }
        public string Status { get; private set; }
        public string PrivateId { get; private set; }
        public string Url { get; private set; }
        public bool Locked { get; private set; }

        public DeploymentInfo(string name, string label, string status, string slot, string privateId, string url, string locked)
        {
            Name = name;
            Label = label;
            Status = status;
            PrivateId = privateId;
            Url = url;

            DeploymentSlots deploymentSlot;
            var slotSuccess = Enum.TryParse(slot, out deploymentSlot);
            if (slotSuccess == false)
            {
                throw new ArgumentException("Slot could not be parsed.  Value: " + slot);
            }

            Slot = deploymentSlot;

            bool deploymentLocked;
            var lockedSuccess = bool.TryParse(locked, out deploymentLocked);
            if (lockedSuccess == false)
            {
                throw new ArgumentException("Locked could not be parsed.  Value: " + locked);
            }

            Locked = deploymentLocked;
        }


        public static List<DeploymentInfo> GetDeploymentList(XDocument hostedServiceProperties)
        {
            if (hostedServiceProperties == null)
            {
                throw new ArgumentNullException("hostedServiceProperties");
            }

            var hostedService = hostedServiceProperties.Element(Constants.WindowsAzureSchema + "HostedService");

            if (hostedService == null)
            {
                throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (HostedService).");
            }

            var deployments = hostedService.Element(Constants.WindowsAzureSchema + "Deployments");

            if (deployments == null)
            {
                throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Deployments).");
            }

            var deploymentList = deployments.Elements(Constants.WindowsAzureSchema + "Deployment");

            var deploymentInfoList = new List<DeploymentInfo>();

            foreach (var deploymentElement in deploymentList)
            {
                var nameElement = deploymentElement.Element(Constants.WindowsAzureSchema + "Name");
                if (nameElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Name).");

                var labelElement = deploymentElement.Element(Constants.WindowsAzureSchema + "Label");
                if (labelElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Label).");

                var statusElement = deploymentElement.Element(Constants.WindowsAzureSchema + "Status");
                if (statusElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Status).");

                var slotElement = deploymentElement.Element(Constants.WindowsAzureSchema + "DeploymentSlot");
                if (slotElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (DeploymentSlot).");

                var privateIdElement = deploymentElement.Element(Constants.WindowsAzureSchema + "PrivateID");
                if (privateIdElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (PrivateID).");

                var urlElement = deploymentElement.Element(Constants.WindowsAzureSchema + "Url");
                if (urlElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Url).");

                var lockedElement = deploymentElement.Element(Constants.WindowsAzureSchema + "Locked");
                if (lockedElement == null) throw new ArgumentException("XDocument hostedServiceProperties does not contain valid XML (Locked).");


                deploymentInfoList.Add(new DeploymentInfo(nameElement.Value, DeploymentSupport.DecodeFromBase64String(labelElement.Value), statusElement.Value, slotElement.Value,
                   privateIdElement.Value, urlElement.Value, lockedElement.Value)); 

            }

            return deploymentInfoList;
        }
    }
}
