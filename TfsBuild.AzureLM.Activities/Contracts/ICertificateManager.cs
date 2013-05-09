using System.Security.Cryptography.X509Certificates;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Contracts
{
    public interface ICertificateManager
    {
        X509Certificate2 Certificate { get; }
        string Thumbprint { get; }
    }
}
