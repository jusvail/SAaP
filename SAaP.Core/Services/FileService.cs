using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI.Windows.Storage;
using StorageFile = Windows.Storage.StorageFile;
using StorageFolder = Windows.Storage.StorageFolder;

namespace SAaP.Core.Services
{
    public class FileService
    {

        public static async Task<StorageFile> TryGetItemAsync(StorageFolder top, string name)
            => await top.TryGetItemAsync(name) as StorageFile;

    }
}
