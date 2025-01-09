using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        ChromeOptions options = new ChromeOptions();
        IWebDriver driver = new ChromeDriver(options);

        try
        {
            // Navigate to the login page
            driver.Navigate().GoToUrl("https://casino-royale-game.vercel.app/login");

            // Login
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var usernameField = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Name("username")));
            usernameField.SendKeys("UsernameHere");
            var passwordField = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Name("password")));
            passwordField.SendKeys("PasswordHere");
            var loginButton = driver.FindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();
            Console.WriteLine("Login successful!");

            bool firstTime = true;

            for (int gameCount = 1; gameCount <= 1000; gameCount++) // Play 10 games
            {
                try
                {
                    if (!firstTime)
                    {
                        // Navigate back to the main page
                        driver.Navigate().GoToUrl("https://casino-royale-game.vercel.app/"); // Replace with your main page URL
                        Console.WriteLine("Returned to main page.");
                    }
                    Console.WriteLine($"Starting game {gameCount}...");

                    // Wait for the "Play Tic-Tac-Toe" button after page refresh
                    var ticTacToeButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                        By.XPath("//button[contains(text(), 'Play Tic-Tac-Toe')]")));
                    ticTacToeButton.Click();
                    Console.WriteLine("Navigated to Tic-Tac-Toe.");

                    // Wait for the bet amount field and place a bet
                    var betField = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("betAmount")));
                    betField.Clear();
                    betField.SendKeys("2000");
                    Console.WriteLine("Bet placed.");

                    // Wait for and click the "Start Game" button
                    var startButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector(".w-full.py-4.bg-green-500")));
                    startButton.Click();
                    Console.WriteLine("Game started.");

                    // Play Tic-Tac-Toe
                    PlayTicTacToe(driver, wait);
                    firstTime = false;
                    // Refresh the page to reset for the next game
                    driver.Navigate().Refresh();
                    Console.WriteLine("Game finished, page refreshed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during game {gameCount}: {ex.Message}");
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            Console.WriteLine("All games completed.");
        }
    }

    static void PlayTicTacToe(IWebDriver driver, WebDriverWait wait)
    {
        string[] gridSelectors = new string[]
        {
            ".grid.grid-cols-3 div:nth-child(1)", // Top-left
            ".grid.grid-cols-3 div:nth-child(2)", // Top-center
            ".grid.grid-cols-3 div:nth-child(3)", // Top-right
            ".grid.grid-cols-3 div:nth-child(4)", // Middle-left
            ".grid.grid-cols-3 div:nth-child(5)", // Center
            ".grid.grid-cols-3 div:nth-child(6)", // Middle-right
            ".grid.grid-cols-3 div:nth-child(7)", // Bottom-left
            ".grid.grid-cols-3 div:nth-child(8)", // Bottom-center
            ".grid.grid-cols-3 div:nth-child(9)"  // Bottom-right
        };

        while (true)
        {
            // Check if the game is over
            var outcomeMessage = driver.FindElements(By.CssSelector("p.text-yellow-400.text-2xl.font-semibold.mt-4"));
            if (outcomeMessage.Count > 0)
            {
                Console.WriteLine(outcomeMessage[0].Text); // Display win/loss message
                break;
            }

            // Read the board state
            string[] boardState = new string[9];
            for (int i = 0; i < gridSelectors.Length; i++)
            {
                var cell = driver.FindElement(By.CssSelector(gridSelectors[i]));
                boardState[i] = cell.Text;
            }

            // Find the best move
            int bestMove = FindBestMove(boardState);
            if (bestMove == -1)
            {
                Console.WriteLine("No moves available.");
                break;
            }

            // Make the move
            var bestCell = driver.FindElement(By.CssSelector(gridSelectors[bestMove]));
            bestCell.Click();
            Console.WriteLine($"Placed mark at position {bestMove + 1}.");
            Thread.Sleep(100); // Wait for AI to make its move
        }
    }

    static int FindBestMove(string[] board)
    {
        // Winning combinations
        int[][] winningCombos = new int[][]
        {
        new int[] {0, 1, 2}, // Top row
        new int[] {3, 4, 5}, // Middle row
        new int[] {6, 7, 8}, // Bottom row
        new int[] {0, 3, 6}, // Left column
        new int[] {1, 4, 7}, // Center column
        new int[] {2, 5, 8}, // Right column
        new int[] {0, 4, 8}, // Diagonal 1
        new int[] {2, 4, 6}  // Diagonal 2
        };

        // Check for winning move
        foreach (int[] combo in winningCombos)
        {
            if (board[combo[0]] == "X" && board[combo[1]] == "X" && board[combo[2]] == "")
                return combo[2];
            if (board[combo[0]] == "X" && board[combo[2]] == "X" && board[combo[1]] == "")
                return combo[1];
            if (board[combo[1]] == "X" && board[combo[2]] == "X" && board[combo[0]] == "")
                return combo[0];
        }

        // Check for blocking move
        foreach (int[] combo in winningCombos)
        {
            if (board[combo[0]] == "O" && board[combo[1]] == "O" && board[combo[2]] == "")
                return combo[2];
            if (board[combo[0]] == "O" && board[combo[2]] == "O" && board[combo[1]] == "")
                return combo[1];
            if (board[combo[1]] == "O" && board[combo[2]] == "O" && board[combo[0]] == "")
                return combo[0];
        }

        // Take the center if available
        if (board[4] == "")
            return 4;

        // Take a corner if available
        int[] corners = { 0, 2, 6, 8 };
        foreach (int corner in corners)
        {
            if (board[corner] == "")
                return corner;
        }

        // Take an edge if available
        int[] edges = { 1, 3, 5, 7 };
        foreach (int edge in edges)
        {
            if (board[edge] == "")
                return edge;
        }

        return -1; // No moves available
    }

}
