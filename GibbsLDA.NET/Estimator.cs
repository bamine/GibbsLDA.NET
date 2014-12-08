using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class Estimator 
    {
        // output model
        protected Model trnModel;
        LDACommandLineOptions option;

        public bool init(LDACommandLineOptions option)
        {
            this.option = option;
            trnModel = new Model();

            trnModel.dfile = option.dfile;
            trnModel.dir = option.dir;
            trnModel.K = option.K;
            trnModel.savestep = option.savestep;
            trnModel.niters = option.niters;

            if (option.est)
            {
                if (!trnModel.initNewModel(option))
                    return false;
                trnModel.data.LocalDictionary.WriteWordMap(option.dir + "\\" + option.wordMapFileName);
            }
            else if (option.estc)
            {
                if (!trnModel.initEstimatedModel(option))
                    return false;
            }

            return true;
        }


        public void estimate()
        {
            Console.WriteLine("Sampling " + trnModel.niters + " iteration!");

            int lastIter = trnModel.liter;
            for (trnModel.liter = lastIter + 1; trnModel.liter < trnModel.niters + lastIter; trnModel.liter++)
            {
                Console.WriteLine("Iteration " + trnModel.liter + " ...");

                // for all z_i
                for (int m = 0; m < trnModel.M; m++)
                {
                    for (int n = 0; n < trnModel.data.Docs[m].Length; n++)
                    {
                        // z_i = z[m][n]
                        // sample from p(z_i|z_-i, w)
                        int topic = sampling(m, n);
                        if (topic < 50)
                        {
                            trnModel.z[m].Insert(n, topic);
                        }

                    }// end for each word
                }// end for each document

                if (option.savestep > 0)
                {
                    if (trnModel.liter % option.savestep == 0)
                    {
                        Console.WriteLine("Saving the model at iteration " + trnModel.liter + " ...");
                        computeTheta();
                        computePhi();
                        trnModel.saveModel("model-" + Conversion.ZeroPad(trnModel.liter, 5));
                    }
                }
            }// end iterations		

            Console.WriteLine("Gibbs sampling completed!\n");
            Console.WriteLine("Saving the final model!\n");
            computeTheta();
            computePhi();
            trnModel.liter--;
            trnModel.saveModel("model-final");
        }

        /**
         * Do sampling
         * @param m document number
         * @param n word number
         * @return topic id
         */
        public int sampling(int m, int n)
        {
            // remove z_i from the count variable
            int topic = trnModel.z[m][n];
            int w = trnModel.data.Docs[m].Words[n];

            //initialize random number generator
            var rnd = new Random();

            trnModel.nw[w][topic] -= 1;
            trnModel.nd[m][topic] -= 1;
            trnModel.nwsum[topic] -= 1;
            trnModel.ndsum[m] -= 1;

            double Vbeta = trnModel.V * trnModel.beta;
            double Kalpha = trnModel.K * trnModel.alpha;

            //do multinominal sampling via cumulative method
            for (int k = 0; k < trnModel.K; k++)
            {
                trnModel.p[k] = (trnModel.nw[w][k] + trnModel.beta) / (trnModel.nwsum[k] + Vbeta) *
                        (trnModel.nd[m][k] + trnModel.alpha) / (trnModel.ndsum[m] + Kalpha);
            }

            // cumulate multinomial parameters
            for (int k = 1; k < trnModel.K; k++)
            {
                trnModel.p[k] += trnModel.p[k - 1];
            }

            // scaled sample because of unnormalized p[]
            double u = rnd.NextDouble() * trnModel.p[trnModel.K - 1];

            for (topic = 0; topic < trnModel.K; topic++)
            {
                if (trnModel.p[topic] > u) //sample topic w.r.t distribution p
                    break;
            }

            if (topic < 50)
            {
                trnModel.nw[w][topic] += 1;
                trnModel.nd[m][topic] += 1;
                trnModel.nwsum[topic] += 1;
                trnModel.ndsum[m] += 1;
            }

            return topic;
        }

        public void computeTheta()
        {
            for (int m = 0; m < trnModel.M; m++)
            {
                for (int k = 0; k < trnModel.K; k++)
                {
                    trnModel.theta[m][k] = (trnModel.nd[m][k] + trnModel.alpha) / (trnModel.ndsum[m] + trnModel.K * trnModel.alpha);
                }
            }
        }

        public void computePhi()
        {
            for (int k = 0; k < trnModel.K; k++)
            {
                for (int w = 0; w < trnModel.V; w++)
                {
                    trnModel.phi[k][w] = (trnModel.nw[w][k] + trnModel.beta) / (trnModel.nwsum[k] + trnModel.V * trnModel.beta);
                }
            }
        }
    }
}
