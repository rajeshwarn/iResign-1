using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ClrPlus.Windows.Api;

namespace iResign
{
    public static class SignFile
    {
        public static void SignFileFromDisk(string filePath)
        {
            SignFileFromDisk(new FileInfo(filePath));
        }

        public static void SignFileFromDisk(FileInfo filePath)
        {
            X509Certificate2 signingCertificate = GetCodeSigningCertificate();

            if (signingCertificate == null)
            {
                throw new SecurityException("No signing certificate found");
            }

            const DigitalSignFlags flags = DigitalSignFlags.NoUI;
            DigitalSignInfo dsi = new DigitalSignInfo();
            IntPtr certificateHandle = signingCertificate.Handle;

            try
            {
                dsi.pwszFileName = filePath.FullName;
                dsi.dwSigningCertChoice = DigitalSigningCertificateChoice.Certificate;
                dsi.dwAdditionalCertChoice = DigitalSignAdditionalCertificateChoice.AddChainNoRoot;
                dsi.dwSubjectChoice = DigitalSignSubjectChoice.File;
                dsi.pwszTimestampURL = null;
                dsi.pSignExtInfo = IntPtr.Zero;
                dsi.pSigningCertContext = certificateHandle;
                dsi.dwSize = Marshal.SizeOf(dsi);
                bool result = CryptUi.CryptUIWizDigitalSign(flags, IntPtr.Zero, "", ref dsi, ref dsi.pSigningCertContext);

                if (!result)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            finally
            {
                //CryptUi.CryptUIWizFreeDigitalSignContext(dsi.pSigningCertContext); 
                //- currently throws, from my reading of http://msdn.microsoft.com/en-us/library/windows/desktop/aa380292%28v=vs.85%29.aspx
                //I'm not doing any damage by not freeing it.
            }
        }

        private static X509Certificate2 GetCodeSigningCertificate()
        {
            X509Store store = new X509Store("My");

            store.Open(OpenFlags.ReadOnly);

            var certs = store.Certificates.Cast<X509Certificate2>().Where(x => x.HasCodeSigningOID()).ToList();

            return certs.FirstOrDefault();
        }

        public static bool HasCodeSigningOID(this X509Certificate2 cert)
        {
            var xyz = cert.Extensions.OfType<X509Extension>();
            var zzz = cert as X509Certificate;

            var enhancedExtensions =
            cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().Select(x => x.EnhancedKeyUsages).FirstOrDefault();

            if (enhancedExtensions != null)
            {
                var oids = enhancedExtensions.OfType<Oid>();
                return oids.Where(IsCodeSigningOID).ToList().Any();
            }

            return false;
        }

        private static bool IsCodeSigningOID(Oid oid)
        {
            return oid.FriendlyName == "Code Signing";
        }

    }
}
