using System.Net;
using System.Net.Sockets;

namespace HTTPServer;

public class HttpServer
{
    private HttpListener listener;
    private string url;
    //старт
    public async Task Start()
    {
        listener =new HttpListener();
        listener.Prefixes.Add("http://localhost:8888/");
        listener.Start();
        await Receive();
    }
    //стоп
    public void Stop()
    {
        listener.Stop();
    }
    //обработка 
    public async Task Receive()
    {
        while (listener.IsListening)
        {
            var context =await listener.GetContextAsync();
            var responce = context.Response;
            var request = context.Request;

            switch (request.Url?.LocalPath)
            {
                case "/rabotavdodo":
                    await WebHelper.ShowStatic(context, @"/index.html");
                         break;
                default:
                    await WebHelper.ShowStatic(context);
                    break;
            }
            responce.OutputStream.Close();
            responce.Close();
        }
    }
}