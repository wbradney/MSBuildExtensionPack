//-----------------------------------------------------------------------
// <copyright file="Certificate.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Security
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using MSBuild.ExtensionPack.Security.Extended;

    internal enum CryptGetProvParamType
    {
        /// <summary>
        /// PP_ENUMALGS
        /// </summary>
        PP_ENUMALGS = 1,

        /// <summary>
        /// PP_ENUMCONTAINERS
        /// </summary>
        PP_ENUMCONTAINERS = 2,

        /// <summary>
        /// PP_IMPTYPE
        /// </summary>
        PP_IMPTYPE = 3,

        /// <summary>
        /// PP_NAME
        /// </summary>
        PP_NAME = 4,

        /// <summary>
        /// PP_VERSION
        /// </summary>
        PP_VERSION = 5,

        /// <summary>
        /// PP_CONTAINER
        /// </summary>
        PP_CONTAINER = 6,

        /// <summary>
        /// PP_CHANGE_PASSWORD
        /// </summary>
        PP_CHANGE_PASSWORD = 7,

        /// <summary>
        /// PP_KEYSET_SEC_DESCR
        /// </summary>
        PP_KEYSET_SEC_DESCR = 8,

        /// <summary>
        /// PP_CERTCHAIN
        /// </summary>
        PP_CERTCHAIN = 9,

        /// <summary>
        /// PP_KEY_TYPE_SUBTYPE
        /// </summary>
        PP_KEY_TYPE_SUBTYPE = 10,

        /// <summary>
        /// PP_PROVTYPE
        /// </summary>
        PP_PROVTYPE = 16,

        /// <summary>
        /// PP_KEYSTORAGE
        /// </summary>
        PP_KEYSTORAGE = 17,

        /// <summary>
        /// PP_APPLI_CERT
        /// </summary>
        PP_APPLI_CERT = 18,

        /// <summary>
        /// PP_SYM_KEYSIZE
        /// </summary>
        PP_SYM_KEYSIZE = 19,

        /// <summary>
        /// PP_SESSION_KEYSIZE
        /// </summary>
        PP_SESSION_KEYSIZE = 20,

        /// <summary>
        /// PP_UI_PROMPT
        /// </summary>
        PP_UI_PROMPT = 21,

        /// <summary>
        /// PP_ENUMALGS_EX
        /// </summary>
        PP_ENUMALGS_EX = 22,

        /// <summary>
        /// PP_ENUMMANDROOTS
        /// </summary>
        PP_ENUMMANDROOTS = 25,

        /// <summary>
        /// PP_ENUMELECTROOTS
        /// </summary>
        PP_ENUMELECTROOTS = 26,

        /// <summary>
        /// PP_KEYSET_TYPE
        /// </summary>
        PP_KEYSET_TYPE = 27,

        /// <summary>
        /// PP_ADMIN_PIN
        /// </summary>
        PP_ADMIN_PIN = 31,

        /// <summary>
        /// PP_KEYEXCHANGE_PIN
        /// </summary>
        PP_KEYEXCHANGE_PIN = 32,

        /// <summary>
        /// PP_SIGNATURE_PIN
        /// </summary>
        PP_SIGNATURE_PIN = 33,

        /// <summary>
        /// PP_SIG_KEYSIZE_INC
        /// </summary>
        PP_SIG_KEYSIZE_INC = 34,

        /// <summary>
        /// PP_KEYX_KEYSIZE_INC
        /// </summary>
        PP_KEYX_KEYSIZE_INC = 35,

        /// <summary>
        /// PP_UNIQUE_CONTAINER
        /// </summary>
        PP_UNIQUE_CONTAINER = 36,

        /// <summary>
        /// PP_SGC_INFO
        /// </summary>
        PP_SGC_INFO = 37,

        /// <summary>
        /// PP_USE_HARDWARE_RNG
        /// </summary>
        PP_USE_HARDWARE_RNG = 38,

        /// <summary>
        /// PP_KEYSPEC
        /// </summary>
        PP_KEYSPEC = 39,

        /// <summary>
        /// PP_ENUMEX_SIGNING_PROT
        /// </summary>
        PP_ENUMEX_SIGNING_PROT = 40,

        /// <summary>
        /// PP_CRYPT_COUNT_KEY_USE
        /// </summary>
        PP_CRYPT_COUNT_KEY_USE = 41,
    }

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Add</i> (<b>Required: </b>FileName <b>Optional: </b>MachineStore, CertPassword, Exportable, StoreName  <b>Output: </b>Thumbprint, SubjectDName)</para>
    /// <para><i>GetInfo</i> (<b>Required: </b> Thumbprint or SubjectDName <b> Optional:</b> MachineStore, StoreName <b>Output:</b> CertInfo)</para>
    /// <para><i>Remove</i> (<b>Required: </b>Thumbprint or SubjectDName <b>Optional: </b>MachineStore, StoreName)</para>
    /// <para><b>Remote Execution Support:</b> No</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="4.0" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <Target Name="Default">
    ///         <!-- Add a certificate -->
    ///         <MSBuild.ExtensionPack.Security.Certificate TaskAction="Add" FileName="C:\MyCertificate.cer" CertPassword="PASSW">
    ///             <Output TaskParameter="Thumbprint" PropertyName="TPrint"/>
    ///             <Output TaskParameter="SubjectDName" PropertyName="SName"/>
    ///         </MSBuild.ExtensionPack.Security.Certificate>
    ///         <Message Text="Thumbprint: $(TPrint)"/>
    ///         <Message Text="SubjectName: $(SName)"/>
    ///         <!-- Get Certificate Information -->
    ///         <MSBuild.ExtensionPack.Security.Certificate TaskAction="GetInfo" SubjectDName="$(SName)">
    ///             <Output TaskParameter="CertInfo" ItemName="ICertInfo" />
    ///         </MSBuild.ExtensionPack.Security.Certificate>
    ///         <Message Text="SubjectName: %(ICertInfo.SubjectName)"/>
    ///         <Message Text="SubjectNameOidValue: %(ICertInfo.SubjectNameOidValue)"/>
    ///         <Message Text="SerialNumber: %(ICertInfo.SerialNumber)"/>
    ///         <Message Text="Archived: %(ICertInfo.Archived)"/>
    ///         <Message Text="NotBefore: %(ICertInfo.NotBefore)"/>
    ///         <Message Text="NotAfter: %(ICertInfo.NotAfter)"/>
    ///         <Message Text="PrivateKeyFileName: %(ICertInfo.PrivateKeyFileName)"/>
    ///         <Message Text="FriendlyName: %(ICertInfo.FriendlyName)"/>
    ///         <Message Text="HasPrivateKey: %(ICertInfo.HasPrivateKey)"/>
    ///         <Message Text="Thumbprint: %(ICertInfo.Thumbprint)"/>
    ///         <Message Text="Version: %(ICertInfo.Version)"/>
    ///         <Message Text="PrivateKeyFileName: %(ICertInfo.PrivateKeyFileName)"/>
    ///         <Message Text="SignatureAlgorithm: %(ICertInfo.SignatureAlgorithm)"/>
    ///         <Message Text="IssuerName: %(ICertInfo.IssuerName)"/>
    ///         <Message Text="PrivateKeyFileName: %(ICertInfo.PrivateKeyFileName)"/>
    ///          <!-- Remove a certificate -->
    ///         <MSBuild.ExtensionPack.Security.Certificate TaskAction="Remove" Thumbprint="$(TPrint)"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>    
    [HelpUrl("http://www.msbuildextensionpack.com/help/4.0.2.0/html/45763eac-8f14-417d-9b27-425161982ffe.htm")]
    public class Certificate : BaseTask
    {
        private const string AddTaskAction = "Add";
        private const string RemoveTaskAction = "Remove";
        private const string GetInfoTaskAction = "GetInfo";
        private string storeName = "MY";

        [DropdownValue(AddTaskAction)]
        [DropdownValue(RemoveTaskAction)]
        [DropdownValue(GetInfoTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets a value indicating whether to use the MachineStore. Default is false
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(RemoveTaskAction, false)]
        [TaskAction(GetInfoTaskAction, false)]
        public bool MachineStore { get; set; }

        /// <summary>
        /// Sets the password for the pfx file from which the certificate is to be imported, defaults to blank
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        public string CertPassword { get; set; }

        /// <summary>
        /// Sets a value indicating whether the certificate is exportable.
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        public bool Exportable { get; set; }

        /// <summary>
        /// The distinguished subject name of the certificate
        /// </summary>
        [Output]
        [TaskAction(GetInfoTaskAction, false)]
        public string SubjectDName { get; set; }

        /// <summary>
        /// Gets the thumbprint. Used to uniquely identify certificate in further tasks
        /// </summary>
        [Output]
        [TaskAction(AddTaskAction, false)]
        [TaskAction(RemoveTaskAction, true)]
        [TaskAction(GetInfoTaskAction, false)]
        public string Thumbprint { get; set; }

        /// <summary>
        /// Sets the name of the store. Defaults to MY
        /// <para/>
        /// AddressBook:          The store for other users<br />
        /// AuthRoot:             The store for third-party certificate authorities<br />
        /// CertificateAuthority: The store for intermediate certificate authorities<br />
        /// Disallowed:           The store for revoked certificates<br />
        /// My:                   The store for personal certificates<br />
        /// Root:                 The store for trusted root certificate authorities <br />
        /// TrustedPeople:        The store for directly trusted people and resources<br />
        /// TrustedPublisher:     The store for directly trusted publishers<br />
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(RemoveTaskAction, false)]
        [TaskAction(GetInfoTaskAction, false)]
        public string StoreName
        {
            get { return this.storeName; }
            set { this.storeName = value; }
        }

        /// <summary>
        /// Sets the name of the file.
        /// </summary>
        [TaskAction(AddTaskAction, true)]
        [Output]
        public ITaskItem FileName { get; set; }

        /// <summary>
        /// Gets the item which contains the Certificate information. The following Metadata is populated: SubjectName, SignatureAlgorithm, SubjectNameOidValue, SerialNumber, Archived, NotAfter, NotBefore, FriendlyName, HasPrivateKey, Thumbprint, Version, PrivateKeyFileName, IssuerName
        /// </summary>
        [Output]
        [TaskAction(AddTaskAction, false)]
        public ITaskItem CertInfo { get; protected set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            if (!this.TargetingLocalMachine())
            {
                return;
            }

            switch (this.TaskAction)
            {
                case AddTaskAction:
                    this.Add();
                    break;
                case RemoveTaskAction:
                    this.Remove();
                    break;
                case GetInfoTaskAction:
                    this.GetInfo();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        private static string GetKeyFileName(X509Certificate cert)
        {
            IntPtr hprovider = IntPtr.Zero; // CSP handle
            bool freeProvider = false; // Do we need to free the CSP ?
            const uint AcquireFlags = 0;
            int keyNumber = 0;
            string keyFileName = null;
            byte[] keyFileBytes;

            // Determine whether there is private key information available for this certificate in the key store
            if (NativeMethods.CryptAcquireCertificatePrivateKey(cert.Handle, AcquireFlags, IntPtr.Zero, ref hprovider, ref keyNumber, ref freeProvider))
            {
                IntPtr pbytes = IntPtr.Zero; // Native Memory for the CRYPT_KEY_PROV_INFO structure
                int cbbytes = 0; // Native Memory size
                try
                {
                    if (NativeMethods.CryptGetProvParam(hprovider, CryptGetProvParamType.PP_UNIQUE_CONTAINER, IntPtr.Zero, ref cbbytes, 0))
                    {
                        pbytes = Marshal.AllocHGlobal(cbbytes);

                        if (NativeMethods.CryptGetProvParam(hprovider, CryptGetProvParamType.PP_UNIQUE_CONTAINER, pbytes, ref cbbytes, 0))
                        {
                            keyFileBytes = new byte[cbbytes];

                            Marshal.Copy(pbytes, keyFileBytes, 0, cbbytes);

                            // Copy eveything except tailing null byte
                            keyFileName = System.Text.Encoding.ASCII.GetString(keyFileBytes, 0, keyFileBytes.Length - 1);
                        }
                    }
                }
                finally
                {
                    if (freeProvider)
                    {
                        NativeMethods.CryptReleaseContext(hprovider, 0);
                    }

                    // Free our native memory
                    if (pbytes != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pbytes);
                    }
                }
            }

            if (keyFileName == null)
            {
                return String.Empty;
            }

            return keyFileName;
        }

        private void GetInfo()
        {
            StoreLocation locationFlag = this.MachineStore ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
            X509Store store = new X509Store(this.StoreName, locationFlag);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            X509Certificate2 cert = null;

            if (!string.IsNullOrEmpty(this.Thumbprint))
            {
                var matches = store.Certificates.Find(X509FindType.FindByThumbprint, this.Thumbprint, false);
                if (matches.Count > 1)
                {
                    this.Log.LogError("More than one certificate with Thumbprint '{0}' found in the {1} store.", this.Thumbprint, this.StoreName);
                    return;
                }

                if (matches.Count == 0)
                {
                    this.Log.LogError("No certificates with Thumbprint '{0}' found in the {1} store.", this.Thumbprint, this.StoreName);
                    return;
                }

                cert = matches[0];
            }
            else if (!string.IsNullOrEmpty(this.SubjectDName))
            {
                var matches = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, this.SubjectDName, false);
                if (matches.Count > 1)
                {
                    this.Log.LogError("More than one certificate with SubjectDName '{0}' found in the {1} store.", this.SubjectDName, this.StoreName);
                    return;
                }

                if (matches.Count == 0)
                {
                    this.Log.LogError("No certificates with SubjectDName '{0}' found in the {1} store.", this.SubjectDName, this.StoreName);
                    return;
                }

                cert = matches[0];
            }

            this.CertInfo = new TaskItem("CertInfo");
            this.CertInfo.SetMetadata("SubjectName", cert.SubjectName.Name);
            this.CertInfo.SetMetadata("SubjectNameOidValue", cert.SubjectName.Oid.Value ?? string.Empty);
            this.CertInfo.SetMetadata("SerialNumber", cert.SerialNumber);
            this.CertInfo.SetMetadata("Archived", cert.Archived.ToString());
            this.CertInfo.SetMetadata("NotBefore", cert.NotBefore.ToString());
            this.CertInfo.SetMetadata("FriendlyName", cert.FriendlyName);
            this.CertInfo.SetMetadata("HasPrivateKey", cert.HasPrivateKey.ToString());
            this.CertInfo.SetMetadata("Thumbprint", cert.Thumbprint);
            this.CertInfo.SetMetadata("Version", cert.Version.ToString());
            this.CertInfo.SetMetadata("SignatureAlgorithm", cert.SignatureAlgorithm.FriendlyName);
            this.CertInfo.SetMetadata("IssuerName", cert.IssuerName.Name);
            this.CertInfo.SetMetadata("NotAfter", cert.NotAfter.ToString());

            var privateKeyFileName = GetKeyFileName(cert);
            if (!String.IsNullOrEmpty(privateKeyFileName))
            {
                // Adapted from the FindPrivateKey application.  See http://msdn.microsoft.com/en-us/library/aa717039(v=VS.90).aspx.
                var keyFileDirectory = this.GetKeyFileDirectory(privateKeyFileName);
                if (!String.IsNullOrEmpty(privateKeyFileName) && !String.IsNullOrEmpty(keyFileDirectory))
                {
                    this.CertInfo.SetMetadata("PrivateKeyFileName", Path.Combine(keyFileDirectory, privateKeyFileName));
                }
            }

            store.Close();
        }

        private void Remove()
        {
            StoreLocation locationFlag = this.MachineStore ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
            X509Store store = new X509Store(this.StoreName, locationFlag);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(this.Thumbprint))
            {
                var matches = store.Certificates.Find(X509FindType.FindByThumbprint, this.Thumbprint, false);
                if (matches.Count > 1)
                {
                    this.Log.LogError("More than one certificate with Thumbprint '{0}' found in the {1} store.", this.Thumbprint, this.StoreName);
                    return;
                }

                if (matches.Count == 0)
                {
                    this.Log.LogError("No certificates with Thumbprint '{0}' found in the {1} store.", this.Thumbprint, this.StoreName);
                    return;
                }

                cert = matches[0];
                this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Removing Certificate: {0}", cert.Thumbprint));
                store.Remove(cert);
            }
            else if (!string.IsNullOrEmpty(this.SubjectDName))
            {
                var matches = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, this.SubjectDName, false);
                if (matches.Count > 1)
                {
                    this.Log.LogError("More than one certificate with SubjectDName '{0}' found in the {1} store.", this.SubjectDName, this.StoreName);
                    return;
                }

                if (matches.Count == 0)
                {
                    this.Log.LogError("No certificates with SubjectDName '{0}' found in the {1} store.", this.SubjectDName, this.StoreName);
                    return;
                }

                cert = matches[0];
                this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Removing Certificate: {0}", cert.SubjectName));
                store.Remove(cert);
            }

            store.Close();
        }

        private void Add()
        {
            if (this.FileName == null)
            {
                this.Log.LogError("FileName not provided");
                return;
            }

            if (System.IO.File.Exists(this.FileName.GetMetadata("FullPath")) == false)
            {
                this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "FileName not found: {0}", this.FileName.GetMetadata("FullPath")));
                return;
            }

            X509Certificate2 cert = new X509Certificate2();
            X509KeyStorageFlags keyflags = this.MachineStore ? X509KeyStorageFlags.MachineKeySet : X509KeyStorageFlags.DefaultKeySet;
            if (this.Exportable)
            {
                keyflags |= X509KeyStorageFlags.Exportable;
            }

            keyflags |= X509KeyStorageFlags.PersistKeySet;
            cert.Import(this.FileName.GetMetadata("FullPath"), this.CertPassword, keyflags);
            StoreLocation locationFlag = this.MachineStore ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Adding Certificate: {0} to Store: {1}", this.FileName.GetMetadata("FullPath"), this.StoreName));
            X509Store store = new X509Store(this.StoreName, locationFlag);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
            this.Thumbprint = cert.Thumbprint;
            this.SubjectDName = cert.SubjectName.Name;
        }

        private string GetKeyFileDirectory(string keyFileName)
        {
            // Look up All User profile from environment variable
            string allUserProfile = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            // set up searching directory
            string machineKeyDir = allUserProfile + "\\Microsoft\\Crypto\\RSA\\MachineKeys";

            // Seach the key file
            string[] fs = System.IO.Directory.GetFiles(machineKeyDir, keyFileName);

            // If found
            if (fs.Length > 0)
            {
                return machineKeyDir;
            }

            // Next try current user profile
            string currentUserProfile = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // seach all sub directory
            string userKeyDir = currentUserProfile + "\\Microsoft\\Crypto\\RSA\\";

            fs = System.IO.Directory.GetDirectories(userKeyDir);
            if (fs.Length > 0)
            {
                // for each sub directory
                foreach (string keyDir in fs)
                {
                    fs = System.IO.Directory.GetFiles(keyDir, keyFileName);
                    if (fs.Length == 0)
                    {
                        continue;
                    }

                    return keyDir;
                }
            }

            this.Log.LogError("Unable to locate private key file directory");
            return String.Empty;
        }
    }
}
