namespace MoogleEngine
{
    public static class SimCos
    {
        //Metodo para rellenar los vectores de la base de datos donde aparece la query
        public static double[] RellenarVectorDelCorpus(Dictionary<string, double> Query, Dictionary<string, double> Corpus)
        {
            //Inicializo el arreglo de double
            double[] vectorCorpus = new double[Query.Count];
            //Inicializo la varible entera
            int i = 0;
            //itero cada palabra de la query
            foreach (string term in Query.Keys)
            {
                //Reviso si esta en la base de datos
                if (Corpus.ContainsKey(term))
                {
                    //En caso de estar le agrero su peso al vector
                    vectorCorpus[i] = (double)Corpus[term];
                    i++;
                }
            }

            return vectorCorpus;
        }

        //Metodo para Rellenar el vector del documento

        public static double[] RellenarCorpus(Dictionary<string, double> Cropus)
        {
            //Inicializo el arreglo de double
            double[] vectorCorpusTotal = new double[Cropus.Count];
            //Lo relleno con los pesos del documento
            vectorCorpusTotal = Cropus.Values.ToArray();
            return vectorCorpusTotal;
        }

        //Metodo para encontarr la Similitud del Coseno
        public static double Coseno(double[] vectorQ, Dictionary<string, double> Query, Dictionary<string, double> tfidfDoc)
        {
            //Realizo la multiplicacion de los Vectores, la norma de los vectores y los divido para encontrar la similitud del coseno
            return MultiVectors(vectorQ, RellenarVectorDelCorpus(Query, tfidfDoc)) / (Norma(vectorQ) * Norma(RellenarCorpus(tfidfDoc)));
        }

        //Metodo para realizar la Multiplicacion de los vectores
        static double MultiVectors(double[] Query, double[] Corpus)
        {
            //Declaro una varibale double
            double Multi = 0;
            //Voy iternando la query
            for (int i = 0; i < Query.Length; i++)
            {
                //Multiplico cada peso de la query por cada peso del Corpus
                Multi += Query[i] * Corpus[i];
            }
            return Multi;
        }

        static double Norma(double[] Vector)
        {
            //Declaro una varibale double
            double norma = 0;
            //Itero el vector
            for (int i = 0; i < Vector.Length; i++)
            {
                //Le calculamos su norma
                norma += Math.Pow(Vector[i], 2);
            }
            return Math.Sqrt(norma);
        }
    }
}