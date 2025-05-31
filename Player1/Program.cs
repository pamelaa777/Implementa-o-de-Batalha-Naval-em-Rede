using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Player1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BATALHA NAVAL - PLAYER 1 (SERVIDOR) ===");
            Board board = new Board();

            Console.WriteLine("Escolha o modo de posicionamento dos navios:");
            Console.WriteLine("1. Aleatório");
            Console.WriteLine("2. Manual");
            int op;
            while (!int.TryParse(Console.ReadLine(), out op) || (op != 1 && op != 2))
                Console.WriteLine("Digite 1 ou 2:");

            if (op == 1)
                board.PlaceShipsRandomly(10);
            else
                board.PlaceShipsManually(10);

            Console.WriteLine("\nTabuleiro com navios:");
            board.Print(true);

            GameServer server = new GameServer(board);
            server.StartServer(5000);
            server.PlayGame();
        }
    }

    class GameServer
    {
        private readonly Board _board;
        private NetworkStream stream;

        public GameServer(Board board) { _board = board; }

        public void StartServer(int port)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Aguardando Player2 na porta {port}...");
            var client = listener.AcceptTcpClient();
            stream = client.GetStream();
            Console.WriteLine("Player2 conectado!");
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
            while (true)
            {
                string attack = Receive();
                Console.WriteLine($"Player2 atacou: {attack}");
                (int row, int col) = ParseCoordinate(attack);

                string response;
                if (row < 0 || row >= 10 || col < 0 || col >= 10)
                    response = "INVALID";
                else if (_board.IsShip(row, col))
                {
                    _board.MarkHit(row, col);
                    response = _board.AreAllShipsSunk() ? "WIN" : "HIT";
                }
                else
                {
                    _board.MarkMiss(row, col);
                    response = "MISS";
                }

                Console.WriteLine("\nTabuleiro atual:");
                _board.Print(true);
                Send(response);

                if (response == "WIN")
                {
                    Console.WriteLine("Player2 venceu! Todos os navios foram afundados.");
                    break;
                }
            }
            Console.WriteLine("Jogo finalizado. Pressione qualquer tecla para sair...");
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

        public void PlaceShipsRandomly(int n)
        {
            var rnd = new Random();
            int placed = 0;
            while (placed < n)
            {
                int r = rnd.Next(10), c = rnd.Next(10);
                if (grid[r, c] == '~')
                {
                    grid[r, c] = '*';
                    placed++;
                }
            }
        }

        public void PlaceShipsManually(int n)
        {
            int placed = 0;
            while (placed < n)
            {
                string input = Console.ReadLine().Trim().ToUpper();
                (int row, int col) = ParseCoordinate(input);
                if (row < 0 || row >= 10 || col < 0 || col >= 10)
                {
                    Console.WriteLine("Coordenada inválida. Tente novamente (ex: A5):");
                    continue;
                }
                if (grid[row, col] == '*')
                {
                    Console.WriteLine("Já existe um navio nessa posição. Tente novamente:");
                    continue;
                }
                grid[row, col] = '*';
                placed++;
                Console.WriteLine($"Navio {placed}/10 posicionado em {input}");
            }
        }

        public bool IsShip(int r, int c) => grid[r, c] == '*';
        public void MarkHit(int r, int c) => grid[r, c] = 'X';
        public void MarkMiss(int r, int c) => grid[r, c] = 'O';
        public bool AreAllShipsSunk()
        {
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    if (grid[r, c] == '*')
                        return false;
            return true;
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
}
