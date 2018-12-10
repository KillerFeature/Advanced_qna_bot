using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace Advanced_qna_bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity.Text.ToLower().IndexOf("action:") == 0)
            {
                var textSplit = activity.Text.Split(new string[] { ":" }, StringSplitOptions.None);
                string userName = "";
                switch (textSplit[1])
                {
                    case "user":
                        if (activity.From.Name != null)
                        {
                            userName = activity.From.Name;
                        }
                        break;

                    default:
                        break;
                }
                await context.PostAsync("Hi " + userName + "!");

                //await Conversation.SendAsync(activity, () => new Dialogs.ActionDialog());
            }
            else
            {
            

            qnahelper qnah = new qnahelper();
            var qnaResponse = await qnah.getQnAresults(activity.Text, context);
            // Return our reply to the user
            await context.PostAsync(qnaResponse);

            //await context.PostAsync($"You sent {activity.Text} which was {length} characters");


            }
            context.Wait(MessageReceivedAsync);
        }
    }
}