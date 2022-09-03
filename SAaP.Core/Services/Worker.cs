using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;
using Windows.Web.AtomPub;

namespace SAaP.Core.Services
{
    public class Worker
    {
        private static readonly string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static readonly string dbName = "saap.db";
        public static readonly string workFolder = "saap";
        public static readonly string workFolderSubDbContainer = "db";
        public static readonly string workFolderSubPyData = "pydata";

        public static string WorkSpacePath => Path.Combine(LocalApplicationData, workFolder);

        public static string DbPath => Path.Combine(WorkSpacePath, workFolderSubDbContainer);
        public static string DbFilePath => Path.Combine(DbPath, dbName);
        public static string DbConnectionString => "Data Source=" + DbFilePath + ";Version=3;";

        public static string PyDataPath => Path.Combine(WorkSpacePath, workFolderSubPyData);

        private static async Task<StorageFolder> LocalAppDataFolder()
        {
            return await StorageFolder.GetFolderFromPathAsync(LocalApplicationData);
        }

        public static async Task<StorageFolder> WorkSpace()
        {
            return await StorageFolder.GetFolderFromPathAsync(Path.Combine(LocalApplicationData, workFolder));
        }

        public static async Task<StorageFolder> EnsureFolderExist(StorageFolder top, string name)
        {
            var folder = await top.TryGetItemAsync(name) as StorageFolder;

            return folder ?? await top.CreateFolderAsync(name);
        }

        public static async Task<StorageFile> EnsureFileExist(StorageFolder top, string name)
        {
            var file = await top.TryGetItemAsync(name) as StorageFile;

            return file ?? await top.CreateFileAsync(name);
        }

        private static async Task EnsureDBFileExist(StorageFolder top, string name)
        {
            var file = await top.TryGetItemAsync(name) as StorageFile;

            if (file == null)
            {
                await top.CreateFileAsync(name);
                // Initialize Database
                await DataAccess.InitializeDatabase();
            }
        }

        public static async Task EnsureWorkSpaceFolderTreeIntegrityAsync()
        {
            var localAppDataFolder = await LocalAppDataFolder();

            var workSpace = await EnsureFolderExist(localAppDataFolder, workFolder);

            await EnsureFolderExist(workSpace, workFolderSubPyData);

            var dbFolder = await EnsureFolderExist(workSpace, workFolderSubDbContainer);

            await EnsureDBFileExist(dbFolder, dbName);
        }
    }
}
