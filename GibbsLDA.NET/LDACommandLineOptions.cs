using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class LDACommandLineOptions
    {
        [Option("-est", HelpText = "Specify whether we want to estimate model from scratch")]
        public bool est { set; get; }

        [Option("-estc", HelpText = "Specify whether we want to continue the last estimation")]
        public bool estc { set; get; }

        [Option("-inf", HelpText = "Specify whether we want to do inference")]
        public bool inf { set; get; }

        [Option("-dir", HelpText = "Specify directory")]
        public string dir { set; get; }

        [Option("-dfile", HelpText = "Specify data file")]
        public string dfile { set; get; }

        [Option("-model", HelpText = "Specify the model name")]
        public string modelName { set; get; }

        [Option("-alpha", HelpText = "Specify alpha")]
        public double alpha { set; get; }

        [Option("-beta", HelpText = "Specify beta")]
        public double beta { set; get; }

        [Option("-ntopics", HelpText = "Specify the number of topics")]
        public int K { set; get; }

        [Option("-niters", HelpText = "Specify the number of iterations")]
        public int niters { set; get; }

        [Option("-savestep", HelpText = "Specify the number of steps to save the model since the last save")]
        public int savestep { set; get; }

        [Option("-twords", HelpText = "Specify the number of most likely words to be printed for each topic")]
        public int twords { set; get; }

        [Option("-withrawdata", HelpText = "Specify whether we include raw data in the input")]
        public bool withrawdata { set; get; }

        [Option("-wordmap", HelpText = "Specify the wordmap file")]
        public string wordMapFileName { set; get; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
