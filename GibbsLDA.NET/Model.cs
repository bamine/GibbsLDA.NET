using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class Model
    {
        //---------------------------------------------------------------
        //	Class Variables
        //---------------------------------------------------------------

        public static string tassignSuffix;	//suffix for topic assignment file
        public static string thetaSuffix;		//suffix for theta (topic - document distribution) file
        public static string phiSuffix;		//suffix for phi file (topic - word distribution) file
        public static string othersSuffix; 	//suffix for containing other parameters
        public static string twordsSuffix;		//suffix for file containing words-per-topics

        //---------------------------------------------------------------
        //	Model Parameters and Variables
        //---------------------------------------------------------------

        public string wordMapFile; 		//file that contain word to id map
        public string trainlogFile; 	//training log file	

        public string dir;
        public string dfile;
        public string modelName;
        public int modelStatus; 		//see Constants class for status of model
        public LDADataset data;			// link to a dataset

        public int M; //dataset size (i.e., number of docs)
        public int V; //vocabulary size
        public int K; //number of topics
        public double alpha, beta; //LDA  hyperparameters
        public int niters; //number of Gibbs sampling iteration
        public int liter; //the iteration at which the model was saved	
        public int savestep; //saving period
        public int twords; //print out top words per each topic
        public int withrawdata;

        // Estimated/Inferenced parameters
        public double[][] theta; //theta: document - topic distributions, size M x K
        public double[][] phi; // phi: topic-word distributions, size K x V

        // Temp variables while sampling
        public List<int>[] z; //topic assignments for words, size M x doc.size()
        public int[][] nw; //nw[i][j]: number of instances of word/term i assigned to topic j, size V x K
        public int[][] nd; //nd[i][j]: number of words in document i assigned to topic j, size M x K
        public int[] nwsum; //nwsum[j]: total number of words assigned to topic j, size K
        public int[] ndsum; //ndsum[i]: total number of words in document i, size M

        // temp variables for sampling
        public double[] p;

        //---------------------------------------------------------------
        //	Constructors
        //---------------------------------------------------------------	

        public Model()
        {
            setDefaultValues();
        }

        /**
         * Set default values for variables
         */
        public void setDefaultValues()
        {
            wordMapFile = "wordmap.txt";
            trainlogFile = "trainlog.txt";
            tassignSuffix = ".tassign";
            thetaSuffix = ".theta";
            phiSuffix = ".phi";
            othersSuffix = ".others";
            twordsSuffix = ".twords";

            dir = "C:\\Users\\Amine\\Downloads\\JGibbLDA\\models";
            dfile = "auchan_clean_small.dat";
            modelName = "model-final";
            modelStatus = Constants.MODEL_STATUS_UNKNOWN;

            M = 0;
            V = 0;
            K = 100;
            alpha = 50.0 / K;
            beta = 0.1;
            niters = 2000;
            liter = 0;
            twords = 20;

            z = null;
            nw = null;
            nd = null;
            nwsum = null;
            ndsum = null;
            theta = null;
            phi = null;
        }

        //---------------------------------------------------------------
        //	I/O Methods
        //---------------------------------------------------------------
        /**
         * read other file to get parameters
         */
        protected bool readOthersFile(string otherFile)
        {
            //open file <model>.others to read:

            try
            {
                var reader = new StreamReader(otherFile);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split();

                    int count = parts.Length;
                    if (count != 2)
                        continue;

                    string optstr = parts[0].ToLowerInvariant();
                    string optval = parts[1];

                    if (optstr == "alpha")
                    {
                        alpha = Convert.ToDouble(optval);
                    }
                    else if (optstr == "beta")
                    {
                        beta = Convert.ToDouble(optval);
                    }
                    else if (optstr == "ntopics")
                    {
                        K = Convert.ToInt32(optval);
                    }
                    else if (optstr == "liter")
                    {
                        liter = Convert.ToInt32(optval);
                    }
                    else if (optstr == "nwords")
                    {
                        V = Convert.ToInt32(optval);
                    }
                    else if (optstr == "ndocs")
                    {
                        M = Convert.ToInt32(optval);
                    }
                    else
                    {
                        // any more?
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while reading other file:" + e.Message);
                return false;
            }
            return true;
        }

        protected bool readTAssignFile(string tassignFile)
        {
            try
            {
                int i, j;
                var reader = new StreamReader(tassignFile);

                string line;
                z = new List<int>[M];
                data = new LDADataset(M);
                data.V = V;
                for (i = 0; i < M; i++)
                {
                    line = reader.ReadLine();
                    var parts = line.Split();

                    int length = parts.Length;

                    var words = new List<int>();
                    var topics = new List<int>();

                    for (j = 0; j < length; j++)
                    {
                        var token = parts[j];

                        var tokenParts = token.Split(':');
                        if (tokenParts.Count() != 2)
                        {
                            Console.WriteLine("Invalid word-topic assignment line\n");
                            return false;
                        }

                        words.Add(Convert.ToInt32(tokenParts[0]));
                        topics.Add(Convert.ToInt32(tokenParts[0]));
                    }//end for each topic assignment

                    //allocate and add new document to the corpus
                    Document doc = new Document(words);
                    data.SetDoc(doc, i);

                    //assign values for z
                    z[i] = new List<int>();
                    for (j = 0; j < topics.Count(); j++)
                    {
                        z[i].Add(topics[j]);
                    }

                }//end for each doc

                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while loading model: " + e.Message);
                return false;
            }
            return true;
        }

        /**
        * load saved model
        */
        public bool loadModel()
        {
            if (!readOthersFile(dir + "\\" + modelName + othersSuffix))
                return false;

            if (!readTAssignFile(dir + "\\" + modelName + tassignSuffix))
                return false;

            // read dictionary
            var dict = new WordDictionary();
            if (!dict.ReadWordMap(dir + "\\" + wordMapFile))
                return false;

            data.LocalDictionary = dict;

            return true;
        }

        /**
        * Save word-topic assignments for this model
        */
        public bool saveModelTAssign(string filename)
        {
            int i, j;

            try
            {
                var writer = new StreamWriter(filename);

                //write docs with topic assignments for words
                for (i = 0; i < data.M; i++)
                {
                    for (j = 0; j < data.Docs[i].Length; ++j)
                    {
                        writer.Write(data.Docs[i].Words[j] + ":" + z[i][j] + " ");
                    }
                    writer.WriteLine("\n");
                }

                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while saving model tassign: " + e.Message);
                return false;
            }
            return true;
        }

        /**
        * Save theta (topic distribution) for this model
        */
        public bool saveModelTheta(string filename)
        {
            try
            {
                var writer = new StreamWriter(filename);
                for (int i = 0; i < M; i++)
                {
                    for (int j = 0; j < K; j++)
                    {
                        writer.Write(theta[i][j] + " ");
                    }
                    writer.Write("\n");
                }
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while saving topic distribution file for this model: " + e.Message);
                return false;
            }
            return true;
        }

        /**
        * Save word-topic distribution
        */

        public bool saveModelPhi(string filename)
        {
            try
            {
                var writer = new StreamWriter(filename);

                for (int i = 0; i < K; i++)
                {
                    for (int j = 0; j < V; j++)
                    {
                        writer.Write(phi[i][j] + " ");
                    }
                    writer.Write("\n");
                }
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while saving word-topic distribution:" + e.Message);
                return false;
            }
            return true;
        }

        /**
        * Save other information of this model
        */
        public bool saveModelOthers(string filename)
        {
            try
            {
                var writer = new StreamWriter(filename);

                writer.WriteLine("alpha=" + alpha + "\n");
                writer.WriteLine("beta=" + beta + "\n");
                writer.WriteLine("ntopics=" + K + "\n");
                writer.WriteLine("ndocs=" + M + "\n");
                writer.WriteLine("nwords=" + V + "\n");
                writer.WriteLine("liters=" + liter + "\n");

                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while saving model others:" + e.Message);
                return false;
            }
            return true;
        }

        /**
        * Save model the most likely words for each topic
        */
        public bool saveModelTwords(string filename)
        {
            try
            {
                var writer = new StreamWriter(filename);

                if (twords > V)
                {
                    twords = V;
                }

                for (int k = 0; k < K; k++)
                {
                    var wordsProbsList = new Dictionary<int, double>();
                    for (int w = 0; w < V; w++)
                    {
                        wordsProbsList.Add(w, phi[k][w]);
                    }//end foreach word

                    //print topic				
                    writer.Write("Topic " + k + "th:\n");
                    var wordsProbsListOrdered = wordsProbsList.OrderBy(e => e.Value).ToList();

                    for (int i = 0; i < twords; i++)
                    {
                        if (data.LocalDictionary.Contains(wordsProbsListOrdered[i].Key))
                        {
                            string word = data.LocalDictionary.GetWord(wordsProbsListOrdered[i].Key);

                            writer.WriteLine("\t" + word + " " + wordsProbsListOrdered[i].Value);
                        }
                    }
                } //end foreach topic			

                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while saving model twords: " + e.Message);
                return false;
            }
            return true;
        }

        /**
        * Save model
        */
        public bool saveModel(string modelName)
        {
            if (!saveModelTAssign(dir + "\\" + modelName + tassignSuffix))
            {
                return false;
            }

            if (!saveModelOthers(dir + "\\" + modelName + othersSuffix))
            {
                return false;
            }

            if (!saveModelTheta(dir + "\\" + modelName + thetaSuffix))
            {
                return false;
            }

            if (!saveModelPhi(dir + "\\" + modelName + phiSuffix))
            {
                return false;
            }

            //if (twords > 0){
            if (!saveModelTwords(dir + "\\" + modelName + twordsSuffix))
                return false;
            //}
            return true;
        }


        //---------------------------------------------------------------
        //	Init Methods
        //---------------------------------------------------------------
        /**
         * initialize the model
         */
        protected bool init(LDACommandLineOptions option)
        {
            if (option == null)
                return false;

            modelName = option.modelName;
            K = option.K;

            alpha = option.alpha;
            if (alpha < 0.0)
                alpha = 50.0 / K;

            if (option.beta >= 0)
                beta = option.beta;

            niters = option.niters;

            dir = option.dir;
            if (dir.EndsWith("\\"))
                dir = dir.Substring(0, dir.Length - 1);

            dfile = option.dfile;
            twords = option.twords;
            wordMapFile = option.wordMapFileName;

            return true;
        }

        /**
        * Init parameters for estimation
        */
        public bool initNewModel(LDACommandLineOptions option)
        {
            //if (!init(option))
            //return false;
            var rnd = new Random();
            int m, n, w, k;
            p = new double[K];

            data = LDADataset.ReadDataset(dir + "\\" + dfile);
            if (data == null)
            {
                Console.WriteLine("Fail to read training data!\n");
                return false;
            }

            //+ allocate memory and assign values for variables		
            M = data.M;
            V = data.V;
            dir = option.dir;
            savestep = option.savestep;

            // K: from command line or default value
            // alpha, beta: from command line or default values
            // niters, savestep: from command line or default values

            nw = new int[V][];
            for (w = 0; w < V; w++)
            {
                nw[w] = new int[K];
                for (k = 0; k < K; k++)
                {
                    nw[w][k] = 0;
                }
            }

            nd = new int[M][];
            for (m = 0; m < M; m++)
            {
                nd[m] = new int[K];
                for (k = 0; k < K; k++)
                {
                    nd[m][k] = 0;
                }
            }

            nwsum = new int[K];
            for (k = 0; k < K; k++)
            {
                nwsum[k] = 0;
            }

            ndsum = new int[M];
            for (m = 0; m < M; m++)
            {
                ndsum[m] = 0;
            }

            z = new List<int>[M];
            for (m = 0; m < data.M; m++)
            {
                int N = data.Docs[m].Length;
                z[m] = new List<int>();

                //initilize for z
                for (n = 0; n < N; n++)
                {
                    int topic = (int)Math.Floor(rnd.NextDouble() * K);
                    z[m].Add(topic);

                    // number of instances of word assigned to topic j
                    nw[data.Docs[m].Words[n]][topic] += 1;
                    // number of words in document i assigned to topic j
                    nd[m][topic] += 1;
                    // total number of words assigned to topic j
                    nwsum[topic] += 1;
                }
                // total number of words in document i
                ndsum[m] = N;
            }

            theta = new double[M][];
            for (m = 0; m < M; m++)
            {
                theta[m] = new double[K];
            }
            phi = new double[K][];
            for (k = 0; k < K; k++)
            {
                phi[k] = new double[V];
            }

            return true;
        }

        /**
        * Init parameters for inference
        * @param newData DataSet for which we do inference
        */
        public bool initNewModel(LDACommandLineOptions option, LDADataset newData, Model trnModel)
        {
            if (!init(option))
                return false;

            int m, n;

            var rnd = new Random();

            K = trnModel.K;
            alpha = trnModel.alpha;
            beta = trnModel.beta;

            p = new double[K];
            Console.WriteLine("K:" + K);

            data = newData;

            //+ allocate memory and assign values for variables		
            M = data.M;
            V = data.V;
            dir = option.dir;
            savestep = option.savestep;
            Console.WriteLine("M:" + M);
            Console.WriteLine("V:" + V);

            // K: from command line or default value
            // alpha, beta: from command line or default values
            // niters, savestep: from command line or default values

            nw = ArrayInitializers.ZerosInt(V, K);
            nd = ArrayInitializers.ZerosInt(M, K);

            nwsum = ArrayInitializers.ZerosInt(K);
            ndsum = ArrayInitializers.ZerosInt(M);

            z = new List<int>[M];
            for (m = 0; m < data.M; m++)
            {
                int N = data.Docs[m].Length;
                z[m] = new List<int>();

                //initilize for z
                for (n = 0; n < N; n++)
                {
                    int topic = (int)Math.Floor(rnd.NextDouble() * K);
                    z[m].Add(topic);

                    // number of instances of word assigned to topic j
                    nw[data.Docs[m].Words[n]][topic] += 1;
                    // number of words in document i assigned to topic j
                    nd[m][topic] += 1;
                    // total number of words assigned to topic j
                    nwsum[topic] += 1;
                }
                // total number of words in document i
                ndsum[m] = N;
            }

            theta = ArrayInitializers.Empty(M, K);
            phi = ArrayInitializers.Empty(K, V);

            return true;
        }

        /**
        * Init parameters for inference
        * reading new dataset from file
        */
        public bool initNewModel(LDACommandLineOptions option, Model trnModel)
        {
            if (!init(option))
                return false;

            LDADataset dataset = LDADataset.ReadDataset(dir + "\\" + dfile, trnModel.data.LocalDictionary);
            if (dataset == null)
            {
                Console.WriteLine("Fail to read dataset!\n");
                return false;
            }

            return initNewModel(option, dataset, trnModel);
        }

        /**
 * init parameter for continue estimating or for later inference
 */
        public bool initEstimatedModel(LDACommandLineOptions option)
        {
            if (!init(option))
                return false;

            int m, n, w;

            p = new double[K];

            // load model, i.e., read z and trndata
            if (!loadModel())
            {
                Console.WriteLine("Fail to load word-topic assignment file of the model!\n");
                return false;
            }

            Console.WriteLine("Model loaded:");
            Console.WriteLine("\talpha:" + alpha);
            Console.WriteLine("\tbeta:" + beta);
            Console.WriteLine("\tM:" + M);
            Console.WriteLine("\tV:" + V);


            nw = ArrayInitializers.ZerosInt(V, K);
            nd = ArrayInitializers.ZerosInt(M, K);

            nwsum = ArrayInitializers.ZerosInt(K);
            ndsum = ArrayInitializers.ZerosInt(M);

            for (m = 0; m < data.M; m++)
            {
                int N = data.Docs[m].Length;

                // assign values for nw, nd, nwsum, and ndsum
                for (n = 0; n < N; n++)
                {
                    w = data.Docs[m].Words[n];
                    int topic = z[m][n];

                    // number of instances of word i assigned to topic j
                    nw[w][topic] += 1;
                    // number of words in document i assigned to topic j
                    nd[m][topic] += 1;
                    // total number of words assigned to topic j
                    nwsum[topic] += 1;
                }
                // total number of words in document i
                ndsum[m] = N;
            }


            theta = ArrayInitializers.Empty(M, K);
            phi = ArrayInitializers.Empty(K, V);
            dir = option.dir;
            savestep = option.savestep;

            return true;
        }

    }
}
