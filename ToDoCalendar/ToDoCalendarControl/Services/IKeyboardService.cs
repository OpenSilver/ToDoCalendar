using System;

namespace ToDoCalendarControl.Services;

public interface IKeyboardService
{
    bool IsKeyboardVisible { get; }
    event Action<bool> KeyboardStateChanged;
}
