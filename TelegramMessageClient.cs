using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBotWPF
{
    class TelegramMessageClient
    {
        public ObservableCollection<Message> Messages {get; set;}
        private static TelegramBotClient botClient;
        private MainWindow w;
        public static ObservableCollection<float> values = new ObservableCollection<float>();
        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {           
            switch (e.Message.Type)
            {
                #region Обработка текстовых сообщений             
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    w.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(
                        new Message(
                            DateTime.Now.ToLongTimeString(),
                            e.Message.Text,
                            e.Message.Chat.FirstName,
                            e.Message.Chat.Id,
                            Convert.ToString(e.Message.Type),
                            String.Empty,
                            String.Empty));
                    });

                    switch (e.Message.Text)
                    {
                        case null:
                            return;

                        case "/start":
                            botClient.SendTextMessageAsync(e.Message.Chat.Id,
                                "Данный бот может выводить курс криптовалюты, " +
                                "для этого необходимо ввести пару, например: BTCUSDT.");
                            break;

                        case "/file":
                            try
                            {
                                var files = new DirectoryInfo($@"{e.Message.Chat.Id}").GetFiles();

                                string file;
                                for (int i = 0; i < files.Length; i++)
                                {
                                    file = files[i].Name;
                                    botClient.SendTextMessageAsync(e.Message.Chat.Id, $"/download{i}-{file}");
                                }
                            }
                            catch (Exception error)
                            {
                                if (error != null)
                                {
                                    botClient.SendTextMessageAsync(e.Message.Chat.Id, "В каталоге отсутствуют файлы");
                                }
                            }

                            break;

                        case "/help":
                            botClient.SendTextMessageAsync(e.Message.Chat.Id,
                                "/start-Описание бота\n" +
                                "/file-Список загруженных файлов");
                            break;

                        default:
                            try
                            {
                                var filesForDownload = new DirectoryInfo($@"{e.Message.Chat.Id}").GetFiles();
                                for (int i = 0; i < filesForDownload.Length; i++)
                                {
                                    if (e.Message.Text == $"/download{i}")
                                    {
                                        string file = filesForDownload[i].Name;
                                        for (int j = 0; j < Messages.Count; j++)
                                        {
                                            if (file == Messages[j].FileName)
                                            {
                                                string fileId = Messages[j].FileId;
                                                InputOnlineFile inputOnlineFile = new InputOnlineFile(fileId);
                                                switch (Messages[j].Type)
                                                {
                                                    case "Photo":
                                                        botClient.SendPhotoAsync(e.Message.Chat.Id, inputOnlineFile);
                                                        break;
                                                    case "Video":
                                                        botClient.SendVideoAsync(e.Message.Chat.Id, inputOnlineFile);
                                                        break;
                                                    case "Document":
                                                        botClient.SendDocumentAsync(e.Message.Chat.Id, inputOnlineFile);
                                                        break;
                                                }
                                            }
                                        }
                                    }                                   
                                }
                            }
                            catch (Exception error)
                            {
                                if (error!=null)
                                {
                                    float temp = RequestforBinance(e.Message.Text,e.Message.Chat.Id);
                                    botClient.SendTextMessageAsync(e.Message.Chat.Id, Convert.ToString(temp));
                                }
                                
                                
                            }
                                                        
                            break;
                    }
                    break;
                #endregion
                #region Обработка картинок
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    Console.WriteLine($"В {DateTime.Now.ToLongTimeString()} " +
                        $"пользователь: {e.Message.Chat.FirstName} {e.Message.Chat.Id} " +
                        $"отправил : {e.Message.Type}");

                    DownLoad(e.Message.Photo[e.Message.Photo.Length - 1].FileId, "photo.jpg", e.Message.Chat.Id);
                    w.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(
                        new Message(
                            DateTime.Now.ToLongTimeString(),
                            e.Message.Text,
                            e.Message.Chat.FirstName,
                            e.Message.Chat.Id,
                            Convert.ToString(e.Message.Type),
                            e.Message.Photo[e.Message.Photo.Length - 1].FileId,
                            "photo.jpg"));
                    });                    
                    break;
                #endregion
                #region Обработка аудиофайлов
                case Telegram.Bot.Types.Enums.MessageType.Audio:
                    Console.WriteLine($"В {DateTime.Now.ToLongTimeString()} " +
                        $"пользователь: {e.Message.Chat.FirstName} {e.Message.Chat.Id} " +
                        $"отправил : {e.Message.Type}");

                    DownLoad(e.Message.Audio.FileId, e.Message.Audio.Title, e.Message.Chat.Id);
                    w.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(
                        new Message(
                            DateTime.Now.ToLongTimeString(),
                            e.Message.Text,
                            e.Message.Chat.FirstName,
                            e.Message.Chat.Id,
                            Convert.ToString(e.Message.Type),
                            e.Message.Audio.FileId,
                            e.Message.Audio.Title));
                    });
                    break;
                #endregion
                #region Обработка видеофайлов
                case Telegram.Bot.Types.Enums.MessageType.Video:
                    Console.WriteLine($"В {DateTime.Now.ToLongTimeString()} " +
                        $"пользователь: {e.Message.Chat.FirstName} {e.Message.Chat.Id} " +
                        $"отправил : {e.Message.Type}");

                    DownLoad(e.Message.Video.FileId, "video.mp4", e.Message.Chat.Id);
                    w.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(
                        new Message(
                            DateTime.Now.ToLongTimeString(),
                            e.Message.Text,
                            e.Message.Chat.FirstName,
                            e.Message.Chat.Id,
                            Convert.ToString(e.Message.Type),
                            e.Message.Video.FileId,
                            "video.mp4"));
                    });                  
                    break;
                #endregion
                #region Обработка документов
                case Telegram.Bot.Types.Enums.MessageType.Document:
                    Console.WriteLine($"В {DateTime.Now.ToLongTimeString()} " +
                        $"пользователь: {e.Message.Chat.FirstName} {e.Message.Chat.Id} " +
                        $"отправил : {e.Message.Type}");

                    DownLoad(e.Message.Document.FileId, e.Message.Document.FileName, e.Message.Chat.Id);
                    w.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(
                        new Message(
                            DateTime.Now.ToLongTimeString(),
                            e.Message.Text,
                            e.Message.Chat.FirstName,
                            e.Message.Chat.Id,
                            Convert.ToString(e.Message.Type),
                            e.Message.Document.FileId,
                            e.Message.Document.FileName));
                    });                 
                    break;
                    #endregion
            }   
        }

        public TelegramMessageClient(MainWindow R, string PathToken = @"token.txt")
        {
            this.Messages = new ObservableCollection<Message>();
            this.w = R;

            botClient = new TelegramBotClient(File.ReadAllText(PathToken));

            botClient.OnMessage += MessageListener;

            botClient.StartReceiving();
        }

        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            botClient.SendTextMessageAsync(id, Text);
        }

        public void SerialisedMessage()
        {
            string json = JsonConvert.SerializeObject(Messages);
            File.WriteAllText("_historyMessages.json", json);
        }

        static async void DownLoad(string fileId, string path, long chatId)
        {

            DirectoryInfo newPath = Directory.CreateDirectory($@"{chatId}");
            Telegram.Bot.Types.File file = await botClient.GetFileAsync(fileId);
            FileStream fs = new FileStream($"{newPath}/{path}", FileMode.Create);
            await botClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();

            fs.Dispose();
        }

        public void StartProcess(string ChatId)
        {
            FileInfo fileInfo = new FileInfo(ChatId);
            var fullName = fileInfo.FullName;
            Process.Start(fullName); 
        }

        private static float StringSplit(string ResultApiBinance)
        {
            string[] temp = ResultApiBinance.Split(',', ':', '"', '{', '}');
            float result = float.Parse(temp[11]);
            return result;
        }

        public float RequestforBinance(string Symbol, long ChatId)
        {

            WebClient clientBinance = new WebClient() { Encoding = Encoding.UTF8 };
            string startUrlBinance = @"https://api.binance.com/api/v3/ticker/price";
            float result = 0;
            try
            {
                string urlBinance = $"{startUrlBinance}?symbol={Symbol}";
                string responseText = clientBinance.DownloadString(urlBinance);
                result = StringSplit(responseText);
                values.Add(result);
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    if (webException.Response is HttpWebResponse)
                    {
                        botClient.SendTextMessageAsync(ChatId, "Ошибка!" +
                            "\nЕсли вы хотите посмотреть курс криптовалюты, " +
                            "необходимо ввести существующую пару с верхним регистром" +
                            "\nПросмотреть список команд: /help");
                    }
                }
            }


            return result;
        }

        
    }
}
