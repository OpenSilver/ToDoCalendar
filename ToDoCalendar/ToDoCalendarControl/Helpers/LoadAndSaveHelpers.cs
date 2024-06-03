using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    static class LoadAndSaveHelpers
    {
        // Note: SAVE is done in the AutoSaveHandler class.

        public static bool TryLoadModelFromFileSystem(out Model model)
        {
#if !OPENSILVER
            // Load the AutoSave from the disk:
            var serializedModel = FileSystemHelpers.ReadTextFromFile(AutoSaveHandler.FileNameForAutoSave);

            if (serializedModel != null)
            {
                // Deserialize the Model:
                model = LoadModelFromTextAndRaiseExceptionIfError(serializedModel);
                return true;
            }
            else
            {
                model = null;
                return false;
            }
#else
            model = null;
            return false;
#endif
        }

#if !OPENSILVER
        public static Model LoadModelFromTextAndRaiseExceptionIfError(string serializedText)
        {
            return SerializationHelpers.Deserialize<Model>(serializedText);
        }
#endif
    }
}
