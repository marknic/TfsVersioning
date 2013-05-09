// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Utility
{
    public static class Constants
    {
        public const string AzureStorageServiceDefaultName = "azuredeploystorage";
        public const string AzureContainerDefaultName = "azuredeploycontainer";
        public const string StorageDescription = "Azure ALM interim deployment storage";
        public const string StorageLabel = "Azure ALM Storage";
        public const string UriBase = "https://management.core.windows.net/{0}";
        public const string WindowsAzureSchema = "http://schemas.microsoft.com/windowsazure";
    }
}
