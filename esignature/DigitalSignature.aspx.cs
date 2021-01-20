using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using System.IO; // Stream
using Org.BouncyCastle.Pkcs; // Pkcs12Store
using Org.BouncyCastle.X509; // X509Certificate

namespace esignature {
    public partial class DigitalSignature : System.Web.UI.Page {

        public static void SignPdfFile ( string sourceDocument, string destinationPath, Stream privateKeyStream, string password, string reason, string location ) {
            Pkcs12Store pk12 = new Pkcs12Store( privateKeyStream, password.ToCharArray() );
            privateKeyStream.Dispose();
            string alias = null;
            foreach ( string tAlias in pk12.Aliases ) {
                if ( pk12.IsKeyEntry( tAlias ) ) {
                    alias = tAlias;
                    break;
                }
            }
            var pk = pk12.GetKey( alias ).Key;
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader( sourceDocument );
            using ( FileStream fout = new FileStream( destinationPath, FileMode.Create, FileAccess.ReadWrite ) ) {
                using ( iTextSharp.text.pdf.PdfStamper stamper = iTextSharp.text.pdf.PdfStamper.CreateSignature( reader, fout, '\0' ) ) {
                    iTextSharp.text.pdf.PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont( System.Web.HttpContext.Current.Server.MapPath( "~/fonts/iconic/fonts/Material-Design-Iconic-Font.ttf" ), iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED );
                    iTextSharp.text.Font font = new iTextSharp.text.Font( bf, 11 );
                    appearance.Layer2Font = font;
                    //appearance.Image = new iTextSharp.text.pdf.PdfImage();
                    appearance.Reason = reason;
                    appearance.Location = location;
                    appearance.SetVisibleSignature( new iTextSharp.text.Rectangle( 20, 10, 170, 60 ), 1, "Icsi-Vendor" );
                    iTextSharp.text.pdf.security.IExternalSignature es = new iTextSharp.text.pdf.security.PrivateKeySignature( pk, "SHA-256" );
                    iTextSharp.text.pdf.security.MakeSignature.SignDetached( appearance, es, new X509Certificate[] { pk12.GetCertificate( alias ).Certificate }, null, null, null, 0, iTextSharp.text.pdf.security.CryptoStandard.CMS );
                    stamper.Close();
                }
            }
        }







        protected void Page_Load ( object sender, EventArgs e ) {
            //string sourceDocument = filePath;
            string sourceDocument = Server.MapPath( "~/files/Document.pdf" );
            //string destinationPath = filePath.Replace( ".pdf", "_signed.pdf" );
            string destinationPath = sourceDocument.Replace( ".pdf", "_signed.pdf" );
            //Stream stream = File.OpenRead( HttpContext.Current.Server.MapPath( $"~/bin/Signatures/{fileName}" ) );
            Stream stream = File.OpenRead( Server.MapPath( "~/files/lockers.pfx" ) );
            //SignPdfFile( sourceDocument, destinationPath, stream, invoice.Password, reason, location );
            SignPdfFile( sourceDocument, destinationPath, stream, "123456", "Digital Solutions", "Ecuador" ); // Clave Checha
        }
    }
}