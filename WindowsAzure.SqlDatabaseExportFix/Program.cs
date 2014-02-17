using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WindowsAzure.SqlDatabaseExportFix
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Header.
            Console.WriteLine("");
            Console.WriteLine(" Windows Azure SQL Database BACPAC Fix v1.0 (2014-02-17) - Sandrino Di Mattia");
            Console.WriteLine(" Usage: bacpacfix.exe <filePath.bacpac>");
            Console.WriteLine("");

            // Validate args.
            if (args.Length != 1)
            {
                Console.WriteLine(" Invalid number of arguments.");
                Console.WriteLine("");
                return;
            }

            // Get the path.
            var path = args[0];
            Console.WriteLine(" Processing: {0}", path);
            Console.WriteLine(" Note: This can take up to a few minutes.");
            Console.WriteLine("");

            using (var archive = Package.Open(path))
            {
                Console.WriteLine(" > Updating model.xml");

                // Update the model.
                var modelContent = archive.Read("model.xml");
                modelContent = modelContent.Replace("get_new_rowversion()", "0");
                archive.Write("model.xml", modelContent);

                // Log.
                Console.WriteLine(" > Model.xml updated.");

                // Calculate the checksum
                var sha256 = new SHA256Managed();
                byte[] hash = sha256.ComputeHash(archive.ReadStream("model.xml"));
                var outputString = new StringBuilder();
                foreach (var b in hash)
                    outputString.AppendFormat("{0:x2}", b);
                var checksum = outputString.ToString().ToUpper();

                // Log.
                Console.WriteLine(" > Checksum: {0}", checksum);
                Console.WriteLine(" > Updating Origin.xml with checksum...");

                // Update checksum.
                var origin = archive.Read("Origin.xml");
                var originDocument = XDocument.Parse(origin);
                var checksumElement = originDocument
                    .Root
                    .Elements(XName.Get("Checksums", "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"))
                    .Elements(XName.Get("Checksum", "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"))
                    .FirstOrDefault(e => e.HasAttributes && e.Attribute("Uri") != null && e.Attribute("Uri").Value == "/model.xml");
                checksumElement.Value = checksum;
                
                // Save origin.
                archive.Write("Origin.xml", originDocument.ToString());

                // Log.
                Console.WriteLine(" > Origin.xml updated.");
            }

            // Done.
            Console.WriteLine("");
            Console.WriteLine(" BACPAC fixed, please retry the 'Import Data-tier application' wizard.");
            Console.ReadLine();
        }
    }
}