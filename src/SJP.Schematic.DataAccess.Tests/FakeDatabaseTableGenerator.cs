using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    public class FakeDatabaseTableGenerator : DatabaseTableGenerator
    {
        public FakeDatabaseTableGenerator(INameProvider nameProvider)
            : base(nameProvider)
        {
        }

        public override string Generate(IRelationalDatabaseTable table) => string.Empty;

        public FileInfo InnerGetFilePath(DirectoryInfo baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
