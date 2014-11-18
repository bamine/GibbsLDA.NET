using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    class Program
    {
        static void Main(string[] args)
        {
            LDACommandLineOptions option = new LDACommandLineOptions();
            var parser = new Parser();
            option.beta = 0.1;
            option.K = 50;
            option.niters = 1000;
            option.savestep = 100;
            option.twords = 20;
            option.dfile = "auchan_clean_full.dat";
            option.dir = "C:\\Users\\Amine\\Downloads\\JGibbLDA\\models";
            option.est = true;
            option.modelName = "model-final";

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                //			if (args.length == 0){
                //				showHelp(parser);
                //				return;
                //			}

                parser.ParseArguments(args, option);

                if (option.est || option.estc)
                {
                    Estimator estimator = new Estimator();
                    estimator.init(option);
                    estimator.estimate();
                }
                else if (option.inf)
                {
                    Inferencer inferencer = new Inferencer();
                    inferencer.init(option);

                    Model newModel = inferencer.inference();

                    for (int i = 0; i < newModel.phi.Length; ++i)
                    {
                        //phi: K * V
                        Console.WriteLine("-----------------------\ntopic" + i + " : ");
                        for (int j = 0; j < 10; ++j)
                        {
                            Console.WriteLine(inferencer.globalDict.Id2Word[j] + "\t" + newModel.phi[i][j]);
                        }
                    }
                }
            }
            catch (ParserException cle)
            {
                Console.WriteLine("Command line error: " + cle.Message);
                showHelp(option);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in main: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }

            stopWatch.Stop();
            Console.WriteLine("\n This run took : " + stopWatch.ElapsedMilliseconds / 1000.0 + " seconds");
        }

        public static void showHelp(LDACommandLineOptions option)
        {
            Console.WriteLine("LDA [options ...] [arguments...] \n");
            Console.WriteLine(option.GetUsage());

        }
    }
}

