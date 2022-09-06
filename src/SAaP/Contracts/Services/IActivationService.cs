using System.Threading.Tasks;

namespace SAaP.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
