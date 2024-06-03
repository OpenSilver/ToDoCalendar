using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    static class FileSystemHelpers
    {
        public static void WriteTextToFile(string fileName, string fileContent)
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

        public static string ReadTextFromFile(string fileName)
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
            return null;
        }
    }
}
