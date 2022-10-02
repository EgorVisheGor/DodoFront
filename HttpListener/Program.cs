// See https://aka.ms/new-console-template for more information

using System.Net;

HttpListener listener = new HttpListener();
// установка адресов прослушки
listener.Prefixes.Add("http://localhost:8888/connection/");
listener.Start();
Console.WriteLine("Ожидание подключений...");
// метод GetContext блокирует текущий поток, ожидая получение запроса 
HttpListenerContext context = listener.GetContext();
HttpListenerRequest request = context.Request;
// получаем объект ответа
HttpListenerResponse response = context.Response;
// создаем ответ в виде кода html
string responseStr = "<html><head><meta charset='utf8'></head><body>Привет мир!</body></html>";
byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
// получаем поток ответа и пишем в него ответ
response.ContentLength64 = buffer.Length;
Stream output = response.OutputStream;
output.Write(buffer, 0, buffer.Length);
// закрываем поток
output.Close();
// останавливаем прослушивание подключений
listener.Stop();
Console.WriteLine("Обработка подключений завершена");
Console.Read();