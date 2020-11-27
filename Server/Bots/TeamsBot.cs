// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using TeamsAuth.Interfaces.Service;

namespace Microsoft.BotBuilderSamples
{
    public class TeamsBot<T> : DialogBot<T> where T : Dialog
    {
        private readonly IStorageService _storageService;

        public TeamsBot(
            ConversationState conversationState,
            UserState userState,
            T dialog,
            ILogger<DialogBot<T>> logger,
            IStorageService storageService)
            : base(conversationState, userState, dialog, logger)
        {
            _storageService = storageService;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var user = await _storageService.RetrieveEntityUsingPointQueryAsync<TableEntity>("User", member.Name, member.Id);
                    if (user.ToList().Count == 0)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to KeyStorageBot. In order to start using the service you need to set a password, for this use 'password' command."), cancellationToken);
                    } else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to KeyStorageBot. Type login to get logged in."), cancellationToken);
                    }
                }
            }
        }

        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
