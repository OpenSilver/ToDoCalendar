using OpenSilver;
using System.IO;
using System.IO.IsolatedStorage;

namespace ToDoCalendarControl
{
    internal static class FileSystemHelpers
    {
        public static void WriteTextToFile(string fileName, string fileContent)
        {
            if (Interop.IsRunningInTheSimulator)
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
            else // IsolatedStorage is not supported on Browser
            {
                Interop.ExecuteJavaScriptVoid($"localStorage.setItem('{fileName}', `{fileContent}`)");
            }
        }

        public static string ReadTextFromFile(string fileName)
        {
            if (Interop.IsRunningInTheSimulator)
            {
                using IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists(fileName))
                {
                    using IsolatedStorageFileStream fs = storage.OpenFile(fileName, FileMode.Open);
                    if (fs != null)
                    {
                        using StreamReader sr = new(fs);
                        return sr.ReadToEnd();
                    }
                }
            }
            else // IsolatedStorage is not supported on Browser
            {
                return Interop.ExecuteJavaScriptGetResult<string>($"localStorage.getItem('{fileName}')");
            }
            return null;
        }
    }
}
