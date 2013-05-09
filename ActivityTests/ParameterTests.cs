using System;
using NUnit.Framework;
using TfsBuild.AzureLm.Activities;

namespace ActivityTests
{
    [TestFixture]
    public class ParameterTests
    {
        const string CertificateThumbprint = "C6C2E8D9CD5D5C17239A1F3E7A9A2182C6028AC1";
        const string SubscriptionId = "80df9fae-60b7-4884-89a0-f7bec854e581";
        const string ServiceName = "TestService";
        const string StorageAccountName = "helloazurediagnostics";
        const bool StartAfterDeploy = true;
        const string ContainerName = "deploycontainer2";
        const string PackageFilepath = @"D:\AzureVPackage\Release\app.publish\HelloWindowsAzure.cspkg";
        const string BlobStorageFolder = @"deploy\package1";

        readonly string _deploymentName = string.Format("Deployment{0}", DateTime.Now.ToString("yyyyMMddhhmm"));
        readonly string _labelName = string.Format("Label{0}", DateTime.Now.ToString("yyyyMMddhhmm"));

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceNameNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      null,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionIdNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(null,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null);
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeploymentNameNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      null,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LabelNameNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      null,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StorageAccountNameNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      null,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConfigurationFilepathNull()
        {
            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      null,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PackageFilepathNull()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      null,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceNameEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      string.Empty,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionIdEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(string.Empty,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeploymentNameEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      string.Empty,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LabelNameEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      string.Empty,
                                      StorageAccountName,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StorageAccountNameEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      string.Empty,
                                      configurationFilepath,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConfigurationFilepathEmpty()
        {
            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      null,
                                      PackageFilepath,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PackageFilepathEmpty()
        {
            var configurationFilepath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ServiceConfiguration.Production.cscfg");

            var azureDeploy = new DeployOperation();

            azureDeploy.Execute(SubscriptionId,
                                      ServiceName,
                                      CertificateThumbprint,
                                      _deploymentName,
                                      _labelName,
                                      StorageAccountName,
                                      configurationFilepath,
                                      string.Empty,
                                      ContainerName,
                                      BlobStorageFolder,
                                      StartAfterDeploy,
                                      false,
                                      null
                                      );
        }
    }
}
