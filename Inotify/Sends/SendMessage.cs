namespace Inotify.Sends
{
    public class SendMessage
    {
        public SendMessage()
        {

        }

        public SendMessage(SendMessage sendMessage)
        {
            Token = sendMessage.Token;
            Title = sendMessage.Title;
            Data = sendMessage.Data;
            Key = sendMessage.Key;

        }
        public string Token;
        public string Title;
        public string? Data;
        public string? Key;
    }


}
