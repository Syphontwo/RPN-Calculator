using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPN_Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            Console.WriteLine(calc.startup);
            Console.WriteLine(calc.help);

            string userinput = "";
            bool quit = false;

            do
            {
                userinput = Console.ReadLine();
                userinput = userinput.Trim();
                userinput = FilterWhiteSpaces(userinput);

                if (userinput.Length > 0) //make sure there was some input
                {

                    if (userinput.Substring(0, 1) == "/") //check if it is a command
                    {
                        userinput = userinput.Substring(1, userinput.Length - 1); //remove the '/'
                        userinput = userinput.ToLower(); //standardize it for less comparison conditions

                        if ((new[] { "h", "help" }).Contains(userinput))
                        {
                            Console.WriteLine(calc.help);
                        }
                        if ((new[] { "q", "quit", "close", "exit", "stop", "end" }).Contains(userinput))
                        {
                            Console.WriteLine("Quitting");
                            quit = true;
                        }
                    }
                    else if (calc.validate(userinput))
                    {
                        Console.WriteLine(calc.evaluate(userinput) + "\r\n");
                        //the expression can be sent to evaluation
                    }
                    else
                    {
                        Console.WriteLine("Invalid\r\n");
                    }
                }
                else
                {
                    
                }

            } while (!quit);
        }

        public static string FilterWhiteSpaces(string input) //credit to Nolonar for this method, taken from stackoverflow http://stackoverflow.com/questions/206717/how-do-i-replace-multiple-spaces-with-a-single-space-in-c
        {
            if (input == null)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (i == 0 || c != ' ' || (c == ' ' && input[i - 1] != ' '))
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }

    }

    class Calculator
    {
        private readonly char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'};
        private readonly char[] operators = { 'x', 'X', '*', '/', '+', '-', '^'};
        private readonly char[] seperators = { ' ', '\t', '\n', '\r' };
        private int seperatorsCount = 0;

        private string startuptext = "Welcome to Mikey's RPN calculator!";

        private string helptext = 
            "Type using Reverse Polish Notation with a [space] between each part like this \r\n" +
            "3 4 + \r\n" +
            "You may use + - * / ^ as well as x instead of * for multiplication.\r\n\r\n" +
            
            "to see this again, type /help.\r\n" +
            "to quit, type /quit or /q\r\n";


        public string startup
        {
            get { return startuptext; }
            set { }
        }
        public string help
        {
            get { return helptext; }
            set { }
        }

        public bool validate(string expressionInput)
        {
            int operatorsCount = 0;
            int numbersCount = 0;
            int decimalsCount = 0;
            int x = 0;
            char ch = '\0';

            char[] expression = expressionInput.ToCharArray();

            for (int i = 0; i < expression.Length; i++)
            {
                ch = expression[i];
                x = i;

                if (!numbers.Contains(ch) && !operators.Contains(ch) && !seperators.Contains(ch))
                {
                    Console.WriteLine("Syntax usage error, invalid token: " + ch + " at index " + i);
                    return false;
                }

                if (operators.Contains(ch)) //count number of operators
                {
                    if (ch != '-')
                    {
                        operatorsCount += 1;
                    }
                }

                if (i == expression.Length - 1 && numbers.Contains(ch)) //if last token is a number, it won't work
                {
                    Console.WriteLine("Missing or misplaced operator error: " + ch + " at index " + i);
                    return false;
                }

                if (numbers.Contains(ch) && !numbers.Contains(expression[i + 1])) //count number of number tokens
                {
                    numbersCount += 1;
                }
            }

            if (numbersCount < operatorsCount) //this is the required ratio for a valid expression
            {
                Console.WriteLine("Incorrect number of operators: Found " + numbersCount + " numbers " + operatorsCount + " operators");
                return false;
            }

            if (ch == '.') { decimalsCount += 1; }
            if (decimalsCount > numbersCount) {
                Console.WriteLine("To many decimals found.  Check numbers and try again");
                return false;
            }

            if (seperators.Contains(ch))
            {
                seperatorsCount += 1;
            }

            return true;

        }

        public double evaluate(string expression)
        {
            List<double> evaluationStack = new List<double>();

            int x = 0;

            for (int i = 0; i < expression.Count(); i++)
            {
                x = i;
                char ch = expression[i];

                if (seperators.Contains(ch)) { continue; } //skip spaces

                if (numbers.Contains(ch) || (ch == '-' && numbers.Contains((char)expression[i+1]))) //add numbers to the stack
                {
                    i = nextNonNumber(expression, i) -1; //this sets the iteration to skip steps to the next token
                    try
                    {
                        evaluationStack.Add(Double.Parse(expression.Substring(x, i+1 - x)));
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Number error occured with " + expression.Substring(x, i+1 - x) + "\r\nPlease check numbers and try again");
                        return 0;
                    }
                    continue;
                }

                if (operators.Contains(ch))
                {
                    if(evaluationStack.Count < 2)
                    {
                        Console.WriteLine("Operator " + ch + " at index " + i + " encountered when not expected.\r\nPlease check formula and try again.");
                        return 0;
                    }
                    else
                    {
                        switch (ch)
                        {
                            case '+':
                                add(ref evaluationStack);
                                break;
                            case '-':
                                //if this was a negative sign, it was picked up already in the stack.  If it reaches here, it is a minus.
                                subtract(ref evaluationStack);
                                break;
                            case '*':
                                multiply(ref evaluationStack);
                                break;
                            case 'x':
                                multiply(ref evaluationStack);
                                break;
                            case 'X':
                                multiply(ref evaluationStack);
                                break;
                            case '/':
                                divide(ref evaluationStack);
                                break;
                            case '^':
                                powerOf(ref evaluationStack);
                                break;
                        }
                    }
                    continue;
                }
            }

            if (evaluationStack.Count() > 1)
            {
                Console.WriteLine("Error in evaluation, too many numbers left in stack.\r\n" + 
                    "This is likely caused by too few operators, or a minus sign being " +
                    "counted as a negative number.\r\n" + 
                    "Check your spacing and operators.");
            }

            return evaluationStack[0];
            //perform the steps
        }
        private void add(ref List<double> stack)
        {
            stack[stack.Count-2] = stack[stack.Count() - 2] + stack[stack.Count() - 1];
            stack.RemoveAt(stack.Count() - 1);
        }
        private void subtract(ref List<double> stack)
        {
            stack[stack.Count - 2] = stack[stack.Count() - 2] - stack[stack.Count() - 1];
            stack.RemoveAt(stack.Count() - 1);
        }

        private void multiply(ref List<double> stack)
        {
            stack[stack.Count - 2] = stack[stack.Count() - 2] * stack[stack.Count() - 1];
            stack.RemoveAt(stack.Count() - 1);
        }

        private void divide(ref List<double> stack)
        {
            stack[stack.Count - 2] = stack[stack.Count() - 2] / stack[stack.Count() - 1];
            stack.RemoveAt(stack.Count() - 1);
        }

        private void powerOf(ref List<double> stack)
        {
            stack[stack.Count - 2] = Math.Pow(stack[stack.Count() - 2] , stack[stack.Count() - 1]);
            stack.RemoveAt(stack.Count() - 1);
        }

        private int nextNonNumber(string expression, int index)
        {
            do { index += 1; } while (numbers.Contains((char)expression[index]));
            return index;
        }

    }
}