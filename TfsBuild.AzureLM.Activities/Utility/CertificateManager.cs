using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TfsBuild.AzureLm.Activities.Contracts;

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
    public class CertificateManager : ICertificateManager
    {
        private CertificateManager(string thumbprint)
        {
            Thumbprint = thumbprint;
        }

        public string Thumbprint { get; private set; }

        public X509Certificate2 Certificate { get; private set; }

        public static CertificateManager CreateCertificateManager(string thumbprint)
        {
            var certificateManager = new CertificateManager(thumbprint)
            {
                Certificate = RetrieveCertificateFromStore(thumbprint)
            };

            return certificateManager;
        }

        /// <summary>
        /// Gets the certificate matching the thumbprint from the local store.
        /// Throws an ArgumentException if a matching certificate is not found.
        /// </summary>
        /// <param name="thumbprint">The thumbprint of the certificate to find.</param>
        /// <returns>The certificate with the specified thumbprint.</returns>
        private static X509Certificate2 RetrieveCertificateFromStore(string thumbprint)
        {
            var locations = new List<StoreLocation> 
                { 
                    StoreLocation.CurrentUser, 
                    StoreLocation.LocalMachine 
                };

            foreach (var store in locations.Select(location => new X509Store("My", location)))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                    if (certificates.Count == 1)
                    {
                        return certificates[0];
                    }
                }
                finally
                {
                    store.Close();
                }
            }

            throw new ArgumentException(string.Format("A Certificate with Thumbprint '{0}' could not be located.", thumbprint));
        }
    }
}
