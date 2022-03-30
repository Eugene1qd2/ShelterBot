using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telebot
{

    public partial class Form1 : Form
    {

        public static Random random = new Random((int)DateTime.Now.Ticks);
        private static bool helpfull_list_updated = false;
        public static List<Lobby> lobbies = new List<Lobby>();
        private static string token = "5108404020:AAE0EtaZh-Th3XoZ7KOXOVLb-8TdbUAgUvw";
        public static TelegramBotClient client = new TelegramBotClient(token);

        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.StartReceiving();
            client.OnMessage += MessageHandler;
            client.OnMessage += InputLobbyId;
            client.OnMessage += LobbyHandler;
            client.OnMessage += GameHandler;
            PrintEveryone("Бот запущен! (✿◠‿◠)", StartButtons());
            button1.Enabled = false;
        }

        private void GameHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            switch(message.Text)
            {
                case "my_cards":
                    if (lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count>0).ToList().Count == 0) break;
                    Lobby lobby = lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList()[0];
                    Player player = lobby.players.Where(x => x.id == message.Chat.Id).ToList()[0];
                    string mes = "";
                    player.cards.ForEach(x =>
                    {
                        mes += "<b>"+x.type.ToString()+"</b>" + ": " + x.text+"\n";
                    });
                    SendMessage(player.id, mes);
                    break;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                client.StopReceiving();
            }
            catch (Exception)
            {
            }
        }
        private static void AddNewUser(Telegram.Bot.Types.Chat chat)
        {
            StreamReader sr = new StreamReader("Users.txt");
            if (!sr.EndOfStream)
            {
                string[] chats = sr.ReadToEnd().Split('\n');
                sr.Close();
                for (int i = 0; i < chats.Length; i++)
                {
                    if (long.TryParse(chats[i], out long id))
                    {
                        if (id == chat.Id)
                            return;
                    }
                }
            }
            sr.Close();

            using (StreamWriter sw = new StreamWriter("Users.txt", true))
            {
                sw.WriteLine(chat.Id);
                sw.Close();
                Console.WriteLine("Новый пользователь зарегистрирован!");
            }
        }
        private static void PrintEveryone(string message, IReplyMarkup Buttons)
        {
            using (StreamReader sr = new StreamReader("Users.txt"))
            {
                string[] chats = sr.ReadToEnd().Split('\n');
                for (int i = 0; i < chats.Length; i++)
                {
                    if (long.TryParse(chats[i], out long Id))
                    {
                        Telegram.Bot.Types.Chat chat = new Telegram.Bot.Types.Chat();
                        chat.Id = Id;
                        client.SendTextMessageAsync(chat, message, replyMarkup: Buttons); ;
                    }
                }
                Console.WriteLine("Рассылка заверщена успешно!");
            }
        }
        private static void PrintEveryone(string message)
        {
            using (StreamReader sr = new StreamReader("Users.txt"))
            {
                string[] chats = sr.ReadToEnd().Split('\n');
                for (int i = 0; i < chats.Length; i++)
                {
                    if (long.TryParse(chats[i], out long Id))
                    {
                        Telegram.Bot.Types.Chat chat = new Telegram.Bot.Types.Chat();
                        chat.Id = Id;
                        client.SendTextMessageAsync(chat, message);
                    }
                }
                Console.WriteLine("Рассылка заверщена успешно!");
            }
        }

        public static Lobby CreateNewLobby(long id)
        {
            uint lobby_id = (uint)random.Next() % 89999999 + 1000000;
            while (lobbies.Where(x => x.lobby_id == lobby_id).ToList().Count > 0)
            {
                lobby_id = ((uint)random.Next() + 1000000) % 100000000;
            }
            Lobby new_lobby = new Lobby(id, lobby_id);
            lobbies.Add(new_lobby);
            return new_lobby;
        }

        public static IReplyMarkup LobbyButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>{ new KeyboardButton{Text="delete" },new KeyboardButton { Text = "start" } , new KeyboardButton { Text = "leave" } , new KeyboardButton { Text = "info" } }
                }
            };
        }
        public static IReplyMarkup StartButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>{ new KeyboardButton{Text="create_lobby" },new KeyboardButton { Text = "join_lobby" } }
                }
            };
        }
        public static IReplyMarkup PlayerButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>{ new KeyboardButton{Text="my_cards" }}
                }
            };
        }

        private static void MessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            Console.WriteLine(message.From.FirstName + ": " + message.Text);
            string keyword = message.Text.Split()[0];
            AddNewUser(message.Chat);
            switch (keyword)
            {
                case "/start":

                    client.SendTextMessageAsync(message.Chat, "Чего желаете?", replyMarkup: StartButtons());
                    break;
                case "create_lobby":
                    if (lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList().Count == 0)
                    {
                        Lobby lobby = CreateNewLobby(message.From.Id);
                        helpfull_list_updated = true;
                        client.SendTextMessageAsync(message.Chat, $"Лобби успешно создано\nid лобби: {lobby.lobby_id}", replyMarkup: LobbyButtons());
                    }
                    else
                    {
                        client.SendTextMessageAsync(message.Chat, "Вы уже находитесь в лобби!");
                    }
                    break;
                case "join_lobby":
                    if (lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList().Count == 0)
                        client.SendTextMessageAsync(message.Chat, $"Введите id лобби");
                    else
                        client.SendTextMessageAsync(message.Chat, "Вы уже находитесь в лобби!");
                    break;

            }

        }
        public static void SendMessage(Lobby lobby, string message)
        {
            lobby.players.ForEach(x => client.SendTextMessageAsync(x.id, message, Telegram.Bot.Types.Enums.ParseMode.Html));
        }
        public static void SendMessage(Lobby lobby, string message, IReplyMarkup Buttons)
        {
            lobby.players.ForEach(x => client.SendTextMessageAsync(x.id, message, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: Buttons));
        }
        public static void SendMessage(long id, string message)
        {
            client.SendTextMessageAsync(id, message,Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        public static void SendMessage(long id, string message, IReplyMarkup Buttons)
        {
            client.SendTextMessageAsync(id, message, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: Buttons);
        }
        private static void LobbyHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList().Count == 0) return;
            switch (message.Text)
            {
                case "start":
                    if (lobbies.Where(x => x.admin == message.Chat.Id).ToList().Count == 0) break;
                    Lobby lobby_start = lobbies.Where(x => x.admin == message.Chat.Id).ToList()[0];

                    lobby_start.Distribution();
                    SendMessage(lobby_start, "Игра началсб!",PlayerButtons());
                    break;
                case "leave":
                    Lobby lobby = lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList()[0];
                    if (lobby.RemovePlayer(message.Chat.Id))
                    {
                        client.SendTextMessageAsync(message.Chat.Id, "Вы успешно покинули лобби!", replyMarkup: StartButtons());
                    }
                    break;
                case "delete":
                    if (lobbies.Where(x => x.admin == message.Chat.Id).ToList().Count == 0) break;
                    Lobby lobby_delete = lobbies.Where(x => x.admin == message.Chat.Id).ToList()[0];
                    lobbies.Remove(lobby_delete);
                    client.SendTextMessageAsync(message.Chat.Id, "Лобби успешно удалено!", replyMarkup: StartButtons());
                    SendMessage(lobby_delete, "Лобби расформировано!", StartButtons());
                    helpfull_list_updated = true;
                    break;
                case "info":

                    Lobby lobby_info = lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList()[0];
                    string info_message = "";
                    info_message += "Lobby ID: " + lobby_info.lobby_id + "\n";
                    info_message += "Lobby admin: " + client.GetChatAsync(lobby_info.admin).Result.FirstName + " (" + lobby_info.admin + ")\n\nMembers:\n";
                    lobby_info.players.ForEach(x =>
                    {
                        info_message += client.GetChatAsync(x.id).Result.FirstName + " (" + x.id + ")\n";
                    });
                    client.SendTextMessageAsync(message.Chat.Id, info_message);
                    break;
            }
        }

        private static void InputLobbyId(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long id;
            if (long.TryParse(message.Text, out id))
            {
                if (lobbies.Where(x => x.players.Where(y => y.id == message.Chat.Id).ToList().Count == 1).ToList().Count == 0)
                {
                    Lobby lobby = lobbies.Find(x => x.lobby_id == id);
                    if (lobby != null)
                    {
                        if (lobby.players.Where(x => x.id == message.Chat.Id).ToList().Count == 0)
                        {
                            lobby.AddPlayer(message.Chat.Id);
                            Console.WriteLine($"{message.Chat.FirstName} joined lobby {lobby.lobby_id}");
                            client.SendTextMessageAsync(message.Chat, "Вы успешно вошли в лобби!", replyMarkup: LobbyButtons());
                        }
                        else
                        {
                            client.SendTextMessageAsync(message.Chat, "Вы успешно НЕ вошли в лобби!");
                        }
                    }
                    else
                    {
                        client.SendTextMessageAsync(message.Chat, "Вы успешно НЕ вошли в лобби!");
                    }
                }
                else
                {
                    client.SendTextMessageAsync(message.Chat, "Вы уже находитесь в лобби!");
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            if (listBox1.SelectedIndex != -1)
            {
                Lobby lobby = lobbies.Where(x => x.lobby_id == uint.Parse(listBox1.SelectedItem.ToString())).ToList()[0];
                lobby.players.ForEach(x => listBox2.Items.Add(client.GetChatAsync(x.id).Result.Id + " " + client.GetChatAsync(x.id).Result.FirstName));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (helpfull_list_updated)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                lobbies.ForEach(x =>
                {
                    listBox1.Items.Add(x.lobby_id);
                });
                helpfull_list_updated = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                Lobby lobby = lobbies.Where(x => x.lobby_id == uint.Parse(listBox1.SelectedItem.ToString())).ToList()[0];
                if (listBox2.SelectedIndex != -1)
                {
                    Player player = lobby.players.Where(x => x.id == long.Parse(listBox2.SelectedItem.ToString().Split()[0])).ToList()[0];
                    client.SendTextMessageAsync(client.GetChatAsync(player.id).Result.Id, textBox1.Text);
                    return;
                }
                lobby.players.ForEach(x => client.SendTextMessageAsync(client.GetChatAsync(x.id).Result.Id, textBox1.Text));
            }
            else
            {
                PrintEveryone(textBox1.Text);
            }
        }

        private void listBox1_ControlRemoved(object sender, ControlEventArgs e)
        {
        }


    }
    /// <summary>
    /// Type of card
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// Player's professtion 
        /// </summary>
        Proffesion,
        /// <summary>
        /// Player's biology
        /// </summary>
        Biology,
        /// <summary>
        /// Players's health
        /// </summary>
        Health,
        /// <summary>
        /// Player's hobby
        /// </summary>
        Hobby,
        /// <summary>
        /// Player's bag
        /// </summary>
        Bag,
        /// <summary>
        /// Fact about player
        /// </summary>
        Fact,
        /// <summary>
        /// Special card 
        /// </summary>
        Special
    }
    public class Card
    {
        public CardType type { get; set; }
        public string text { get; set; }
        public bool isOpened;

        public Card(CardType type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }

    public class Player
    {
        public long id;
        public List<Card> cards;

        public Player(long id)
        {
            this.id = id;
            cards = new List<Card>();
        }
    }
    public class Shelter
    {

    }

    public class Disaster
    {

    }

    public class Lobby
    {
        public static Dictionary<CardType, string> cardTypes = new Dictionary<CardType, string> {
                { CardType.Proffesion, "Профессия"},
                { CardType.Biology, "Биология"},
                { CardType.Bag, "Багаж"},
                { CardType.Fact, "Факты"},
                { CardType.Health, "Здоровье"},
                { CardType.Hobby, "Хобби"},
                { CardType.Special, "Особое"},
            };
        Dictionary<CardType, List<Card>> deck;
        Shelter shelter;
        Disaster disaster;
        public long admin;
        public uint lobby_id;
        public List<Player> players = new List<Player>();


        public Lobby(long admin, uint id)
        {
            this.admin = admin;
            lobby_id = id;
            players.Add(new Player(admin));

        }
        public static List<Card> LoadCardsFromFile(string path, CardType type)
        {
            List<Card> cards = new List<Card>();
            string[] lines;
            using (StreamReader sr = new StreamReader(path))
            {
                 lines = sr.ReadToEnd().Split(new char[] { '\n' },StringSplitOptions.RemoveEmptyEntries);
            }
            foreach (var item in lines)
            {
                cards.Add(new Card(type, item));
            }
            return cards;
        }

        public void CreateDeck()
        {
            Dictionary<CardType, List<Card>> deck = new Dictionary<CardType, List<Card>>();

            foreach (var type in cardTypes.Keys)
            {
                deck.Add(type, LoadCardsFromFile($"Cards\\{cardTypes[type]}.txt", type));
            }
            this.deck = deck;
        }

        public void AddPlayer(long id)
        {
            players.Add(new Player(id));
        }

        public bool RemovePlayer(long id)
        {
            return id != admin ? players.Remove(players.Where(x => x.id == id).ToList()[0]) : false;
        }

        public void Distribution()
        {
            CreateDeck();
            players.ForEach(x =>
            {
                foreach (var cards in deck.Keys)
                {
                    List<Card> listOfCards = deck[cards];
                    int index = Form1.random.Next(0, listOfCards.Count);
                    x.cards.Add(listOfCards[index]);
                    listOfCards.RemoveAt(index);
                }
            });
        }
    }
}
