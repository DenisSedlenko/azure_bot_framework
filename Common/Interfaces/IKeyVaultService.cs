using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IKeyVaultService
    {
        Task<bool> SetSecret(string secretName, string secretValue);

        Task<string> GetSecret(string secretName);
    }
}
