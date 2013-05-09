using System;
using System.Text;
using TfsBuild.AzureLm.Activities.Utility;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLM.Activities.Models
{
    public class Storage
    {
        /// <summary>
        /// Extracts and organizes all of the information required for creating or using an Azure blob storage container 
        /// </summary>
        /// <param name="storageService">Azure storage service (blob storage account)</param>
        /// <param name="container">Name of the container within blob storage</param>
        /// <param name="storageKey">Key for accessing blob storage</param>
        public Storage(string storageService, string container, string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageService)) throw new ArgumentNullException("storageService");
            if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentNullException("storageKey");

            StorageService = storageService.Replace("/", " ").Replace("\\", " ").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(container))
            {
                container = string.Empty;
            }

            container = container.Trim();

            var namesArray = container.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (namesArray.Length > 0)
            {
                var builder = new StringBuilder();
                for (var i = 0; i < namesArray.Length; i++)
                {
                    if (i == 0)
                    {
                        namesArray[0] = namesArray[0].ToLower();
                    }

                    builder.Append(namesArray[i].Trim());

                    if (i < namesArray.Length - 1)
                    {
                        builder.Append('/');
                    }
                }

                StorageContainer = builder.ToString();
            }
            else
            {
                StorageContainer = Constants.AzureContainerDefaultName;
            }

            StorageKey = storageKey;
        }

        public string StorageService { get; private set; }
        public string StorageContainer { get; private set; }
        public string StorageKey { get; private set; }
    }

}
