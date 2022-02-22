namespace MoogleEngine
{
    public static class SimCos
    {
        public static double[] RellenarVectorDelCorpus(Dictionary<string, double> Query, Dictionary<string, double> Corpus)
        {
            double[] vectorCorpus = new double[Query.Count];
            int i = 0;
            foreach (string term in Query.Keys)
            {
                if (Corpus.ContainsKey(term))
                {
                    vectorCorpus[i] = (double)Corpus[term];
                    i++;
                }
            }

            return vectorCorpus;
        }

        public static double[] RellenarCorpus(Dictionary<string, double> Cropus)
        {
            double[] vectorCorpusTotal = new double[Cropus.Count];
            vectorCorpusTotal = Cropus.Values.ToArray();
            return vectorCorpusTotal;
        }

        public static double Coseno(double[] vectorQ, Dictionary<string, double> Query, Dictionary<string, double> tfidfDoc)
        {
            return MultiVectors(vectorQ, RellenarVectorDelCorpus(Query, tfidfDoc)) / (Norma(vectorQ) * Norma(RellenarCorpus(tfidfDoc)));
        }

        static double MultiVectors(double[] Query, double[] Corpus)
        {
            double Multi = 0;
            for (int i = 0; i < Query.Length; i++)
            {
                Multi += Query[i] * Corpus[i];
            }
            return Multi;
        }

        static double Norma(double[] Vector)
        {
            double norma = 0;
            for (int i = 0; i < Vector.Length; i++)
            {
                norma += Math.Pow(Vector[i], 2);
            }
            return Math.Sqrt(norma);
        }
    }
}