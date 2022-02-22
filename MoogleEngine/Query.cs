using System.Numerics;
namespace MoogleEngine
{
    public class QueryD
    {
        public Dictionary<string, double> CalculoTf_IdfQueryD = new Dictionary<string, double>();
        public double[] vectorQ = new double[0];
        public string[] QueryWordsQ;
        public Dictionary<string, double> Scores = new Dictionary<string, double>();


        public QueryD(string query)
        {
            this.QueryWordsQ = QueryWordsQ;
            QueryWordsQ = QueryWords(query);
            this.CalculoTf_IdfQueryD = CalculoTf_IdfQueryD;
            this.vectorQ = vectorQ;
            CalculoTf_IdfQuery(QueryWordsQ);
            CalScore();
        }

        public void CalScore()
        {
            double Csocre = 0;
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                Csocre = SimCos.Coseno(vectorQ, CalculoTf_IdfQueryD, Moogle.ldoc[i].CalTf_Idf);
                if (Csocre != 0)
                {
                    Scores.Add(Moogle.ldoc[i].Tittle, Csocre);
                }
            }
        }
        public string[] QueryWords(string query)
        {
            string QueryArreglado = DocumentP.EliminarSignos(query);
            string[] QueryWords = QueryArreglado.Split(' ').ToArray();
            return QueryWords;
        }

        public void CalculoTf_IdfQuery(string[] QueryWords)
        {
            Dictionary<string, double> NewDictionary = new Dictionary<string, double>();
            double Calculo = 0;
            for (int i = 0; i < QueryWords.Length; i++)
            {
                if (!CalculoTf_IdfQueryD.ContainsKey(QueryWords[i]))
                {
                    if (Moogle.idf.ContainsKey(QueryWords[i]))
                    {
                        Calculo = (ContadorDePalabras(QueryWords, QueryWords[i]) / QueryWords.Length) * (double)Moogle.idf[QueryWords[i]];
                        CalculoTf_IdfQueryD.Add(QueryWords[i], Calculo);
                        vectorQ = vectorQ.Append(Calculo).ToArray();
                    }
                }
            }

        }

        public static double ContadorDePalabras(string[] QueryWords, string palabra)
        {
            double cnt = 0;
            for (int i = 0; i < QueryWords.Length; i++)
            {
                if (palabra == QueryWords[i])
                {
                    cnt++;
                }
            }
            return cnt;
        }

        #region Sniped

        #endregion
    }
}