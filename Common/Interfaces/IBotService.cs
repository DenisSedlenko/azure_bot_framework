using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Common.Interfaces
{
    public interface IBotService
    {
        Task<DialogTurnResult> AddPairAsync(DialogContext context, string text, CancellationToken cancellationToken);

        Task<DialogTurnResult> GetPairAsync(DialogContext context, string text, CancellationToken cancellationToken);

        Task<DialogTurnResult> GetAllPairsAsync(DialogContext context, string text, CancellationToken cancellationToken);

        Task<DialogTurnResult> LoginAsync(DialogContext context, string text, CancellationToken cancellationToken);

        Task<DialogTurnResult> LogoutAsync(DialogContext context, string connectionName, CancellationToken cancellationToken);

        Task<DialogTurnResult> PasswordAsync(DialogContext context, string text, CancellationToken cancellationToken);
    }
}
