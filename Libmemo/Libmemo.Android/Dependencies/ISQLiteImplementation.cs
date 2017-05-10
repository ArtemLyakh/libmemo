using System;
using Libmemo.Droid;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(ISQLiteImplementation))]
namespace Libmemo.Droid {
    public class ISQLiteImplementation : ISQLite {
        public ISQLiteImplementation() { }
        public string GetDatabasePath(string sqliteFilename) {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(documentsPath, sqliteFilename);
            return path;
        }
    }
}