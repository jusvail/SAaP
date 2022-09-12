using SAaP.Contracts.Enum;

namespace SAaP.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    InstanceType GetPageInstanceType(string key);
}