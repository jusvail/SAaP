using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using ABI.Windows.Devices.AllJoyn;

namespace SAaP.Core.Services;

public static class StartupService
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

    private static async Task EnsureDbFileExist(StorageFolder top, string name)
    {
        var file = await top.TryGetItemAsync(name) as StorageFile;

        if (file == null)
        {
            await top.CreateFileAsync(name);
            // Initialize Database
            await DbService.InitializeDatabase();
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

    public static async Task InitializeWorkSpaceFolderTreeAsync()
    {
        var localAppDataFolder = await StorageFolder.GetFolderFromPathAsync(LocalApplicationData);

        var workSpace = await EnsureFolderExist(localAppDataFolder, WorkFolder);

        await workSpace.DeleteAsync();

        await EnsureWorkSpaceFolderTreeIntegrityAsync();
    }
}