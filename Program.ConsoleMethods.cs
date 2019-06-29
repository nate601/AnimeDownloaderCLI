using System;
using System.Collections.Generic;

namespace AnimeDown
{
    public partial class Program
    {
        /// <summary>
        /// Prompts the operator for a number, and repeats the request if the number is invalid
        /// </summary>
        /// <param name="prompt ">The message with which to prompt the operator</param>
        /// <param name="max ">The maximum number that the operator must enter (inclusive)</param>
        /// <returns></returns>
        private static int ReadNumber(string prompt, int max) => ReadNumber(prompt, 0, max);
        /// <summary>
        /// Writes a string to the console, in the foreground color
        /// </summary>
        /// <param name="writable">The string to write to the console</param>
        /// <param name="color">The foreground color to write in</param>
        private static void WriteInColor(string writable, ConsoleColor color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(writable);
            Console.ForegroundColor = oldColor;
        }
        /// <summary>
        /// Prompts the user for a keypress, and maps that keypress to an object
        /// </summary>
        /// <param name="prompt">The message to show to the user.  Does not automaticaly append a newline to the end of the string</param>
        /// <param name="consoleKeyMap">The map between the console keys and the result</param>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <returns></returns>
        public static T ReadKeyMap<T>(string prompt, Dictionary<ConsoleKey, T> consoleKeyMap)
        {
            while (true)
            {
                Console.Write(prompt);
                var key = Console.ReadKey(true);
                if (consoleKeyMap.ContainsKey(key.Key))
                    return consoleKeyMap[key.Key];
            }
        }
        /// <summary>
        /// Prompts the operator for a number, and repeats the request if the number is invalid
        /// </summary>
        /// <param name="prompt ">The message with which to prompt the operator</param>
        /// <param name="min ">The minimum number that the operator must enter (inclusive)</param>
        /// <param name="max ">The maximum number that the operator must enter (inclusive)</param>
        /// <returns></returns>
        private static int ReadNumber(string prompt, int min, int max)
        {
            int result;
            while (true)
            {
                Console.WriteLine($"{ prompt } ({ min }-{ max })");
                if (int.TryParse(Console.ReadLine(), out result) && result >= min && result <= max)
                    break;
            }
            return result;
        }
    }
}
