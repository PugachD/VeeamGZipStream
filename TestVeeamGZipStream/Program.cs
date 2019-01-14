using System;

namespace TestVeeamGZipStream
{
    class Program
    {
        static void Main(string[] args)
        {
            ParamsReader paramsReader = new ParamsReader();
            ProcessManager manager = new ProcessManager();
            try
            {
               var settings = paramsReader.Read(args);
                Console.WriteLine("Входные данные разобраны. Алгоритм начал работу ...");
                manager.Run(settings);
                Console.WriteLine("Алгоритм закончил работу без ошибок.");
            }
            catch(Exception ex)
            {
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
