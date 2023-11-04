using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using SAaP.Core.Contracts.Services;

namespace SAaP.Core.Services.Generic;

public class FileService : IFileService
{
	public T Read<T>(string folderPath, string fileName)
	{
		var path = Path.Combine(folderPath, fileName);

		if (!File.Exists(path)) return default;

		var json = File.ReadAllText(path);

		return JsonConvert.DeserializeObject<T>(json);
	}

	public void Save<T>(string folderPath, string fileName, T content)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);

		var fileContent = JsonConvert.SerializeObject(content);
		File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
	}

	public async Task<string> ReadAsync(string folderPath, string fileName)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);
		try
		{
			var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
			var file   = await folder.TryGetItemAsync(fileName) as StorageFile;

			if (file != null) return await File.ReadAllTextAsync(file.Path);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
		}

		return string.Empty;
	}

	public async Task SaveAsync<T>(string folderPath, string fileName, T content)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);
		try
		{
			var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
			var file   = await folder.TryGetItemAsync(fileName) as StorageFile;

			if (file != null)
			{
				var fileContent = JsonConvert.SerializeObject(content, new JsonSerializerSettings
				                                                       {
					                                                       ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				                                                       });
				await File.WriteAllTextAsync(file.Path, fileContent, Encoding.UTF8);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
		}
	}

	public void Delete(string folderPath, string fileName)
	{
		if (fileName != null && File.Exists(Path.Combine(folderPath, fileName))) File.Delete(Path.Combine(folderPath, fileName));
	}

	public void Append<T>([NotNull] string folderPath, string fileName, T content)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);

		var fileContent = JsonConvert.SerializeObject(content);
		File.AppendAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
	}

	public async Task AppendAsync<T>([NotNull] string folderPath, string fileName, T content)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);

		var fileContent = JsonConvert.SerializeObject(content);
		await File.AppendAllTextAsync(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
	}

	public async Task AppendAsync<T>([NotNull] string folderPath, string fileName, IEnumerable<T> content)
	{
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath!);

		var fileContent = JsonConvert.SerializeObject(content);
		await File.AppendAllTextAsync(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
	}
}