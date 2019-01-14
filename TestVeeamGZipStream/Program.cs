using System;

namespace TestVeeamGZipStream
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ParamsReader paramsReader = new ParamsReader();
                ProcessManager manager = new ProcessManager();

                var settings = paramsReader.Read(args);

                Console.WriteLine("Входные данные разобраны. Алгоритм начал работу ...\n");
                manager.RunProcessManager(settings);
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
