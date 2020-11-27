using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Utils;
using Common.Interfaces;
using Common.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage.Table;
using TeamsAuth.Interfaces.Service;

namespace BusinessLayer.Services
{
    public class BotService : IBotService
    {
        private readonly IStorageService _storageService;
        private readonly IKeyVaultService _keyVaultService;

        public BotService(IStorageService storageService, IKeyVaultService keyVaultService)
        {
            _storageService = storageService;
            _keyVaultService = keyVaultService;
        }

        public async Task<DialogTurnResult> AddPairAsync(DialogContext dialogContext, string text, CancellationToken cancellationToken)
        {
            if (!await IsActiveSession(dialogContext, dialogContext.Context.Activity.From, cancellationToken))
            {
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            var data = text.Replace("add", "").Trim().Split(":");

            try
            {
                if (data.Length == 2 && await _keyVaultService.SetSecret(data[0], data[1])) {
                    var pairEntity = new PairEntity(dialogContext.Context.Activity.From.Id, data[0])
                    {
                        UserId = new Guid(dialogContext.Context.Activity.From.Id),
                        Service = data[0]
                    };

                    var entity = await _storageService.InsertOrMergeEntityAsync("Pair", pairEntity);
                    if (entity == null)
                    {
                        await dialogContext.Context.SendActivityAsync(MessageFactory.Text("Try again."), cancellationToken);
                    }
                    else
                    {
                        await dialogContext.Context.SendActivityAsync(MessageFactory.Text("New pair id successfully added. \nTo get password from service use 'get SERVICE_NAME' command. \n To get list pairs use 'get all' command"), cancellationToken);
                    }

                }
            }
            catch
            {
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> GetAllPairsAsync(DialogContext dialogContext, string text, CancellationToken cancellationToken)
        {
            if (!await IsActiveSession(dialogContext, dialogContext.Context.Activity.From, cancellationToken))
            {
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            var all = text.Replace("get", "").Trim();

            if (all.Equals("all"))
            {
                var pairs = (await _storageService.RetrieveEntityUsingPointQueryAsync<PairEntity>("Pair", dialogContext.Context.Activity.From.Id, all)).ToArray();

                if (pairs.Length == 0)
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("You haven't added pairs yet."), cancellationToken);
                }
                else
                {
                    var services = new List<string>();
                    foreach(var pair in pairs)
                    {
                        var secret = await _keyVaultService.GetSecret(pair.Service);
                        services.Add($"{pair.Service} - {secret}");
                    }

                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text(string.Join("\n", services)), cancellationToken);
                }
            }

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> GetPairAsync(DialogContext dialogContext, string text, CancellationToken cancellationToken)
        {
            if (!await IsActiveSession(dialogContext, dialogContext.Context.Activity.From, cancellationToken))
            {
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            var serviceName = text.Replace("get", "").Trim();

            var pairs = (await _storageService.RetrieveEntityUsingPointQueryAsync<PairEntity>("Pair", dialogContext.Context.Activity.From.Id, serviceName)).ToArray();

            if (pairs.Length == 0)
            {
                await dialogContext.Context.SendActivityAsync(MessageFactory.Text("Information about this service is not found."), cancellationToken);
            }
            else
            {
                await dialogContext.Context.SendActivityAsync(MessageFactory.Text(await _keyVaultService.GetSecret(pairs[0].Service)), cancellationToken);
            }

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> LoginAsync(DialogContext dialogContext, string text, CancellationToken cancellationToken = default)
        {

            if (await IsActiveSession(dialogContext, dialogContext.Context.Activity.From, cancellationToken, false))
            {
                await dialogContext.Context.SendActivityAsync(MessageFactory.Text("You are already logged in."), cancellationToken);
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            var user = (await _storageService.RetrieveEntityUsingPointQueryAsync<UserEntity>("User", dialogContext.Context.Activity.From.Name, dialogContext.Context.Activity.From.Id)).ToArray();
            if (user.Length != 0)
            {
                var password = text.Replace("login", "").Trim();
                if (user[0].Password.Equals(CryptographyProcessor.GenerateHash(password, user[0].Salt)))
                {
                    user[0].UpdatedAt = DateTime.UtcNow;
                    await _storageService.InsertOrMergeEntityAsync("User", user[0]);
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("You are successfully logged in. To add new password from service use 'add SERVICE_NAME:PASSWORD' command. \nTo get password from service use 'get SERVICE_NAME' command. \n To get list pairs use 'get all' command"), cancellationToken);
                } else
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("Your password is wrong. Please try again."), cancellationToken);
                }

            }

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> LogoutAsync(DialogContext dialogContext, string connectionName, CancellationToken cancellationToken = default)
        {
            var botAdapter = (BotFrameworkAdapter)dialogContext.Context.Adapter;
            await botAdapter.SignOutUserAsync(dialogContext.Context, connectionName, null, cancellationToken);
            await dialogContext.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> PasswordAsync(DialogContext dialogContext, string text, CancellationToken cancellationToken = default)
        {
            var user = (await _storageService.RetrieveEntityUsingPointQueryAsync<TableEntity>("User", dialogContext.Context.Activity.From.Name, dialogContext.Context.Activity.From.Id)).ToArray();

            if (user.Length == 0)
            {
                var password = text.Replace("password", "").Trim();
                var userEntity = new UserEntity(dialogContext.Context.Activity.From.Name, dialogContext.Context.Activity.From.Id)
                {
                    Id = new Guid(dialogContext.Context.Activity.From.Id),
                    Username = dialogContext.Context.Activity.From.Name,
                    Salt = CryptographyProcessor.CreateSalt(8)
                };
                userEntity.Password = CryptographyProcessor.GenerateHash(password, userEntity.Salt);
                userEntity.UpdatedAt = DateTime.Now;

                var entity = await _storageService.InsertOrMergeEntityAsync("User", userEntity);
                if (entity == null)
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("Try again."), cancellationToken);
                } else
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("You are successfully set password. To add new password from service use 'add SERVICE_NAME:PASSWORD' command. \nTo get password from service use 'get SERVICE_NAME' command. \n To get list pairs use 'get all' command"), cancellationToken);
                }
            }

            return await dialogContext.CancelAllDialogsAsync(cancellationToken);
        }

        private async Task<bool> IsActiveSession(DialogContext dialogContext, ChannelAccount account, CancellationToken cancellationToken = default, bool showMessage = true)
        {
            var user = (await _storageService.RetrieveEntityUsingPointQueryAsync<UserEntity>("User", account.Name, account.Id)).ToArray();

            if (user.Length == 0 || (user.Length != 0 && user[0].UpdatedAt.AddSeconds(60) < DateTime.UtcNow))
            {
                if (showMessage)
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text("Your session is expired. Re-login please, for that use 'login' command"), cancellationToken);
                }

                return false;
            }

            return true;
        }
    }
}
