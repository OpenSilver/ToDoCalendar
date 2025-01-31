using System;

namespace ToDoCalendarControl
{
    static class LoadAndSaveHelpers
    {
        // Note: SAVE is done in the AutoSaveHandler class.

        public static bool TryLoadModelFromFileSystem(out Model model)
        {
            // Load the AutoSave from the disk:
            var serializedModel = FileSystemHelpers.ReadTextFromFile(AutoSaveHandler.FileNameForAutoSave);
            model = null;

            if (serializedModel != null)
            {
                // Deserialize the Model:
                try
                {
                    model = LoadModelFromTextAndRaiseExceptionIfError(serializedModel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return model != null;
        }

        public static Model LoadModelFromTextAndRaiseExceptionIfError(string serializedText)
        {
            return SerializationHelpers.Deserialize<Model>(serializedText);
        }
    }
}
