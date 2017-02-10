using System;
using System.Collections;
using System.Collections.Generic;

public static class NetApiExt
{
    public static IEnumerator ContinueWith(this IEnumerator request, Action<NetResponse> resp)
    {
        var link = request;
        while (link.MoveNext())
        {
            yield return link;
        }

        var obj = link.Current as NetResponse;

        resp(obj);

        yield return obj;
    }
}