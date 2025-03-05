
namespace ToDoCalendar.Photino.JavaScript
{
    internal class PhotinoExecutionHandler : DotNetForHtml5.IJavaScriptExecutionHandler
    {
        private readonly Action<Action> _dispatcherAction;
        private readonly Func<Func<Task>, Task> _dispatcherAsyncAction;
        private readonly Func<bool> _dispatcherCheckAccessAction;
        private readonly Func<string, Task<object?>> _executeScriptAsyncAction;

        public PhotinoExecutionHandler(
            Func<string, Task<object?>> executeScriptAsyncAction,
            Action<Action> dispatcherAction,
            Func<Func<Task>, Task> dispatcherAsyncAction,
            Func<bool> dispatcherCheckAccessAction)
        {
            _executeScriptAsyncAction = executeScriptAsyncAction;
            _dispatcherAction = dispatcherAction;
            _dispatcherAsyncAction = dispatcherAsyncAction;
            _dispatcherCheckAccessAction = dispatcherCheckAccessAction;
        }

        public async void ExecuteJavaScript(string javaScriptToExecute)
        {
            if (_dispatcherCheckAccessAction())
            {
                await _executeScriptAsyncAction(javaScriptToExecute);
            }
            else
            {
                await _dispatcherAsyncAction(() => _executeScriptAsyncAction(javaScriptToExecute));
            }
        }

        public object? ExecuteJavaScriptWithResult(string javaScriptToExecute)
        {
            return ExecuteJavascriptAsync(javaScriptToExecute).GetAwaiter().GetResult();
        }

        private Task<object?> ExecuteJavascriptAsync(string javaScriptToExecute)
        {
            var tcs = new TaskCompletionSource<object?>();

            _dispatcherAction(async () =>
            {
                try
                {
                    var rawResult = await _executeScriptAsyncAction(javaScriptToExecute);

                    tcs.SetResult(rawResult);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
