
using System.Collections.Generic;
using System.Linq;

public enum PacketTypes
{
    nil,
    auth,
    updateinfo
   
}


public class NetRequest : Dictionary<string, string>
{
    private const string METHOD_KEY = "device-mac";

    public NetRequest(PacketTypes header)
    {
        AddParam(METHOD_KEY, "00:15:5D:01:7E:00");
    }

    public void AddParam(string key, string value)
    {
        var val = HttpUtility.HtmlEncode(value);

        if (ContainsKey(key))
            base[key] = val;
        else
            Add(key, val);
    }

    public void AddParam(string key, long value)
    {
        AddParam(key, value.ToString());
    }

    public void AddParam(string key, bool value)
    {
        AddParam(key, value ? 1 : 0);
    }

    public void RemoveParam(string key)
    {
        if (ContainsKey(key))
            Remove(key);
    }

    public string GetParamsString()
    {
        return string.Join("&", this.Select(n => n.Key + "=" + n.Value).ToArray());
    }
}