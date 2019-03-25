﻿using System;

namespace VeeamGZipStream
{
    class Program
    {
        static void Main(string[] args)
        {
            ValidationParams paramsReader = new ValidationParams();
            ProcessManager manager = new ProcessManager();
            try
            {
                var settings = paramsReader.Read(args);

                Console.WriteLine("Входные данные разобраны. Алгоритм начал работу ...\n");
                manager.RunProcessManager(settings);
                Console.WriteLine("Алгоритм закончил работу без ошибок.");
            }
            catch(Exception ex)
            {
                manager.Pool.Stop();
                Console.WriteLine("Алгоритм прервал работу по причине:\n{0}", ex.Message);
            }
            finally
            {
                Console.WriteLine("Нажмите любую клавишу для выхода.");
                Console.ReadLine();
            }
        }
    }
}
