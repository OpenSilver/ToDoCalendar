using ToDoCalendar.Photino.JavaScript;
using ToDoCalendar.Photino.Threading;
using CSHTML5.Internal;
using DotNetForHtml5.Core;
using Photino.NET;
using System.Text.Json;

namespace ToDoCalendar.Photino.Runner
{
    public class PhotinoRunner(PhotinoWindow window)
    {
        private const string IdKey = "id";
        private const string TypeKey = "type";
        private const string ResultKey = "result";
        private const string CallbackIdKey = "callbackId";
        private const string IdWhereCallbackArgsAreStoredKey = "idWhereCallbackArgsAreStored";
        private const string CallbackArgsObject = "callbackArgsObject";

        private const string ResponseMessageType = "response";
        private const string InvokeDotNetMessageType = "invoke-net";
        private const string StartMessageType = "start";

        private static bool _isRunApplicationCalled;

        private int _idCounter = 0;
        private Dictionary<int, TaskCompletionSource<object?>> _communication = new();

        private TaskCompletionSource<bool> _jsStarted = new();

        private static readonly Lazy<OnCallbackSimulator> _onCallbackSimulator =
            new(() => new OnCallbackSimulator());
        private static OnCallbackSimulator OnCallbackSimulator => _onCallbackSimulator.Value;

        #region Interface implementation
        public async Task<T> RunApplicationAsync<T>(Func<Task<T>> createAppDelegate) where T : System.Windows.Application
        {
            ArgumentNullException.ThrowIfNull(createAppDelegate);

            if (_isRunApplicationCalled)
            {
                throw new InvalidOperationException("RunApplicationAsync can only be called once.");
            }
            _isRunApplicationCalled = true;

            window.RegisterWebMessageReceivedHandler((object? sender, string message) =>
                {
                    using JsonDocument doc = JsonDocument.Parse(message);
                    JsonElement root = doc.RootElement;

                    var typeValue = root.GetProperty(TypeKey).GetString();

                    if (typeValue == ResponseMessageType)
                    {
                        var id = root.GetProperty(IdKey).GetInt32();
                        var res = root.GetProperty(ResultKey);
                        if (res is JsonElement je)
                        {
                            _communication[id].SetResult(GetValueFromJsonElement(je));
                        }
                        else
                        {
                            _communication[id].SetResult(res);
                        }
                    }
                    else if (typeValue == InvokeDotNetMessageType)
                    {
                        InkoveFromJs(root.GetProperty(CallbackIdKey).GetInt32(),
                            root.GetProperty(IdWhereCallbackArgsAreStoredKey).GetString() ?? "",
                            root.GetProperty(CallbackArgsObject).EnumerateArray().ToArray());
                    }
                    else if (typeValue == StartMessageType)
                    {
                        _jsStarted.SetResult(true);
                    }
                });
            await StartJsAsync();
            var context = InitializeOpenSilver();
            var tcs = new TaskCompletionSource<T>();

            context.Post(async (s) => {
                try
                {
                    var app = await createAppDelegate();
                    tcs.SetResult(app);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            var result = await tcs.Task;
            return result;
        }

        public Task<T> RunApplicationAsync<T>(Func<T> createAppDelegate) where T : System.Windows.Application
        {
            return RunApplicationAsync(() => Task.FromResult(createAppDelegate()));
        }

        public Task<T> RunApplicationAsync<T>() where T : System.Windows.Application, new()
            => RunApplicationAsync(() => new T());

        #endregion

        private static object? GetValueFromJsonElement(JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Object or JsonValueKind.Array => jsonElement,
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetDouble(),
                JsonValueKind.True or JsonValueKind.False => jsonElement.GetBoolean(),
                JsonValueKind.Undefined or JsonValueKind.Null or _ => null,
            };
        }

        private async Task<object?> ExecuteJavaScriptAsync(string js)
        {
            try
            {
                var id = _idCounter++;
                var tcs = new TaskCompletionSource<object?>();
                _communication[id] = tcs;
                var message = JsonSerializer.Serialize(new { id, js });
                window.SendWebMessage(message);
                var result = await tcs.Task;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task StartJsAsync()
        {
            await _jsStarted.Task;
        }

        private BackgroundThreadSynchronizationContext InitializeOpenSilver()
        {
            var currentThread = Environment.CurrentManagedThreadId;
            var handler = new PhotinoExecutionHandler(
                ExecuteJavaScriptAsync,
                a => {
                    window.Invoke(a);
                },
                a => {
                    var tcs = new TaskCompletionSource<Task>();
                    window.Invoke(() =>
                    {
                        tcs.SetResult(a());
                    });
                    return tcs.Task;
                },
                () => Thread.CurrentThread.ManagedThreadId == currentThread
            );

            INTERNAL_Simulator.JavaScriptExecutionHandler = handler;
            INTERNAL_Simulator.IsRunningInTheSimulator_WorkAround = true;

            var context = new BackgroundThreadSynchronizationContext();

            var thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(context);

                context.RunMessageLoop();
            })
            {
                IsBackground = true
            };
            thread.Start();

            INTERNAL_Simulator.OpenSilverDispatcherBeginInvoke = (method) => context.Post((s) => method(), null);
            INTERNAL_Simulator.OpenSilverDispatcherInvoke = (method, _) => {
                if (context.CheckAccess())
                {
                    method();
                }
                else
                {
                    context.Send((s) => method(), null);
                }
            };
            INTERNAL_Simulator.OpenSilverDispatcherCheckAccess = context.CheckAccess;

            return context;
        }

        private static void InkoveFromJs(int callbackId, string idWhereCallbackArgsAreStored,
            JsonElement[] callbackArgsObject)
        {
            OnCallbackSimulator.OnCallbackFromJavaScript(
                callbackId,
                idWhereCallbackArgsAreStored,
                callbackArgsObject.Select(GetValueFromJsonElement).ToArray(),
                false
            );
        }

        public static void ErrorFromJs(string idWhereCallbackArgsAreStored)
        {
            OnCallbackSimulator.OnCallbackFromJavaScriptError(
                idWhereCallbackArgsAreStored
            );
        }
    }
}
