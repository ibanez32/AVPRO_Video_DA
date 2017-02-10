using System.Collections.Generic;

public class NetResponse : Dictionary<string, string>
{
    private const string ERROR_KEY = "error";

    public string GetError
    {
        get
        {
            if (IsError)
                return base[ERROR_KEY];

            return string.Empty;
        }
    }

    public bool IsError
    {
        get { return ContainsKey(ERROR_KEY); }
    }

    public NetResponse(string data)
    {
        var mas = data.Split(new[] { '&' });

        foreach (var s in mas)
        {
            var keyVal = s.Split(new[] { '=' });
            Add(keyVal[0], HttpUtility.UrlDecode(keyVal[1]));
        }
    }
}
