using System;
using System.IO;
using NUnit.Framework;
using TfsBuild.AzureLM.Activities.BlobTransfer;
using TfsBuild.AzureLM.Activities.Models;

namespace ActivityTests
{
    [TestFixture]
    public class BlobTransferTests
    {
        const string CertificateThumbprint = "C6C2E8D9CD5D5C17239A1F3E7A9A2182C6028AC1";
        const string BadCertificateThumbprint = "C6C2E8D9CD5D5C17239A1F3E7A9A2182C6028AC1D";
        const string SubscriptionId = "80df9fae-60b7-4884-89a0-f7bec854e581";
        const string ServiceName = "TestService";
        const string StorageAccountName = "helloazurediagnostics";
        const string ContainerName = "deploycontainer2";
        private const string FilePath = @"C:\fictional\path\for\testing.dat";

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlobTransferTests_WhenCallingUploadBlobWithNullFilePathShouldThrowException()
        {
            var storage = new Storage(StorageAccountName, ContainerName, CertificateThumbprint);
            BlobTransfer.UploadBlob(storage, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlobTransferTests_WhenCallingUploadBlobWithEmptyFilePathShouldThrowException()
        {
            var storage = new Storage(StorageAccountName, ContainerName, CertificateThumbprint);
            BlobTransfer.UploadBlob(storage, string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlobTransferTests_WhenCallingUploadBlobWithNullStorageShouldThrowException()
        {
            BlobTransfer.UploadBlob(null, FilePath);
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void BlobTransferTests_WhenCallingUploadBlobWithBadCredentialsShouldThrowException()
        {
            var storage = new Storage(StorageAccountName, ContainerName, BadCertificateThumbprint);
            BlobTransfer.UploadBlob(storage, FilePath);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void BlobTransferTests_WhenCallingUploadBlobWithBadFileShouldThrowException()
        {
            var storage = new Storage(StorageAccountName, ContainerName, CertificateThumbprint);
            BlobTransfer.UploadBlob(storage, FilePath);
        }

    }
}
