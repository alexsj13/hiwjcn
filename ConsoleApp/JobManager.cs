using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.task;
using System.Reflection;

namespace ConsoleApp
{
    public static class JobManager
    {
        public static void Start()
        {
            TaskManager.StartAllTasks(new Assembly[] { typeof(JobManager).Assembly });
            while (true)
            {
                if (Console.ReadLine() == "q") { break; }
            }
            TaskManager.Dispose();
        }
    }
}
