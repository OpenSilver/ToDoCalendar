using OpenSilver;
using System.IO;
using System.IO.IsolatedStorage;

namespace ToDoCalendarControl
{
    static class FileSystemHelpers
    {
        public static void WriteTextToFile(string fileName, string fileContent)
        {
            if (Interop.IsRunningInTheSimulator)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    IsolatedStorageFileStream fs = null;
                    using (fs = storage.CreateFile(fileName))
                    {
                        if (fs != null)
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                sw.Write(fileContent);
                            }
                            //byte[] bytes = System.BitConverter.GetBytes(number);
                            //fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
            else // IsolatedStorage is not supported on Browser
            {
                Interop.ExecuteJavaScript($"localStorage.setItem('{fileName}', '{fileContent}')");
            }
        }

        public static string ReadTextFromFile(string fileName)
        {
            if (Interop.IsRunningInTheSimulator)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists(fileName))
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile(fileName, System.IO.FileMode.Open))
                        {
                            if (fs != null)
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    return sr.ReadToEnd();
                                }

                                //byte[] saveBytes = new byte[4];
                                //int count = fs.Read(saveBytes, 0, 4);
                                //if (count > 0)
                                //{
                                //    number = System.BitConverter.ToInt32(saveBytes, 0);
                                //}
                            }
                        }
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
