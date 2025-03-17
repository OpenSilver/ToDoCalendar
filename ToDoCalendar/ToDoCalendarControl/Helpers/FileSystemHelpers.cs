using System.IO;
using System.IO.IsolatedStorage;

namespace ToDoCalendarControl
{
    internal static class FileSystemHelpers
    {
        public static void WriteTextToFile(string fileName, string fileContent)
        {
#if OPENSILVER
            if (OpenSilver.Interop.IsRunningInTheSimulator)
            {
                Save(fileName, fileContent);
            }
            else // IsolatedStorage is not supported on Browser
            {
                OpenSilver.Interop.ExecuteJavaScriptVoid($"localStorage.setItem('{fileName}', `{fileContent}`)");
            }
#elif WINDOWS
            Save(fileName, fileContent);
#endif
        }

        private static void Save(string fileName, string fileContent)
        {
            using IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream fs = null;
            using (fs = storage.CreateFile(fileName))
            {
                if (fs != null)
                {
                    using StreamWriter sw = new(fs);
                    sw.Write(fileContent);
                }
            }
        }

        public static string ReadTextFromFile(string fileName)
        {
#if OPENSILVER
            if (OpenSilver.Interop.IsRunningInTheSimulator)
            {
                return Read(fileName);
            }
            else // IsolatedStorage is not supported on Browser
            {
                return OpenSilver.Interop.ExecuteJavaScriptGetResult<string>($"localStorage.getItem('{fileName}')");
            }
#elif WINDOWS
            return Read(fileName);
#endif
        }

        private static string Read(string fileName)
        {
            using var storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (storage.FileExists(fileName))
            {
                using IsolatedStorageFileStream fs = storage.OpenFile(fileName, FileMode.Open);
                if (fs != null)
                {
                    using StreamReader sr = new(fs);
                    return sr.ReadToEnd();
                }
            }
            return null;
        }
    }
}
