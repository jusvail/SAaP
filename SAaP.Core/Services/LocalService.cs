using Microsoft.UI.Xaml.Controls;
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
    public static class LocalService
    {
        private static readonly string LocalApplicationData = ApplicationData.Current.LocalFolder.Path;

        private const string DbName = "saap.db";
        private const string WorkFolder = "saap";
        private const string WorkFolderSubDbContainer = "db";
        private const string WorkFolderSubPyData = "pydata";

        private static string WorkSpacePath => Path.Combine(LocalApplicationData, WorkFolder);

        public static string DbPath => Path.Combine(WorkSpacePath, WorkFolderSubDbContainer);
        public static string DbFilePath => Path.Combine(DbPath, DbName);
        public static string DbConnectionString => "Data Source=" + DbFilePath + ";Version=3;";

        public static string PyDataPath => Path.Combine(WorkSpacePath, WorkFolderSubPyData);

        private static async Task<StorageFolder> EnsureFolderExist(StorageFolder top, string name)
        {
            var folder = await top.TryGetItemAsync(name) as StorageFolder;

            return folder ?? await top.CreateFolderAsync(name);
        }

        public static async Task<StorageFile> EnsureFileExist(StorageFolder top, string name)
        {
            var file = await top.TryGetItemAsync(name) as StorageFile;

            return file ?? await top.CreateFileAsync(name);
        }

        private static async Task EnsureDbFileExist(StorageFolder top, string name)
        {
            var file = await top.TryGetItemAsync(name) as StorageFile;

            if (file == null)
            {
                await top.CreateFileAsync(name);
                // Initialize Database
                await DbAccess.InitializeDatabase();
            }
        }

        public static async Task EnsureWorkSpaceFolderTreeIntegrityAsync()
        {
            var localAppDataFolder = await StorageFolder.GetFolderFromPathAsync(LocalApplicationData);

            var workSpace = await EnsureFolderExist(localAppDataFolder, WorkFolder);

            await EnsureFolderExist(workSpace, WorkFolderSubPyData);

            var dbFolder = await EnsureFolderExist(workSpace, WorkFolderSubDbContainer);

            await EnsureDbFileExist(dbFolder, DbName);
        }
    }
}
