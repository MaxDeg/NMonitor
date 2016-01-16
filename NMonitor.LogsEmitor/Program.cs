/******************************************************************************
    Copyright 2016 Maxime Degallaix

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
******************************************************************************/

using NLog;
using System;
using System.Threading;

namespace NMonitor.LogsEmitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var randomer = new Random();

            while (true)
            {
                switch (randomer.Next() % 6)
                {
                    case 0:
                    case 5:
                        break;

                    case 1:
                        logger.Info("FYI it happends");
                        break;

                    case 2:
                        logger.Warn("MMMh something strange happends");
                        break;

                    case 3:
                        try { throw new Exception("a exception occurred!!"); }
                        catch (Exception e) { logger.Error(e); }
                        break;

                    case 4:
                        try { throw new Exception("FATAL EXCEPTION EVERYTHING IS DEAD"); }
                        catch (Exception e) { logger.Fatal(e); }
                        break;
                }

                Thread.Sleep(500);
            }
        }
    }
}
