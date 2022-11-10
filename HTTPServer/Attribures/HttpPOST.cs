namespace HTTPServer.Attribures;

public class HttpPOST : Attribute
{
    public string MethodURI { get; set; }

    public HttpPOST(string methodUri)
    {
        MethodURI = methodUri;
    }
}