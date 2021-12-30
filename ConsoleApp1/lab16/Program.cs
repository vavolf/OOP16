using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace lab16
{
    class Program
    {
        public static List<uint> SieveEratosthenes(uint n)
        {
            var numbers = new List<uint>();
            //заполнение списка числами от 2 до n-1
            for (var i = 2u; i < n; i++)
            {
                numbers.Add(i);
            }

            for (var i = 0; i < numbers.Count; i++)
            {
                for (var j = 2u; j < n; j++)
                {
                    //удаляем кратные числа из списка
                    numbers.Remove(numbers[i] * j);
                }
            }
            return numbers;
        }

        static public void Producer(int id, BlockingCollection<int> storehouse)
        {
            var rand = new Random();
            var sleepTime = rand.Next(0, 3000);
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(sleepTime);
                storehouse.Add(i + 10 * id);
                Console.WriteLine($"Поставщик {id} завез товар {i + 3 * id} на склад");
            }

        }
        static public void Consumer(int id, BlockingCollection<int> storehouse)
        {
            var rand = new Random();
            var sleepTime = rand.Next(0, 7000);
            Thread.Sleep(sleepTime);
            if (storehouse.Count == 0)
            {
                Console.WriteLine($"Покупатель {id} ушел без товара");
                return;
            }
            foreach (var item in storehouse)
            {
                int _item = item;
                if (storehouse.TryTake(out _item))
                {
                    Console.WriteLine($"Покупатель {id} купил товар {item}");
                }
            }

        }

        static async void SieveEratosthenesAsync(uint n)
        {
            Console.WriteLine("Начало метода SieveEratosthenesAsync"); // выполняется синхронно
            await Task.Run(() => SieveEratosthenes(n));                // выполняется асинхронно
            Console.WriteLine("Конец метода SieveEratosthenesAsync");
        }

        static void Main(string[] args)
        {
            // ЗАДАНИЕ 1 - СОЗДАНИЕ ДЛИТЕЛЬНОЙ ЗАДАЧИ И ОЦЕНКА ПРОИЗВОДИТЕЛЬНОСТИ

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Task<List<uint>> task1 = new Task<List<uint>>(() => SieveEratosthenes(1000), TaskCreationOptions.LongRunning);
            task1.RunSynchronously();
            stopwatch.Stop();
            Console.WriteLine($"Идентификатор задачи - {task1.Id}, статус - {task1.Status}");
            TimeSpan timeSpan = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds,
            timeSpan.Milliseconds / 10);
            Console.WriteLine("Затраченное время: " + elapsedTime);

            // ЗАДАНИЕ 2 - ЭТА ЖЕ ЗАДАЧА С ТОКЕНОМ ОТМЕНЫ

            Task<List<uint>> task2 = new Task<List<uint>>(() => SieveEratosthenes(1000));
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            task2.RunSynchronously();
            cancellationTokenSource.Cancel();

            //ЗАДАНИЕ 3 - 3 ЗАДАЧИ С ВОЗВРАТОМ РЕЗУЛЬТАТА ДЛЯ ВЫПОЛНЕНИЯ ЧЕТВЕРТОЙ ЗАДАЧИ

            int i = 10;
            int f = 20;
            Task<int> task3 = new Task<int>(() => i * i);
            task3.Start();
            Task<int> task4 = new Task<int>(() => f * f);
            task4.Start();
            Task<int> task5 = new Task<int>(() => i * f);
            task5.Start();
            Task<int> task6 = new Task<int>(() => task3.Result * task4.Result * task5.Result);
            task6.Start();
            Console.WriteLine($"{task3.Result} * {task4.Result} * {task5.Result} = {task6.Result}");

            // ЗАДАНИЕ 4 - ПРОДОЛЖЕНИЕ ЗАДАЧ

            Task task7 = task6.ContinueWith(t => Console.WriteLine($"{task6.Result} * 2 = {task6.Result * 2}"));
            var awaiter = task6.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                int result = awaiter.GetResult();
                Console.WriteLine($"{result} * 2 = {result * 2}");
            });

            // ЗАДАНИЕ 5 - РАСПАРАЛЛЕЛИВАНИЕ ЦИКЛОВ

            stopwatch.Start();
            int[] array1 = new int[1000];
            Parallel.For(1, 1000000, d =>
            {

                Random rand = new Random();
                for (int y = 0; y < 1000; y++)
                {
                    array1[y] = rand.Next();
                }
                int[] array2 = new int[1000];
                for (int y = 0; y < 1000; y++)
                {
                    array2[y] = rand.Next();
                }
            });
            stopwatch.Stop();
            TimeSpan timeSpan1 = stopwatch.Elapsed;
            string elapsedTime1 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            timeSpan1.Hours, timeSpan1.Minutes, timeSpan1.Seconds,
            timeSpan1.Milliseconds / 10);
            Console.WriteLine("Затраченное время: " + elapsedTime1);

            stopwatch.Start();

            Parallel.ForEach(array1, elem => Console.WriteLine(elem));

            stopwatch.Stop();
            TimeSpan timeSpan2 = stopwatch.Elapsed;
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds,
            timeSpan2.Milliseconds / 10);
            Console.WriteLine("Затраченное время: " + elapsedTime2);

            // ЗАДАНИЕ 6 - РАСПАРАЛЛЕЛИВАНИЕ ВЫПОЛНЕНИЯ БЛОКА ОПЕРАТОРОВ

            Parallel.Invoke(() => SieveEratosthenes(500),
                            () => SieveEratosthenes(1000));

            // ЗАДАНИЕ 7 - ЗАДАЧА В BLOCKINGCOLLECTION

            var storehouse = new BlockingCollection<int>();

            for (int z = 0; z < 5; z++)
            {
                new Thread(() => Producer(z, storehouse)).Start();
                Thread.Sleep(10);
            }
            for (int z = 0; z < 10; z++)
            {
                new Thread(() => Consumer(z, storehouse)).Start();
                Thread.Sleep(10);
            }

            // АСИНХРОННОЕ ВЫПОЛНЕНИЕ МЕТОДА С ПОМОЩЬЮ ASYNC И AWAIT

            SieveEratosthenesAsync(100);

            Console.ReadKey();
        }
    }
}