using System;

namespace ToDoCalendarControl.Services;

public interface IKeyboardService
{
    event Action<bool> KeyboardStateChanged;
}
