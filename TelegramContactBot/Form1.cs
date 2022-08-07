using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TL;
using WTelegram;

namespace DemoProject
{
    public partial class Form1 : Form
    {
        #region Telegram User Client
        string apiId = "00000"; 
        string apiHash = "xxxxx";
        string phoneNumber = "+xxxxxx";
        string password = "xxxxxxx";
        public static Client client = null;
        public static long ChatID;
        public static long ChatIDForBot;
        string userIDSelected;

        public static User user = null;
        public ListBox LB;

        
        public Form1()
        {
            InitializeComponent();
        }

        public string? Config(string what)
        {



            switch (what)
            {
                case "api_id": return apiId;
                case "api_hash": return apiHash;
                case "phone_number": return phoneNumber;
                case "verification_code": Console.Write("Verification Code: "); return Console.ReadLine();
                case "first_name": return "Test";      // if sign-up is required
                case "last_name": return "Test2";        // if sign-up is required
                case "password": return password;     // if user has enabled 2FA
                default: return null;                  // default config
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            File.Delete("session.dat");

        
            client = new Client(Config);

            user = await client.LoginUserIfNeeded();

            Console.WriteLine($"You are logged-as {user.username ?? user.first_name + " " + user.last_name} (id {user.id})");

            var contacts = await client.Contacts_GetContacts(user.access_hash);
            var t = typeof(InputContact[]);







        }

        #endregion

        #region Telegram Bot Client
        public TelegramBotClient bot;
        string bottoken = "xxxxxx";

        private async void Form1_Load(object sender, EventArgs e)
        {


            string path = Application.StartupPath + "\\" + "config.txt";
            string text = File.ReadAllText(path);
      
            text = text.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\r", "\n");
            var splittedtext = text.Split("\n");

            #region API ID
            int apitextst = splittedtext[0].IndexOf("api_id:") + "api_id:".Length;
            string apiidtext = splittedtext[0][apitextst..];
            apiId = apiidtext;
            #endregion

            #region API HASH

            int apihastst = splittedtext[1].IndexOf("api_hash:") + "api_hash:".Length;
            string apihashtext = splittedtext[1][apihastst..];
            apiHash = apihashtext;
            #endregion

            #region PHONE NUMBER

            int phonenumberst = splittedtext[2].IndexOf("phone_number:") + "phone_number:".Length;
            string phonenumbertext = splittedtext[2][phonenumberst..];
            phoneNumber = phonenumbertext;

            #endregion

            #region BOT TOKEN

            int bottokenst = splittedtext[3].IndexOf("bot_token:") + "bot_token:".Length;
            string bottokentext = splittedtext[3][bottokenst..];
            bottoken = bottokentext;

            #endregion

            #region PASSWORD

            int passwordst = splittedtext[4].IndexOf("password:") + "password:".Length;
            string passwordtext = splittedtext[4][passwordst..];
            password = passwordtext;
            #endregion

            bot = new TelegramBotClient(bottoken);
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            bot.StartReceiving(HandleUpdateAsync,
                               Handlers.HandleErrorAsync,
                               receiverOptions,
                               cts.Token);


            LB = listBox1;
            listBox1 = LB;
            await Task.Delay(-1);
        }


        public async Task BotOnMessageReceived(ITelegramBotClient botClient, Telegram.Bot.Types.Message msg)
        {
            Console.WriteLine($"Receive message type: {msg.Type}");
            if (msg.Type != MessageType.Text)
                return;

            var action = msg.Text!.Split(' ')[0] switch
            {
                "/inline" => SendInlineKeyboard(botClient, msg),
                "/keyboard" => SendReplyKeyboard(botClient, msg),
                "/remove" => RemoveKeyboard(botClient, msg),
                "/request" => RequestContactAndLocation(botClient, msg),
                "/addusers" => AddUsersToGroup(botClient, msg),
                "/getgroup" => GetGroup(botClient, msg),
                "/addallusers" => AddAllUsersToGroup(botClient, msg),
                _ => Usage(botClient, msg)
            };


            Telegram.Bot.Types.Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            async Task<Telegram.Bot.Types.Message> AddUsersToGroup(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                long chatid = long.Parse(message.Chat.Id.ToString()[1..]);
                if (user == null || client == null)
                {
                    return await botClient.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                              text: "You're not connected to client.");
                }

                if (message.From.Id != user.ID)
                {
                    return await botClient.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                             text: "You're no my creator.");
                }

                ChatID = chatid;

                ChatIDForBot = message.Chat.Id;

                if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                {
                    Contacts_Contacts contacts = await client.Contacts_GetContacts(user.access_hash);
                    List<UserBase> users_add = new List<UserBase>();
                    List<InputUserBase> newUserList = new List<InputUserBase>();


                    List<Telegram.Bot.Types.ChatMember> members = new List<Telegram.Bot.Types.ChatMember>();
                    int succesfulladd = 0;

                    if (contacts.users.Values.Count == 0)
                    {
                        return await botClient.SendTextMessageAsync(message.Chat.Id, "You've no friend. :(");
                    }
                    else
                    {

                        for (int i = 0; i < contacts.users.Values.Count; i++)
                        {

                            users_add.Add(contacts.users.Values.ToList()[i]);














                        }

                        foreach (var item in listBox1.SelectedItems)
                        {
                            var userstring = item.ToString();

                            if (userstring != null)
                            {
                                int stindex = userstring.IndexOf("{") + 1;

                                string userid = userstring[stindex..];
                                Console.WriteLine(userid);
                                Console.WriteLine(userstring);



                                if (users_add.Find(m => m.ID == long.Parse(userid)) != null)
                                {
                                    var useritem = users_add.Find(m => m.ID == long.Parse(userid));
                                    try
                                    {
                                        var kek = users_add.Remove(useritem);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }

                                }



                            }



                        }

                        for (int i = 0; i < users_add.Count; i++)
                        {
                            newUserList.Add(users_add[i]);
                        }

                        for (int i = 0; i < newUserList.Count; i++)
                        {
                            try
                            {
                                Console.WriteLine("A");
                                await client.Messages_AddChatUser(chatid, newUserList[i], 5000);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }

                        //     listBox1.Items.Clear();
                        //     for (int i = 0; i < contacts.users.Values.Count; i++)
                        //     {
                        //         try
                        //         {
                        //             Telegram.Bot.Types.ChatMember a = await botClient.GetChatMemberAsync(message.Chat.Id, contacts.users.Values.ToList()[i].ID);
                        //
                        //             listBox1.Items.Add(a.User.FirstName + " " + a.User.LastName +  "| @" + a.User.Username +"                                                                                                                                                                                                                                                                      id/{"+ a.User.Id);
                        //
                        //
                        //         }
                        //         catch (Exception ex)
                        //         {
                        //             Console.WriteLine(ex.Message);
                        //
                        //         }
                        //
                        //     }
                        // }




                        return await botClient.SendTextMessageAsync(message.Chat.Id, "You've added " +  users_add.Count + " Contact succesfully." + "\n");

                    }




                }
                else
                {
                    return await botClient.SendTextMessageAsync(message.Chat.Id, "This is not a group.");


                }




            }
            async Task<Telegram.Bot.Types.Message> AddAllUsersToGroup(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                long chatid = long.Parse(message.Chat.Id.ToString()[1..]);
                if (user == null || client == null)
                {
                    return await botClient.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                              text: "You're not connected to client.");
                }


                ChatID = chatid;

                ChatIDForBot = message.Chat.Id;

                if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                {
                    Contacts_Contacts contacts = await client.Contacts_GetContacts(user.access_hash);
                    List<UserBase> users_add = new List<UserBase>();
                    List<InputUserBase> newUserList = new List<InputUserBase>();


                    List<Telegram.Bot.Types.ChatMember> members = new List<Telegram.Bot.Types.ChatMember>();


                    if (contacts.users.Values.Count == 0)
                    {
                        return await botClient.SendTextMessageAsync(message.Chat.Id, "You've no friend. :(");
                    }
                    else
                    {

                        for (int i = 0; i < contacts.users.Values.Count; i++)
                        {

                            users_add.Add(contacts.users.Values.ToList()[i]);














                        }





                        for (int i = 0; i < users_add.Count; i++)
                        {
                            try
                            {

                                await client.Messages_AddChatUser(chatid, users_add[i], 5000);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }






                        return await botClient.SendTextMessageAsync(message.Chat.Id, "You've added " +  users_add.Count + " Contact succesfully." + "\n");

                    }




                }
                else
                {
                    return await botClient.SendTextMessageAsync(message.Chat.Id, "This is not a group.");


                }




            }
            async Task<Telegram.Bot.Types.Message> GetGroup(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                long chatid = long.Parse(message.Chat.Id.ToString()[1..]);
                if (user == null || client == null)
                {
                    return await botClient.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                              text: "You're not connected to client.");
                }

                if (message.From.Id != user.ID)
                {
                    return await botClient.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                             text: "You're no my creator.");
                }

                ChatID = chatid;

                ChatIDForBot = message.Chat.Id;

                if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                {
                    Contacts_Contacts contacts = await client.Contacts_GetContacts(user.access_hash);
                    List<InputUserBase> users_add = new List<InputUserBase>();
                    List<InputUserBase> emptylist = new List<InputUserBase>();


                    List<Telegram.Bot.Types.ChatMember> members = new List<Telegram.Bot.Types.ChatMember>();
                    Console.WriteLine(contacts.users.Values.Count);



                    listBox1.Items.Clear();
                    for (int i = 0; i < contacts.users.Values.Count; i++)
                    {
                        try
                        {
                            Telegram.Bot.Types.ChatMember a = await botClient.GetChatMemberAsync(message.Chat.Id, contacts.users.Values.ToList()[i].ID);
                            Console.WriteLine(a.User.FirstName + " " + a.User.LastName +  "| @" + a.User.Username);
                            listBox1.Items.Add(a.User.FirstName + " " + a.User.LastName +  "| @" + a.User.Username +"                                                                                                                                                                                                                                                                      id/{"+ a.User.Id);


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);

                        }

                    }




                    return await botClient.SendTextMessageAsync(message.Chat.Id, "I sended the values of Contact :)");

                }
                else
                {
                    return await botClient.SendTextMessageAsync(message.Chat.Id, "This is not a group.");

                }



            }

            static async Task<Telegram.Bot.Types.Message> SendInlineKeyboard(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                    });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: inlineKeyboard);
            }

            static async Task<Telegram.Bot.Types.Message> SendReplyKeyboard(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new  Telegram.Bot.Types.ReplyMarkups.KeyboardButton[] { "1.1", "1.2" },
                        new  Telegram.Bot.Types.ReplyMarkups.KeyboardButton[] { "2.1", "2.2" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: replyKeyboardMarkup);
            }

            static async Task<Telegram.Bot.Types.Message> RemoveKeyboard(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Removing keyboard",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }

            static async Task<Telegram.Bot.Types.Message> RequestContactAndLocation(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup RequestReplyKeyboard = new(
                    new[]
                    {
                     Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestLocation("Location"),
                     Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Contact"),
                    });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Who or Where are you?",
                                                            replyMarkup: RequestReplyKeyboard);
            }

            static async Task<Telegram.Bot.Types.Message> Usage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
            {
                const string usage = "Usage:\n" +
                                    "/addusers   - add all contacts except selected ones to group (only callable by API owner)\n" +
                                    "/getgroup   - gets group value and sending to client (only callable by API owner)\n" +
                                     "/adddallusers   - do this if your group member values are not showing up in client (only callable by API owner)\n" +
                                     "/inline   - send inline keyboard\n" +
                                     "/keyboard - send custom keyboard\n" +
                                     "/remove   - remove custom keyboard\n" +
                                     "/request  - request location or contact";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => Handlers.BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                UpdateType.InlineQuery => Handlers.BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                UpdateType.ChosenInlineResult => Handlers.BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                _ => Handlers.UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await Handlers.HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }
        #endregion



    }
}


