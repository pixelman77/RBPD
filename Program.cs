
using System.Runtime.InteropServices;

public enum Tile : int
{
    Empty, Red, Blue
}
public class Board
{
    public int redOut = 0;
    public int blueOut = 0;
    public int redPiece = 0;
    public int bluePiece = 0;
    public Tile[][] Tiles = new Tile[8][];
    public int[] lastMove = new int[6];
    public bool justRemoved = false;
    public bool nowRemoveable = false;
    public Board()
    {
        for (int a = 0; a < 8; a++)
        {
            Tiles[a] = new Tile[3];
            for (int r = 0; r < 3; r++)
            {
                Tiles[a][r] = Tile.Empty;
            }
        }
    }

    public List<int[]> GetConnections(int arm, int reach)
    {
        List<int[]> Connections = new List<int[]>();
        if (arm == 0)
        {
            Connections.Add(new int[] { 7, reach });
            Connections.Add(new int[] { 1, reach });
        }
        else if (arm == 7)
        {
            Connections.Add(new int[] { 6, reach });
            Connections.Add(new int[] { 0, reach });
        }
        else
        {
            Connections.Add(new int[] { (int)(arm - 1), reach });
            Connections.Add(new int[] { (int)(arm + 1), reach });
        }

        if (reach == 1)
        {
            Connections.Add(new int[] { arm, 0 });
            Connections.Add(new int[] { arm, 2 });
        }
        else
        {
            Connections.Add(new int[] { arm, 1 });
        }

        return Connections;
    }


    //{player(int), origin arm, origin reach, dest arm, dest reach, isRemove}
    //origin is -1 if still deploying
    public int[][] GetValidMoves(Tile player, int arm, int reach)
    {


        List<int[]> Moves = new List<int[]>();

        if ((player == Tile.Red && (redOut > 0)) || (player == Tile.Blue && (blueOut > 0)))
        {
            for (int a = 0; a < 8; a++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (Tiles[a][r] == Tile.Empty)
                    {
                        Moves.Add(new int[] { (int)player, -1, -1, a, r, 0 });
                    }
                }
            }
            return Moves.ToArray();
        }

        if ((player == Tile.Red && redOut == 0 && redPiece == 3) || (player == Tile.Blue && blueOut == 0 && bluePiece == 3))
        {
            for (int a = 0; a < 8; a++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (Tiles[a][r] == Tile.Empty)
                    {
                        Moves.Add(new int[] { (int)player, arm, reach, a, r, 0 });
                    }
                }
            }
            return Moves.ToArray();
        }



        //hacky solution
        int[][] Connections = GetConnections(arm, reach).ToArray();
        for (int i = 0; i < Connections.Count(); i++)
        {
            if (Tiles[Connections[i][0]][Connections[i][1]] == Tile.Empty)
            {
                Moves.Add(new int[] { (int)player, arm, reach, Connections[i][0], Connections[i][1], 0 });
            }
        }
        return Moves.ToArray();
    }

    public int[][] GetAllValidMoves(Tile player)
    {
        List<int[]> allMoves = new List<int[]>();
        int[][] holder;
        if ((player == Tile.Red && redOut > 0) || (player == Tile.Blue && blueOut > 0))
        {
            holder = GetValidMoves(player, -1, -1);
            foreach (int[] i in holder)
            {
                allMoves.Add(i);
            }
            return allMoves.ToArray();
        }

        for (int a = 0; a < 8; a++)
        {
            for (int r = 0; r < 3; r++)
            {
                if (Tiles[a][r] == player)
                {
                    holder = GetValidMoves(player, a, r);
                    foreach (int[] i in holder)
                    {
                        allMoves.Add(i);
                    }
                }
            }
        }
        return allMoves.ToArray();

    }

    public Board MakeMove(int[] moveArr)
    {

        Tile player = (Tile)moveArr[0];
        int oArm = moveArr[1];
        int oReach = moveArr[2];
        int deArm = moveArr[3];
        int deReach = moveArr[4];


        //prepping the new board
        Board neoBoard = new Board();
        for (int a = 0; a < 8; a++)
        {
            for (int r = 0; r < 3; r++)
            {
                neoBoard.Tiles[a][r] = this.Tiles[a][r];
            }
        }
        neoBoard.blueOut = blueOut;
        neoBoard.bluePiece = bluePiece;
        neoBoard.redOut = redOut;
        neoBoard.redPiece = redPiece;


        neoBoard.justRemoved = false;
        neoBoard.nowRemoveable = false;

        moveArr.CopyTo(neoBoard.lastMove, 0);
        neoBoard.Tiles[deArm][deReach] = (Tile)moveArr[0];
        if (moveArr[1] != -1)
        {
            neoBoard.Tiles[oArm][oReach] = Tile.Empty;
        }
        else
        {
            if ((Tile)moveArr[0] == Tile.Red)
            {
                neoBoard.redOut--;
                neoBoard.redPiece++;
            }
            else
            {
                neoBoard.blueOut--;
                neoBoard.bluePiece++;
            }
        }

        if (neoBoard.IsMoveDooz(neoBoard.lastMove))
        {
            if ((Tile)neoBoard.lastMove[0] == Tile.Red)
            {
                if (neoBoard.blueOut > 0)
                {
                    neoBoard.blueOut--;
                    neoBoard.justRemoved = true;
                }
                neoBoard.nowRemoveable = true;
            }
            else
            {
                if (neoBoard.redOut > 0)
                {
                    neoBoard.redOut--;
                    neoBoard.justRemoved = true;
                }
                neoBoard.nowRemoveable = true;
            }
        }


        return neoBoard;

    }

    public bool IsMoveDooz(int[] move)
    {
        int arm = move[3];
        int reach = move[4];

        if (Tiles[arm][0] == Tiles[arm][1] && Tiles[arm][1] == Tiles[arm][2]) { return true; }
        if (arm == 6)
        {
            if (Tiles[4][reach] == Tiles[5][reach] && Tiles[5][reach] == Tiles[6][reach]) { return true; }
        }
        if (arm == 0 || arm == 7 || arm == 6)
        {
            if (Tiles[6][reach] == Tiles[7][reach] && Tiles[7][reach] == Tiles[0][reach]) { return true; }
        }
        else if (arm % 2 == 0)
        {
            if (Tiles[arm - 2][reach] == Tiles[arm - 1][reach] && Tiles[arm - 1][reach] == Tiles[arm][reach]) { return true; }
            if (Tiles[arm][reach] == Tiles[arm + 1][reach] && Tiles[arm + 1][reach] == Tiles[arm + 2][reach]) { return true; }
        }
        else
        {
            if (Tiles[arm - 1][reach] == Tiles[arm][reach] && Tiles[arm][reach] == Tiles[arm + 1][reach]) { return true; }
        }
        return false;
    }


    //player signifies the REMOVING player
    //this could've been integrated with GetMoves but the loose option bool will cause trouble
    public int[][] GetRemovablePieces(Tile player, bool isDoozRemovingAllowed)
    {
        List<int[]> removables = new List<int[]>();
        Tile targetPlayer;
        if (player == Tile.Red)
        {
            targetPlayer = Tile.Blue;
        }
        else
        {
            targetPlayer = Tile.Red;
        }

        for (int a = 0; a < 8; a++)
        {
            for (int r = 0; r < 3; r++)
            {
                if (Tiles[a][r] == targetPlayer)
                {
                    if (!isDoozRemovingAllowed)
                    {
                        if (IsMoveDooz(new int[] { (int)targetPlayer, -1, -1, a, r, 0 }))
                        {
                            continue;
                        }
                    }
                    removables.Add(new int[] { (int)player, -1, -1, a, r, 1 });
                }
            }
        }

        return removables.ToArray();
    }

    public Board RemovePiece(int arm, int reach)
    {
        Board neoBoard = new Board();
        neoBoard.nowRemoveable = false;
        neoBoard.justRemoved = true;
        for (int a = 0; a < 8; a++)
        {
            for (int r = 0; r < 3; r++)
            {
                neoBoard.Tiles[a][r] = this.Tiles[a][r];
            }
        }
        neoBoard.blueOut = blueOut;
        neoBoard.bluePiece = bluePiece;
        neoBoard.redOut = redOut;
        neoBoard.redPiece = redPiece;



        if (neoBoard.Tiles[arm][reach] == Tile.Red)
        {
            neoBoard.redPiece--;
        }
        else
        {
            neoBoard.bluePiece--;
        }
        neoBoard.Tiles[arm][reach] = Tile.Empty;
        neoBoard.lastMove = new int[] { lastMove[0], -1, -1, arm, reach, 1 };



        return neoBoard;
    }

}
public class Master
{
    public bool botMoved = false;
    public int searchDepth = 8;
    public int botCrossArmScore = 0;
    public int botXArmScore = 0;
    public int botCornerScore = 0;
    public int botEdgeScore = 0;
    public int botCoreScore = 0;
    public int botMidReachScore = 0;
    public int botDoozScore = 30;
    public int botWinScore = 50;
    public bool isDoozRemovingAllowed = false;
    public Board mainBoard = new Board();
    public int pieceAmount = 9;

    public void PrepareBoard()
    {
        mainBoard = new Board();
        for (int a = 0; a < 8; a++)
        {
            for (int r = 0; r < 3; r++)
            {
                mainBoard.Tiles[a][r] = Tile.Empty;
            }
        }
        mainBoard.redOut = pieceAmount;
        mainBoard.blueOut = pieceAmount;
    }



    public void ThinkNGo(Tile botColor)
    {
        int[][] allMoves = mainBoard.GetAllValidMoves(botColor);
        int bestScore = -999;
        int[] bestMove = allMoves[0];
        int temp;

        foreach (int[] move in allMoves)
        {
            temp = Minimax(mainBoard.MakeMove(move), searchDepth, +999, -999, false);
            if (botColor == Tile.Blue)
            {
                temp = -temp;
            }

            if (temp > bestScore)
            {
                bestMove = move;
                bestScore = temp;
            }
        }

        Console.WriteLine();
        if (bestMove[1] == -1)
        {
            Console.WriteLine("Bot places on " + bestMove[3].ToString() + bestMove[4].ToString());
        }
        else
        {
            Console.WriteLine("Bot moves from " + bestMove[1].ToString() + bestMove[2].ToString() + " to " + bestMove[3].ToString() + bestMove[4].ToString());
        }
        mainBoard = mainBoard.MakeMove(bestMove);

        if (mainBoard.IsMoveDooz(mainBoard.lastMove))
        {
            Console.WriteLine("DOOZ");
        }
        if (mainBoard.IsMoveDooz(mainBoard.lastMove) && !mainBoard.justRemoved)
        {
            allMoves = mainBoard.GetRemovablePieces(botColor, isDoozRemovingAllowed);
            bestScore = -999;
            bestMove = allMoves[0];

            foreach (int[] move in allMoves)
            {
                temp = Minimax(mainBoard.RemovePiece(move[3], move[4]), searchDepth, +999, -999, false);
                if (botColor == Tile.Blue)
                {
                    temp = -temp;
                }

                if (temp > bestScore)
                {
                    bestMove = move;
                    bestScore = temp;
                }
            }

            Console.WriteLine("Bot removes " + bestMove[3].ToString() + bestMove[4].ToString());
            mainBoard = mainBoard.RemovePiece(bestMove[3], bestMove[4]);
        }

        botMoved = true;
    }



    public bool IsMoveDooz(int[] move, Board board)
    {
        Tile[][] Tiles = board.Tiles;
        int arm = move[3];
        int reach = move[4];

        if (Tiles[arm][0] == Tiles[arm][1] && Tiles[arm][1] == Tiles[arm][2]) { return true; }
        if (arm == 6)
        {
            if (Tiles[4][reach] == Tiles[5][reach] && Tiles[5][reach] == Tiles[6][reach]) { return true; }
        }
        if (arm == 0 || arm == 7 || arm == 6)
        {
            if (Tiles[6][reach] == Tiles[7][reach] && Tiles[7][reach] == Tiles[0][reach]) { return true; }
        }
        else if (arm % 2 == 0)
        {
            if (Tiles[arm - 2][reach] == Tiles[arm - 1][reach] && Tiles[arm - 1][reach] == Tiles[arm][reach]) { return true; }
            if (Tiles[arm][reach] == Tiles[arm + 1][reach] && Tiles[arm + 1][reach] == Tiles[arm + 2][reach]) { return true; }
        }
        else
        {
            if (Tiles[arm - 1][reach] == Tiles[arm][reach] && Tiles[arm][reach] == Tiles[arm + 1][reach]) { return true; }
        }
        return false;
    }


    //RED player is positive
    //BLUE player is negative
    //bot itself will handle the advantage logic
    public int GetScore(Board board)
    {
        int score = 0;
        if (IsMoveDooz(board.lastMove, board))
        {
            if ((Tile)board.lastMove[0] == Tile.Red)
            {
                score += botDoozScore;
            }
            else
            {
                score -= botDoozScore;
            }

        }

        if((Tile)board.lastMove[0] == Tile.Red && board.GetAllValidMoves(Tile.Blue).Length < 1){
            score += botWinScore;
        }
        else if((Tile)board.lastMove[0] == Tile.Blue && board.GetAllValidMoves(Tile.Red).Length < 1){
            score -= botWinScore;
        }

        if (board.blueOut + board.bluePiece < 3)
        {
            score += botWinScore;
        }
        else if (board.redOut + board.redPiece < 3)
        {
            score -= botWinScore;
        }

        //score calculating logic to be written here after 8000 years probably

        return score;
    }

    public bool IsGameDone(Board board)
    {
        Tile lastPlayer = (Tile)board.lastMove[0];
        if(board.GetAllValidMoves(Tile.Red).Length < 1 || board.GetAllValidMoves(Tile.Blue).Length < 1){
            return true;
        }
        if ((board.redOut + board.redPiece) == 2 || (board.blueOut + board.bluePiece) == 2)
        {
            return true;
        }
        return false;
    }

    public int Minimax(Board board, int depth, int alpha, int beta, bool maximizer)
    {
        if (depth < 1 || IsGameDone(board))
        {
            return GetScore(board);
        }


        Tile lastPlayer = (Tile)board.lastMove[0];
        Tile notlastPlayer;
        int bestVal = 0;
        int temp;
        if (lastPlayer == Tile.Red)
        {
            notlastPlayer = Tile.Blue;
        }
        else
        {
            notlastPlayer = Tile.Red;
        }

        //double turn
        if (board.nowRemoveable)
        {
            maximizer = !maximizer;
        }

        int[][] allMoves;
        if (board.nowRemoveable)
        {
            allMoves = board.GetRemovablePieces(lastPlayer, isDoozRemovingAllowed);
        }
        else
        {
            allMoves = board.GetAllValidMoves(notlastPlayer);
        }

        //piece removal
        if (board.nowRemoveable)
        {
            if (maximizer)
            {
                bestVal = -999;
                foreach (int[] move in allMoves)
                {
                    temp = Minimax(board.RemovePiece(move[3], move[4]), depth - 1, alpha, beta, false);
                    bestVal = Math.Max(temp, bestVal);
                    alpha = Math.Max(alpha, bestVal);

                }
                return bestVal + GetScore(board);

            }
            else
            {
                bestVal = 999;
                foreach (int[] move in allMoves)
                {
                    temp = Minimax(board.RemovePiece(move[3], move[4]), depth - 1, alpha, beta, true);
                    bestVal = Math.Min(bestVal, temp);
                    beta = Math.Min(beta, bestVal);

                }

                return bestVal + GetScore(board);

            }
        }

        //making moves
        if (maximizer)
        {
            bestVal = -999;
            foreach (int[] move in allMoves)
            {
                temp = Minimax(board.MakeMove(move), depth - 1, alpha, beta, false);
                bestVal = Math.Max(temp, bestVal);
                alpha = Math.Max(alpha, bestVal);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return bestVal + GetScore(board);

        }
        else
        {
            bestVal = 999;
            foreach (int[] move in allMoves)
            {
                temp = Minimax(board.MakeMove(move), depth - 1, alpha, beta, true);
                bestVal = Math.Min(bestVal, temp);
                beta = Math.Min(beta, bestVal);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return bestVal + GetScore(board);

        }

    }

    public Char Tile2Char(Tile tile)
    {
        switch (tile)
        {
            case Tile.Empty:
                return ' ';
            case Tile.Red:
                return 'O';
            default:
                return 'X';
        }
    }

    public void PrintBoard()
    {
        Console.WriteLine("O out: " + mainBoard.redOut + "  X out: " + mainBoard.blueOut);
        Console.WriteLine("O: " + mainBoard.redPiece + "  X: " + mainBoard.bluePiece);

        Console.Write("[" + Tile2Char(mainBoard.Tiles[0][2]) + "]      [" + Tile2Char(mainBoard.Tiles[1][2]) + "]      [" + Tile2Char(mainBoard.Tiles[2][2]) + "]");
        Console.WriteLine("   [02]      [12]      [22]");

        Console.Write("   [" + Tile2Char(mainBoard.Tiles[0][1]) + "]   [" + Tile2Char(mainBoard.Tiles[1][1]) + "]   [" + Tile2Char(mainBoard.Tiles[2][1]) + "]   ");
        Console.WriteLine("      [01]   [11]   [21]   ");

        Console.Write("      [" + Tile2Char(mainBoard.Tiles[0][0]) + "][" + Tile2Char(mainBoard.Tiles[1][0]) + "][" + Tile2Char(mainBoard.Tiles[2][0]) + "]      ");
        Console.WriteLine("         [00][10][20]      ");

        Console.Write("[" + Tile2Char(mainBoard.Tiles[7][2]) + "][" + Tile2Char(mainBoard.Tiles[7][1]) + "][" + Tile2Char(mainBoard.Tiles[7][0]) + "]   ");
        Console.Write("[" + Tile2Char(mainBoard.Tiles[3][0]) + "][" + Tile2Char(mainBoard.Tiles[3][1]) + "][" + Tile2Char(mainBoard.Tiles[3][2]) + "]");
        Console.WriteLine(" [72][71][70]    [30][31][32]");

        Console.Write("      [" + Tile2Char(mainBoard.Tiles[6][0]) + "][" + Tile2Char(mainBoard.Tiles[5][0]) + "][" + Tile2Char(mainBoard.Tiles[4][0]) + "]      ");
        Console.WriteLine("         [60][50][40]     ");

        Console.Write("   [" + Tile2Char(mainBoard.Tiles[6][1]) + "]   [" + Tile2Char(mainBoard.Tiles[5][1]) + "]   [" + Tile2Char(mainBoard.Tiles[4][1]) + "]   ");
        Console.WriteLine("      [61]   [51]   [41]  ");

        Console.Write("[" + Tile2Char(mainBoard.Tiles[6][2]) + "]      [" + Tile2Char(mainBoard.Tiles[5][2]) + "]      [" + Tile2Char(mainBoard.Tiles[4][2]) + "]");
        Console.WriteLine("   [62]      [52]      [42]");

    }

    public void PlayerTurn(Tile playerColor)
    {
        bool moved = false;
        bool placing = false;
        bool doozed = false;
        bool removed = false;
        if ((playerColor == Tile.Red && mainBoard.redOut > 0) || (playerColor == Tile.Blue && mainBoard.blueOut > 0))
        {
            placing = true;
        }

        PrintBoard();
        Console.WriteLine();
        int[][] allMoves = mainBoard.GetAllValidMoves(playerColor);

        while (!moved)
        {
            if (placing)
            {

                Console.WriteLine("Your placements:");
                foreach (int[] move in allMoves)
                {
                    Console.Write(move[3].ToString() + move[4].ToString() + " | ");
                }
                Console.WriteLine();
                string input = Console.ReadLine();
                if (input.Length < 2)
                {
                    Console.WriteLine("Invalid Move (too short)");
                    continue;
                }
                char tarm = input[0];
                char treach = input[^1];

                int arm = tarm - '0';
                int reach = treach - '0';

                if (arm > 7 || arm < 0 || reach > 2 || reach < 0)
                {
                    Console.WriteLine("Invalid Move (out of bounds)");
                    continue;
                }

                foreach (int[] move in allMoves)
                {
                    if (move[3] == arm && move[4] == reach)
                    {



                        mainBoard = mainBoard.MakeMove(move);
                        doozed = mainBoard.IsMoveDooz(move);
                        moved = true;
                        break;
                    }
                }

                if (!moved)
                {
                    Console.WriteLine("Invalid Move (tile occupied)");
                }

            }
            else
            {
                Console.WriteLine("Your moves:");
                foreach (int[] move in allMoves)
                {
                    Console.Write(move[1].ToString() + move[2].ToString() + " to " + move[3].ToString() + move[4].ToString() + " | ");
                }
                Console.WriteLine();

                string input = Console.ReadLine();
                if (input.Length < 4)
                {
                    Console.WriteLine("Invalid Move (too short)");
                    continue;
                }
                int aarm = input[0] - '0';
                int areach = input[1] - '0';
                int barm = input[^2] - '0';
                int breach = input[^1] - '0';

                if (aarm > 7 || aarm < 0 || areach > 2 || areach < 0 || barm > 7 || barm < 0 || breach > 2 || breach < 0)
                {
                    Console.WriteLine("Invalid Move (out of bounds)");
                    continue;
                }

                foreach (int[] move in allMoves)
                {
                    if (move[1] == aarm && move[2] == areach && move[3] == barm && move[4] == breach)
                    {



                        mainBoard = mainBoard.MakeMove(move);
                        doozed = mainBoard.IsMoveDooz(move);
                        moved = true;
                        break;
                    }
                }

                if (!moved)
                {
                    Console.WriteLine("Invalid Move (tile occupied)");
                }
            }



        }
        if (doozed)
        {
            Console.WriteLine("DOOZ");
            if (mainBoard.justRemoved)
            {
                return;
            }
            PrintBoard();
            Console.WriteLine("Now you can remove a piece.");

            int[][] removables = mainBoard.GetRemovablePieces(playerColor, isDoozRemovingAllowed);
            while (!removed)
            {
                Console.WriteLine("Your options:");
                foreach (int[] move in removables)
                {
                    Console.Write(move[3].ToString() + move[4].ToString() + " | ");
                }

                string input = Console.ReadLine();
                if (input.Length < 2)
                {
                    Console.WriteLine("Invalid option (too short)");
                    continue;
                }
                int arm = input[0] - '0';
                int reach = input[^1] - '0';

                if (arm > 7 || arm < 0 || reach > 2 || reach < 0)
                {
                    Console.WriteLine("Invalid option (out of bounds)");
                    continue;
                }

                foreach (int[] move in removables)
                {
                    if (move[3] == arm && move[4] == reach)
                    {

                        mainBoard = mainBoard.RemovePiece(arm, reach);
                        removed = true;
                        break;
                    }
                }

                if (!removed)
                {
                    Console.WriteLine("Invalid option");
                }

            }
        }

    }

}


public class Program
{




    public static void Main(string[] args)
    {
        Master master = new Master();
        Tile playerColor = Tile.Empty;
        Tile botColor = Tile.Empty;
        master.PrepareBoard();

        while (playerColor == Tile.Empty)
        {
            Console.WriteLine("Select Symbol (O/X), O playes first");
            string temp = Console.ReadLine();
            if (temp == "X" || temp == "x")
            {
                playerColor = Tile.Blue;
                botColor = Tile.Red;
            }
            else if (temp == "O" || temp == "o")
            {
                playerColor = Tile.Red;
                botColor = Tile.Blue;
            }
        }


        if (playerColor == Tile.Blue)
        {
            master.ThinkNGo(botColor);
        }

        while (true)
        {
            master.PlayerTurn(playerColor);
            if (master.IsGameDone(master.mainBoard))
            {
                Console.WriteLine("Human Player Won");
                break;
            }

            Console.WriteLine("Now Thinking.......");
            master.ThinkNGo(botColor);
            if (master.IsGameDone(master.mainBoard))
            {
                master.PrintBoard();
                Console.WriteLine("Bot Won");
                break;
            }
        }

        Console.ReadLine();
        System.Environment.Exit(0);


    }



}
