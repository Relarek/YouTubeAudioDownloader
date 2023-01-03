using MediaToolkit.Model;
using MediaToolkit;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VideoLibrary;

var botClient = new TelegramBotClient("5425622449:AAF-myA2olDH8RwI_yJOBKIfRz5ZnXGIlKg");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    SaveMP3(@"G:\Music", "https://www.youtube.com/watch?v=JDXfqN3VKhc", "asd");
    // Echo received message text
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "You said:\n" + messageText,
        cancellationToken: cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
async void SaveMP3(string SaveToFolder, string VideoURL, string MP3Name)
{
    var source = @"G:\Music";
    var youtube = YouTube.Default;
    var vid = youtube.GetVideo(VideoURL);
    System.IO.File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

    var inputFile = new MediaFile { Filename = source + vid.FullName };
    var outputFile = new MediaFile { Filename = $"{MP3Name}.mp3" };

    using (var engine = new Engine())
    {
        engine.GetMetadata(inputFile);

        engine.Convert(inputFile, outputFile);
    }
}