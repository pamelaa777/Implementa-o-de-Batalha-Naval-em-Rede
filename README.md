# Implementa-o-de-Batalha-Naval-em-Rede
Exercício: Implementação de “Batalha Naval” em C# com Comunicação em Rede
Objetivo Desenvolver dois programas de Console Application em C# que se comuniquem via TCP para simular um jogo de Batalha Naval em um tabuleiro 10×10, com 10 navios de tamanho 1 posicionados pelo Servidor (Player 1) e ataques enviados pelo Cliente (Player 2).

1. Preparação do Ambiente
1.    Crie duas pastas no seu diretório de trabalho: Player1 e Player2.
2.    Inicialize em cada uma um projeto de console:
cd Player1
dotnet new console
cd ../Player2
dotnet new console
3.    Abra o workspace no VSCode (code .) e verifique se consegue executar (dotnet run) em cada projeto.
2. Requisitos Funcionais
2.1. Player 1 (Servidor)
•      Posicionamento de Navios
–     Ao iniciar, exibir menu para escolha de modo:
1.    Posicionamento Aleatório de 10 navios (“*”).
2.    Posicionamento Manual de 10 coordenadas (ex.: A5, J0).
–     Após posicionar, imprimir o tabuleiro no console mostrando onde estão os navios (*).
•      Comunicação
–     Abrir um listener TCP (porta à sua escolha) e aguardar a conexão do Player 2.

public void StartServer(int port)
{
    var listener = new TcpListener(IPAddress.Any, port);
    listener.Start();
    Console.WriteLine($"Aguardando Player2 na porta {port}...");
    var client = listener.AcceptTcpClient();
    stream = client.GetStream();
    Console.WriteLine("Player2 conectado!");
}

–     Em loop, receber uma string de ataque (p. ex. "B7"), converter em índices linha/coluna, verificar se há navio, marcar no tabuleiro X em caso de acerto e enviar de volta um dos sinais:
•      "HIT"
•      "MISS"
•      "WIN" (quando todos os 10 navios tiverem sido afundados).
–     Reimprimir o tabuleiro após cada ataque.

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

2.2. Player 2 (Cliente)
•      Inicialização
–     Conectar ao IP/porta do Player 1 via TCP.
•      Loop de Ataques
–     Exibir próprio tabuleiro de ataque (em branco no início).
–     Ler coordenada do usuário, enviá-la ao servidor, aguardar resposta.
–     Marcar no tabuleiro de ataque: X para "HIT", O para "MISS".
–     Caso receba "WIN", declarar vitória e encerrar.
3. Dicas de Implementação
1.    Classe Board
–     Internamente use char[10,10]; inicializar com '~'.
–     Métodos sugeridos:
•      Print(bool showShips) – exibe o tabuleiro;
•      PlaceShipsRandomly(int count);
•      PlaceShipsManually(int count);
•      IsShip(int r, int c), MarkHit(int r, int c), MarkMiss(int r, int c), AreAllShipsSunk().


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
            Console.Write(!showShips && cell=='*' ? "~ " : $"{cell} ");
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
        if (grid[r,c] == '~')
        {
            grid[r,c] = '*';
            placed++;
        }
    }
}

2.    Parsing de Coordenadas
–     Mapear 'A'–'J' para 0–9 para linhas e converter substring numérica para coluna.
3.    Comunicação TCP
–     No Player 1, use TcpListener para ouvir; em AcceptTcpClient() obtenha um NetworkStream.
–     No Player 2, use TcpClient.Connect(host, port) e obtenha NetworkStream.
–     Envie/receba strings simples (ASCII ou UTF8); cuide para ler exatamente até o fim da mensagem.
4.    Fluxo de Turnos
–     Defina quem começa (servidor inicia o loop de recebimento).
–     Sempre que um jogador ataca, bloqueie até receber a resposta antes de alternar.
5.    Validação e Tratamento de Erros
–     Verifique formatos de entrada (tamanho mínimo de string, letra válida, número no intervalo).
–     Garantir que não se posicione dois navios na mesma célula.
–     Trate desconexões inesperadas de forma elegante (exibir mensagem e encerrar).
6.    Testes Locais
–     Abra dois terminais: em um, dotnet run em Player1; no outro, em Player2.
–     Garanta que a conexão seja estabelecida e que ataques reflitam corretamente no tabuleiro do servidor.