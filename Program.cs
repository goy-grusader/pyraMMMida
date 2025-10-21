
using System;
using System.Collections.Generic;
using System.Linq;

namespace PyramidGame
{
    // Перечисление для статуса игрока
    public enum PlayerStatus
    {
        Active,
        Left,
        Angry
    }

    // Запись для действия
    public record GameAction(string Description, decimal Amount, DateTime Time)
    {
        public override string ToString() => $"[{Time:HH:mm}] {Description}";
    }

    // Класс игрока
    public class Player
    {
        public string Name { get; }
        public decimal Money { get; set; }
        public int Level { get; set; }
        public PlayerStatus Status { get; set; }
        public int InvitedCount { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalReceived { get; set; }

        public Player(string name, decimal initialMoney)
        {
            Name = name;
            Money = initialMoney;
            Level = 1;
            Status = PlayerStatus.Active;
            TotalInvested = initialMoney;
        }

        public bool CanInvest(decimal amount) => Money >= amount;

        public void Invest(decimal amount)
        {
            Money -= amount;
            TotalInvested += amount;
        }

        public void Receive(decimal amount)
        {
            Money += amount;
            TotalReceived += amount;
        }

        public override string ToString() => $"{Name} (${Money})";
    }

    // Основная игра
    public class PyramidGame
    {
        private List<Player> _players;
        private Player _organizer;
        private List<GameAction> _actions;
        private Random _random;
        private int _round;
        private decimal _pyramidMoney;

        public PyramidGame(string organizerName)
        {
            _organizer = new Player(organizerName, 1000);
            _players = new List<Player>();
            _actions = new List<GameAction>();
            _random = new Random();
            _round = 1;
            _pyramidMoney = 0;
        }

        public void StartGame()
        {
            Console.WriteLine(" ФИНАНСОВАЯ ПИРАМИДА - ВЫ РУКОВОДИТЕ ПИРАМИДОЙ!");
            Console.WriteLine($"Вы: {_organizer.Name} (Организатор)");
            Console.WriteLine("Ваша задача: решать, кому платить, а кого кинуть!\n");

            // Добавляем первых участников
            AddPlayer("Алексей", 200);
            AddPlayer("Мария", 150);
            AddPlayer("Дмитрий", 300);
            AddPlayer("Сергей", 250);

            _pyramidMoney = _players.Sum(p => p.TotalInvested);
        }

        public void PlayRound()
        {
            Console.WriteLine($"\n=== РАУНД {_round} ===");
            Console.WriteLine($"Деньги в пирамиде: {_pyramidMoney:C}");

            // Новые участники
            if (_random.Next(0, 100) < 60)
            {
                var newPlayerName = GetRandomName();
                var investment = _random.Next(100, 301);
                AddPlayer(newPlayerName, investment);
                _pyramidMoney += investment;
            }

            // Требуют выплаты
            var demandingPlayers = _players
                .Where(p => p.Status == PlayerStatus.Active && p.TotalReceived < p.TotalInvested * 1.5m)
                .OrderBy(_ => _random.Next())
                .Take(2)
                .ToList();

            if (demandingPlayers.Any())
            {
                Console.WriteLine("\n Участники требуют выплат:");
                for (int i = 0; i < demandingPlayers.Count; i++)
                {
                    var player = demandingPlayers[i];
                    var expectedPayment = player.TotalInvested * 0.3m;
                    Console.WriteLine($"{i + 1}. {player.Name} ожидает {expectedPayment:C} (вложил {player.TotalInvested:C})");
                }


Console.Write("\n Кому ВЫПЛАТИТЬ? (введите номер, 0 - никому): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= demandingPlayers.Count)
                {
                    var luckyPlayer = demandingPlayers[choice - 1];
                    var payment = luckyPlayer.TotalInvested * 0.3m;

                    if (_pyramidMoney >= payment)
                    {
                        luckyPlayer.Receive(payment);
                        _pyramidMoney -= payment;
                        _actions.Add(new GameAction($"Выплачено {payment:C} {luckyPlayer.Name}", payment, DateTime.Now));
                        Console.WriteLine($" {luckyPlayer.Name} получил выплату!");
                    }
                    else
                    {
                        Console.WriteLine(" В пирамиде недостаточно денег для выплаты!");
                    }
                }
                else
                {
                    Console.WriteLine(" Никому не выплачено!");
                    // Недовольные участники могут уйти
                    foreach (var player in demandingPlayers)
                    {
                        if (_random.Next(0, 100) < 40)
                        {
                            player.Status = PlayerStatus.Angry;
                            _actions.Add(new GameAction($"{player.Name} ушел из-за отсутствия выплат", 0, DateTime.Now));
                            Console.WriteLine($" {player.Name} разозлился и ушел!");
                        }
                    }
                }
            }

            // Приглашения новых участников
            var activePlayers = _players.Where(p => p.Status == PlayerStatus.Active).ToList();
            foreach (var player in activePlayers)
            {
                if (_random.Next(0, 100) < 30 && player.Money > 50)
                {
                    var newPlayerName = GetRandomName();
                    var investment = _random.Next(50, 201);

                    Console.WriteLine($"\n{player.Name} хочет пригласить {newPlayerName} с взносом {investment:C}");
                    Console.Write("Разрешить? (y/n): ");
                    var response = Console.ReadLine()?.ToLower();

                    if (response == "y")
                    {
                        AddPlayer(newPlayerName, investment);
                        _pyramidMoney += investment;

                        // Бонус пригласившему
                        var bonus = investment * 0.2m;
                        player.Receive(bonus);
                        player.InvitedCount++;
                        _actions.Add(new GameAction($"{player.Name} получил бонус {bonus:C} за приглашение", bonus, DateTime.Now));
                        Console.WriteLine($" {newPlayerName} присоединился!");
                    }
                    else
                    {
                        Console.WriteLine($" Вы запретили приглашение");
                    }
                }
            }

            // Специальное событие
            if (_random.Next(0, 100) < 25)
            {
                SpecialEvent();
            }

            PrintStatus();
            _round++;
        }

        private void SpecialEvent()
        {
            var events = new[]
            {
                " В СМИ вышла статья о вашей пирамиде! Репутация улучшилась",
                " Полиция начала проверку! Часть участников напугана",
                " Участник сделал крупное вложение!",
                " Кризис доверия! Часть участников хочет забрать деньги"
            };

            var randomEvent = events[_random.Next(events.Length)];
            Console.WriteLine($"\n СОБЫТИЕ: {randomEvent}");


switch (randomEvent)
            {
                case var s when s.Contains("СМИ"):
                    // Добавляем больше участников
                    for (int i = 0; i < 2; i++)
                    {
                        var name = GetRandomName();
                        var money = _random.Next(200, 501);
                        AddPlayer(name, money);
                        _pyramidMoney += money;
                    }
                    break;

                case var s when s.Contains("Полиция"):
                    // Некоторые участники уходят
                    var scaredPlayers = _players.Where(p => p.Status == PlayerStatus.Active)
                                               .OrderBy(_ => _random.Next())
                                               .Take(2)
                                               .ToList();
                    foreach (var player in scaredPlayers)
                    {
                        player.Status = PlayerStatus.Left;
                        Console.WriteLine($" {player.Name} испугался и ушел");
                    }
                    break;

                case var s when s.Contains("крупное"):
                    var bigInvestment = _random.Next(500, 1001);
                    _pyramidMoney += bigInvestment;
                    Console.WriteLine($" Кто-то анонимно внес {bigInvestment:C}!");
                    break;

                case var s when s.Contains("кризис"):
                    var worriedPlayers = _players.Where(p => p.Status == PlayerStatus.Active)
                                                .OrderBy(_ => _random.Next())
                                                .Take(3)
                                                .ToList();
                    Console.WriteLine("Эти участники требуют свои деньги назад:");
                    foreach (var player in worriedPlayers)
                    {
                        Console.WriteLine($"- {player.Name} (вложил {player.TotalInvested:C})");
                    }

                    Console.Write("Вернуть деньги всем? (y/n): ");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        foreach (var player in worriedPlayers)
                        {
                            var returnAmount = player.TotalInvested * 0.5m;
                            if (_pyramidMoney >= returnAmount)
                            {
                                player.Receive(returnAmount);
                                _pyramidMoney -= returnAmount;
                                Console.WriteLine($"{player.Name} получил назад {returnAmount:C}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(" Вы отказались возвращать деньги");
                        foreach (var player in worriedPlayers)
                        {
                            if (_random.Next(0, 100) < 60)
                            {
                                player.Status = PlayerStatus.Angry;
                                Console.WriteLine($" {player.Name} разозлился!");
                            }
                        }
                    }
                    break;
            }
        }

        private void AddPlayer(string name, decimal money)
        {
            var player = new Player(name, money);
            _players.Add(player);
            _actions.Add(new GameAction($"{name} присоединился с {money:C}", money, DateTime.Now));
        }

        private string GetRandomName()
        {
            var names = new[] { "Анна", "Иван", "Ольга", "Михаил", "Елена", "Андрей", "Наталья", "Павел", "Юлия", "Владимир" };
            return names[_random.Next(names.Length)] + (_players.Count + 1);
        }


public void PrintStatus()
        {
            var activePlayers = _players.Where(p => p.Status == PlayerStatus.Active).ToList();
            var leftPlayers = _players.Where(p => p.Status != PlayerStatus.Active).ToList();

            Console.WriteLine($"\nСТАТУС ПИРАМИДЫ:");
            Console.WriteLine($" Деньги в системе: {_pyramidMoney:C}");
            Console.WriteLine($" Активных участников: {activePlayers.Count}");
            Console.WriteLine($" Вышедших: {leftPlayers.Count}");

            if (activePlayers.Any())
            {
                Console.WriteLine("\n Топ участников:");
                var topPlayers = activePlayers.OrderByDescending(p => p.Money).Take(3);
                foreach (var player in topPlayers)
                {
                    Console.WriteLine($"  {player.Name}: {player.Money:C} (вложил: {player.TotalInvested:C})");
                }
            }
        }

        public void PrintHistory()
        {
            Console.WriteLine("\n📜 ИСТОРИЯ ДЕЙСТВИЙ:");
            foreach (var action in _actions.TakeLast(10))
            {
                Console.WriteLine(action);
            }
        }

        public bool IsGameOver()
        {
            var activePlayers = _players.Count(p => p.Status == PlayerStatus.Active);
            return _pyramidMoney <= 0  ;
        }

        public void PrintGameResult()
        {
            Console.WriteLine("\n ИГРА ОКОНЧЕНА!");
            Console.WriteLine($" Ваша прибыль: {_organizer.Money - 1000:C}");
            Console.WriteLine($" Максимум участников: {_players.Count}");
            Console.WriteLine($" Самая крупная пирамида: {_pyramidMoney:C}");

            var mostSuccessful = _players.OrderByDescending(p => p.Money).FirstOrDefault();
            if (mostSuccessful != null)
            {
                Console.WriteLine($" Самый успешный участник: {mostSuccessful.Name} с {mostSuccessful.Money:C}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите ваше имя как организатора пирамиды:");
            var name = Console.ReadLine() ?? "Организатор";

            var game = new PyramidGame(name);
            game.StartGame();

            // Играем пока не закончится
            while (!game.IsGameOver())
            {
                game.PlayRound();
                game.PrintHistory();

                Console.WriteLine("\nНажмите Enter для следующего раунда...");
                Console.ReadLine();
            }

            game.PrintGameResult();
            Console.WriteLine("\nСпасибо за игру!");
        }
    }
}


