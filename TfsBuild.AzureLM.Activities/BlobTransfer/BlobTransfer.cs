using System;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using TfsBuild.AzureLM.Activities.Models;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLM.Activities.BlobTransfer
{
    public class BlobTransfer
    {
        public static Uri UploadBlob(Storage storageProperties, string sourceFilePath)
        {
            StorageCredentials credentials;
            Uri blobUri;

            if (storageProperties == null) throw new ArgumentNullException("storageProperties");
            if (string.IsNullOrWhiteSpace(sourceFilePath)) throw new ArgumentNullException("sourceFilePath");

            try
            {
                credentials =
                    new StorageCredentials(storageProperties.StorageService, storageProperties.StorageKey);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Blob storage credentials are invalid. - " + ex.Message);
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("the file in sourceFilePath does not exist: " + sourceFilePath);
            }

            var fileName = Path.GetFileName(sourceFilePath);

            var storageAccount = new CloudStorageAccount(credentials, true);

            //Create a new client object.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            var container = blobClient.GetContainerReference(storageProperties.StorageContainer);

            // Create the container if it does not already exist.
            container.CreateIfNotExists();

            // Output container URI to debug window.
            Debug.WriteLine(container.Uri);

            var blob = container.GetBlockBlobReference(fileName);

            try
            {
                using (var fileStream = File.OpenRead(sourceFilePath))
                {
                    blob.UploadFromStream(fileStream);
                    blobUri = blob.Uri;
                }
            }
            catch (Exception ex)
            {
                var error = string.Format("Error uploading the file: '{0}' - {1}", fileName, ex.Message);
                Debug.WriteLine(error);
                throw new ApplicationException(error);
            }

            return blobUri;
        }
    }
}
