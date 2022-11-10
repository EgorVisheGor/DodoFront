using HTTPServer;
using HTTPServer.MyORM;

internal class Program
{
    private static bool _appIsRunning = true;
    
    static void Main(string[] args)
    {
        string str = @"Data source=postgres.public;Database=postgres;Integrated Security=true;";

        var result = new Database(str).AddParameter("@Name", "SomeTea")
            .AddParameter("@Price", 34.34M)
            .ExecuteNonQuery("insert into ProductRecord values (@Name,@Price)");
        Console.WriteLine(result);
        
        var af = Directory.Exists("\\site\\index.html" );

        //--Работа с найстройками сервера (сериализация и десериализация json)--
        var settings = new ServerSettings();
        settings.Serialize();
        var settingsDeserialized = ServerSettings.Deserialize();


        //Запуск сервера
        var httpserver = new HttpServer();
        using (httpserver)
        {
            httpserver.Start();

            while (_appIsRunning)
            {
                Handler(Console.ReadLine()?.ToLower(), httpserver);
            }
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    
    static void Handler(string command, HttpServer httpserver)
    {
        switch (command)
        {
            case "stop":
                httpserver.Stop();
                break;
            case "start":
                httpserver.Start();
                break;
            case "restart":
                httpserver.Stop();
                httpserver.Start();
                break;
            case "status":
                Console.WriteLine(httpserver.Status);
                break;
            case "exit":
                _appIsRunning = false;
                break;
        }
    }
}