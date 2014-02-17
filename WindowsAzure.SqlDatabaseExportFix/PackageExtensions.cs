using System;
using System.IO;
using System.IO.Packaging;

namespace WindowsAzure.SqlDatabaseExportFix
{
    public static class PackageExtensions
    {
        public static Stream ReadStream(this Package package, string name)
        {
            var packagePart = package.GetPart(new Uri("/" + name, UriKind.Relative));
            return packagePart.GetStream();
        }

        public static string Read(this Package package, string name)
        {
            var packageContent = "";

            // Read package.
            using (var stream = package.ReadStream(name))
            using (var reader = new StreamReader(stream))
                packageContent = reader.ReadToEnd();

            // Done.
            return packageContent;
        }

        public static void Write(this Package package, string name, string content)
        {
            var packagePart = package.GetPart(new Uri("/" + name, UriKind.Relative));

            // Read package.
            using (var stream = packagePart.GetStream(FileMode.Create))
            using (var writer = new StreamWriter(stream))
                writer.Write(content);
        }
    }
}