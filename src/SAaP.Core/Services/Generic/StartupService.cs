using SAaP.Core.Services.Analyst;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SAaP.Core.Services.Generic;

public static class StartupService
{
	public const string TaskFile = "tasks.txt";
	private const string DbName = "saap.db";
	private const string WorkFolder = "saap";
	private const string WorkFolderSubDbContainer = "db";
	private const string WorkFolderSubPyData = "pydata";
	private const string WorkFolderSubLogData = "log";
	private const string WorkFolderSubFilter = "filter";
	private const string WorkFolderSubMinData = "mindata";

	private static readonly string LocalApplicationData = ApplicationData.Current.LocalFolder.Path;

	private static string WorkSpacePath => Path.Combine(LocalApplicationData, WorkFolder);

	public static string DbPath => Path.Combine(WorkSpacePath, WorkFolderSubDbContainer);
	public static string DbFilePath => Path.Combine(DbPath, DbName);
	public static string DbConnectionString => "Data Source=" + DbFilePath + ";Version=3;";

	public static string PyDataPath => Path.Combine(WorkSpacePath, WorkFolderSubPyData);
	public static string LogPath => Path.Combine(WorkSpacePath, WorkFolderSubLogData);
	public static string MinDataPath => Path.Combine(WorkSpacePath, WorkFolderSubMinData);
	public static string FilterPath => Path.Combine(WorkSpacePath, WorkFolderSubFilter);

	private static async Task<StorageFolder> EnsureFolderExistAsync(StorageFolder top, string name)
	{
		var folder = await top.TryGetItemAsync(name) as StorageFolder;

		return folder ?? await top.CreateFolderAsync(name);
	}

	private static async Task EnsureDbFileExistAsync(StorageFolder top, string name)
	{
		await EnsureFileExistAsync(top, name);
		try
		{
			// Initialize Database
			await DbService.InitializeDatabase();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(nameof(StartupService) + nameof(EnsureDbFileExistAsync));
		}
	}

	public static async Task EnsureFileExistAsync(StorageFolder top, string name)
	{
		var file = await top.TryGetItemAsync(name) as StorageFile;

		if (file == null) await top.CreateFileAsync(name);
	}

	public static async Task EnsureWorkSpaceFolderTreeIntegrityAsync()
	{
		var localAppDataFolder = await StorageFolder.GetFolderFromPathAsync(LocalApplicationData);

		var workSpace = await EnsureFolderExistAsync(localAppDataFolder, WorkFolder);

		await EnsureFolderExistAsync(workSpace, WorkFolderSubPyData);
		await EnsureFolderExistAsync(workSpace, WorkFolderSubLogData);
		await EnsureFolderExistAsync(workSpace, WorkFolderSubMinData);
		await EnsureFolderExistAsync(workSpace, WorkFolderSubFilter);

		var dbFolder = await EnsureFolderExistAsync(workSpace, WorkFolderSubDbContainer);
		var tkFolder = await EnsureFolderExistAsync(workSpace, WorkFolderSubFilter);

		await EnsureDbFileExistAsync(dbFolder, DbName);
		await EnsureFileExistAsync(tkFolder, TaskFile);
	}

	public static async Task InitializeWorkSpaceFolderTreeAsync()
	{
		var localAppDataFolder = await StorageFolder.GetFolderFromPathAsync(LocalApplicationData);

		var workSpace = await EnsureFolderExistAsync(localAppDataFolder, WorkFolder);

		await workSpace.DeleteAsync();

		await EnsureWorkSpaceFolderTreeIntegrityAsync();
	}
}