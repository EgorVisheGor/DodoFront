using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HTTPServer.Attribures;

namespace HTTPServer;

public class HttpServer : IDisposable
{
    public ServerStatus Status = ServerStatus.Stop;

    private ServerSettings _serverSettings;
    private readonly HttpListener _httpListener;

    public HttpServer()
    {
        _serverSettings = ServerSettings.Deserialize();
        _httpListener = new HttpListener();
    }

    public void Start()
    {
        if (Status == ServerStatus.Start)
        {
            Console.WriteLine("Сервер уже запущен!");
        }
        else
        {
            Console.WriteLine("Запуск сервера...");

            _serverSettings = ServerSettings.Deserialize();
            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add($"http://localhost:" + _serverSettings.Port + "/");

            _httpListener.Start();
            Console.WriteLine("Ожидание подключений...");
            Status = ServerStatus.Start;
        }

        Listening();
    }

    public void Stop()
    {
        if (Status == ServerStatus.Start)
        {
            _httpListener.Stop();
            Status = ServerStatus.Stop;
            Console.WriteLine("Обработка подключений завершена");
        }
        else
            Console.WriteLine("Сервер уже остановлен");
    }

    private async void Listening()
    {
        while (_httpListener.IsListening)
        {
            //_httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);
            var context = await _httpListener.GetContextAsync();


            if (MethodHandler(context)) return;

            StaticFiles(context.Response, context.Request);
        }
    }

    private void StaticFiles(HttpListenerResponse response, HttpListenerRequest request)
    {
        try
        {
            byte[] buffer;

            var rawurl = request.RawUrl;

            buffer = Files.GetFile(rawurl.Replace("%20", " "));

            //Задаю расширения для файлов
            Files.GexExtension(ref response, "." + rawurl);

            //Неправильно задан запрос / не найдена папка
            if (buffer == null)
            {
                Show404(ref response, ref buffer);
            }

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            //закрываем поток
            output.Close();

            Listening();
        }

        catch
        {
            // ???
            Console.WriteLine("Возникла ошибка. Сервер остановлен");
            Stop();
        }
    }

    private bool MethodHandler(HttpListenerContext _httpContext)
    {
        // объект запроса
        HttpListenerRequest request = _httpContext.Request;

        // объект ответа
        HttpListenerResponse response = _httpContext.Response;

        //пустой url
        if (_httpContext.Request.Url.Segments.Length < 2) return false;

        string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");

        var assembly = Assembly.GetExecutingAssembly();

        // ищет контроллер accounts
        var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController)))
            .FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

        if (controller == null) return false;

        string[] strParams = _httpContext.Request.Url
            .Segments
            .Skip(2)
            .Select(s => s.Replace("/", ""))
            .ToArray();

        if (strParams.Length == 0) return false;

        string methodURI = strParams[0];

        var methods = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
            .Any(attr => attr.GetType().Name == $"Http{_httpContext.Request.HttpMethod}"));

        var method = methods.FirstOrDefault(x => _httpContext.Request.HttpMethod switch
        {
            "GET" => x.GetCustomAttribute<HttpGET>()?.MethodURI == methodURI,
            "POST" => x.GetCustomAttribute<HttpPOST>()?.MethodURI == methodURI
        });

        //object[] queryParams = method.GetParameters()
        //                    .Select(p => Convert.ChangeType(strParams[1], p.ParameterType))
        //                    .ToArray();

        object[] queryParams = null;

        if (request.HttpMethod == "POST")
        {
            ShowRequestData(request);
        }
        else
        {
            switch (methodURI)
            {
                case "getaccounts":
                    //параметров нет
                    break;
                case "getaccountbyid":
                    object[] temp = new object[1] { Convert.ToInt32(strParams[1]) };
                    queryParams = temp;
                    break;
                case "saveaccount":
                    //колхоз, как красиво написать?? (чтобы не переименовывать переменную)
                    object[] temp1 = new object[2] { Convert.ToString(strParams[1]), Convert.ToString(strParams[2]) };
                    queryParams = temp1;
                    break;
            }
        }


        var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

        response.ContentType = "Application/json";

        byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
        response.ContentLength64 = buffer.Length;

        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);

        output.Close();

        Listening();

        return true;
    }

    //метанит, прием данных с фронта (не работает)
    public object[] ShowRequestData(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            Console.WriteLine("No client data was sent with the request.");
            return null;
        }

        System.IO.Stream body = request.InputStream;
        System.Text.Encoding encoding = request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
        if (request.ContentType != null)
        {
            Console.WriteLine("Client data content type {0}", request.ContentType);
        }

        Console.WriteLine("Client data content length {0}", request.ContentLength64);

        Console.WriteLine("Start of client data:");
        // Convert the data to a string and display it on the console.
        string s = reader.ReadToEnd();
        Console.WriteLine(s);
        Console.WriteLine("End of client data:");
        body.Close();
        reader.Close();
        object[] paramsA = null;


        // If you are finished with the request, it should be closed also.
        return paramsA;
    }

    //Закидывает текст ошибки в buffer и настраивает response
    private void Show404(ref HttpListenerResponse response, ref byte[] buffer)
    {
        response.Headers.Set("Content-Type", "text/html");
        response.StatusCode = 404;
        response.ContentEncoding = Encoding.UTF8;
        string err = "<h1>404<h1> <h2>The resource can not be found.<h2>";
        buffer = Encoding.UTF8.GetBytes(err);
    }

    public void Dispose()
    {
        Stop();
    }
}