using SAaP.Contracts.Services;
using SAaP.Core.Services;
using Windows.Storage;
using SAaP.Core.Helpers;

namespace SAaP.Services
{
    internal class Logger : ILogger
    {
        private const string FileFormat = "log-{0}.txt";

        private static readonly ReaderWriterLockSlim LogWriteLock = new();

        public static string LogFilePath { get; set; }

        private const int MaxCommittedCount = 10;

        private static int _uncommittedCount;

        private static readonly string[] Messages = new string[MaxCommittedCount];

        public async Task Log(string message)
        {
            try
            {
                LogWriteLock.EnterWriteLock();
                //
                // if (_uncommittedCount < MaxCommittedCount)
                // {
                //     Messages[_uncommittedCount++] = message;
                //     return;
                // }

                await File.AppendAllTextAsync(LogFilePath, Time.GetTimeRightNow() + ": " + message + Environment.NewLine);
                // await File.AppendAllLinesAsync(LogFilePath, Messages);
                //_uncommittedCount = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }

        public async Task Log(List<string> message)
        {
            try
            {
                LogWriteLock.EnterWriteLock();

                for (var i = 0; i < message.Count; i++)
                {
                    message[i] = Time.GetTimeRightNow() + ": " + message[i] + Environment.NewLine;
                }

                await File.AppendAllLinesAsync(LogFilePath, message);
                // await File.AppendAllLinesAsync(LogFilePath, Messages);
                //_uncommittedCount = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }

        public static async void EnsuredLogEnv()
        {
            var logFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.LogPath);

            var td = Time.GetTimeRightNow().ToString("yyyy-MM-dd");

            var fileName = string.Format(FileFormat, td);

            var logFile = await logFolder.TryGetItemAsync(fileName) as StorageFile;

            if (logFile == null)
            {
                var file = await logFolder.CreateFileAsync(fileName);

                LogFilePath = file.Path;
            }
            else
            {
                LogFilePath = logFile.Path;
            }
        }
    }
}
