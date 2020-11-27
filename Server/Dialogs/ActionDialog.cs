// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class ActionDialog : ComponentDialog
    {
        private readonly IBotService _botService;

        public ActionDialog(string id, string connectionName, IBotService botService)
            : base(id)
        {
            _botService = botService;

            ConnectionName = connectionName;
        }

        protected string ConnectionName { get; }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                if (text.IndexOf("logout") >= 0)
                {
                    return await _botService.LogoutAsync(innerDc, ConnectionName, cancellationToken);
                }

                if (text.IndexOf("password") >= 0)
                {
                    return await _botService.PasswordAsync(innerDc, text, cancellationToken);
                }

                if (text.IndexOf("login") >= 0)
                {
                    return await _botService.LoginAsync(innerDc, text, cancellationToken);
                }

                if (text.IndexOf("add") >= 0)
                {
                    return await _botService.AddPairAsync(innerDc, text, cancellationToken);
                }

                if (text.IndexOf("get all") >= 0)
                {
                    return await _botService.GetAllPairsAsync(innerDc, text, cancellationToken);
                }

                if (text.IndexOf("get") >= 0)
                {
                    return await _botService.GetPairAsync(innerDc, text, cancellationToken);
                }
            }

            return null;
        }
    }
}
