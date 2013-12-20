#region

using System;
using System.Threading.Tasks;
using HipChat;

#endregion

namespace RemoteMonitor
{
    public class HipChatClientDff
    {
        public enum BackgroundColor
        {
            Gray,
            Green,
        }


        public static async void SendToHipChat(string statusReport, BackgroundColor color, string hipChatToken,
            string hipChatRoom, string hipChatUser)
        {
            await Task.Run(delegate
            {
                try
                {
                    var hipChatColor = HipChatClient.BackgroundColor.gray;
                    if (color == BackgroundColor.Green) hipChatColor = HipChatClient.BackgroundColor.green;
                    HipChatClient.SendMessage(hipChatToken, hipChatRoom, hipChatUser,
                        statusReport, false, hipChatColor, HipChatClient.MessageFormat.html);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
                );
        }
    }
}