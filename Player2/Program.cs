using System;
using System.Net.Sockets;
using System.Text;

namespace Player2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BATALHA NAVAL - PLAYER 2 (CLIENTE) ===");
            Board attackBoard = new Board();

            Console.Write("Digite o IP do servidor (ou Enter para localhost): ");
            string ip = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

            Console.Write("Digite a porta do servidor (ou Enter para 5000): ");
            string portStr = Console.ReadLine();
            int port = string.IsNullOrWhiteSpace(portStr) ? 5000 : int.Parse(portStr);

            try
            {
                GameClient client = new GameClient(attackBoard);
                client.Connect(ip, port);
                client.PlayGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
                Console.ReadKey();
            }
        }
    }

    class GameClient
    {
        private readonly Board _attackBoard;
        private NetworkStream stream;
        private TcpClient client;

        public GameClient(Board attackBoard) { _attackBoard = attackBoard; }

        public void Connect(string ip, int port)
        {
            client = new TcpClient();
            Console.WriteLine($"Conectando a {ip}:{port}...");
            client.Connect(ip, port);
            stream = client.GetStream();
            Console.WriteLine("Conectado ao servidor!");
        }

        public void Send(string msg)
        {
            var data = Encoding.ASCII.GetBytes(msg);
            stream.Write(data, 0, data.Length);
        }

        public string Receive()
        {
            var buf = new byte[32];
            int len = stream.Read(buf, 0, buf.Length);
            return Encoding.ASCII.GetString(buf, 0, len);
        }

        public void PlayGame()
        {
            Console.WriteLine("\nJogo iniciado. Digite as coordenadas para atacar (ex: A5).");
            while (true)
            {
                Console.WriteLine("\nSeu tabuleiro de ataque:");
                _attackBoard.Print(false);

                Console.Write("\nDigite uma coordenada para atacar: ");
                string attack = Console.ReadLine().Trim().ToUpper();
                (int row, int col) = ParseCoordinate(attack);
                if (row < 0 || row >= 10 || col < 0 || col >= 10)
                {
                    Console.WriteLine("Coordenada inválida. Tente novamente (ex: A5)");
                    continue;
                }

                Send(attack);
                string response = Receive();
                Console.WriteLine($"Resposta: {response}");

                if (response == "HIT")
                {
                    _attackBoard.MarkHit(row, col);
                    Console.WriteLine("Você acertou um navio!");
                }
                else if (response == "MISS")
                {
                    _attackBoard.MarkMiss(row, col);
                    Console.WriteLine("Você errou.");
                }
                else if (response == "WIN")
                {
                    _attackBoard.MarkHit(row, col);
                    Console.WriteLine("PARABÉNS! Você venceu!");
                    break;
                }
                else if (response == "INVALID")
                {
                    Console.WriteLine("Coordenada inválida. Tente novamente.");
                }
            }
            Console.WriteLine("\nTabuleiro final:");
            _attackBoard.Print(false);
            Console.WriteLine("\nJogo finalizado. Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        private (int, int) ParseCoordinate(string coord)
        {
            if (coord.Length < 2) return (-1, -1);
            int row = char.ToUpper(coord[0]) - 'A';
            if (row < 0 || row >= 10) return (-1, -1);
            if (!int.TryParse(coord.Substring(1), out int col) || col < 0 || col >= 10)
                return (-1, -1);
            return (row, col);
        }
    }

    class Board
    {
        private char[,] grid = new char[10, 10];

        public Board()
        {
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    grid[r, c] = '~';
        }

        public void Print(bool showShips)
        {
            Console.Write("   ");
            for (int c = 0; c < 10; c++) Console.Write($"{c} ");
            Console.WriteLine();
            for (int r = 0; r < 10; r++)
            {
                Console.Write($"{(char)('A' + r)}  ");
                for (int c = 0; c < 10; c++)
                {
                    char cell = grid[r, c];
                    Console.Write(!showShips && cell == '*' ? "~ " : $"{cell} ");
                }
                Console.WriteLine();
            }
        }

        public void MarkHit(int r, int c) => grid[r, c] = 'X';
        public void MarkMiss(int r, int c) => grid[r, c] = 'O';
    }
}
