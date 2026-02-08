namespace OpaqueTokenSample.Infrastructure.Cache.Helper;

public class ResponseBase<TResponse> where TResponse : class
{
    public ResponseBase()
    {

    }
    public ResponseBase(string message)
    {
        messageList.Add(message);
    }
    public ResultEnum status { get; set; }
    public List<string> messageList { get; set; } = new(); 
    public TResponse? data { get; set; }
}
public enum ResultEnum
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}