using System.Collections.Generic;

namespace SAaP.Core.Models.HttpResponse;

internal class EmResponse<T>
{
    public string GlobalId { get; set; }

    public string Message { get; set; }

    public int Status { get; set; }

    public int Code { get; set; }

    public List<T> Data { get; set; }
}