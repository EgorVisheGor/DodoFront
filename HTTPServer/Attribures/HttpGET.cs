namespace HTTPServer.Attribures;

public class HttpGET : Attribute
{
    public string MethodURI { get; set; }

    public HttpGET(string methodUri)
    {
        MethodURI = methodUri;
    }
}