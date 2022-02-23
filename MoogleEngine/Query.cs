using System.Xml.Schema;
using System.Numerics;
namespace MoogleEngine
{
    public class QueryD
    {
        public Dictionary<string, double> CalculoTf_IdfQueryD = new Dictionary<string, double>();
        public double[] vectorQ = new double[0];
        public string[] QueryWordsQ;
        public Dictionary<string, double> Scores = new Dictionary<string, double>();
        public Dictionary<string, double> Operators = new Dictionary<string, double>();
        public Dictionary<string, double> ResultFOperators = new Dictionary<string, double>();
        public string[] CadenasConOperadores;


        public QueryD(string query)
        {
            this.QueryWordsQ = QueryWordsQ;
            this.CadenasConOperadores = query.Split(' ');
            QueryWordsQ = QueryWords(query);
            RevisarOrtogra(QueryWordsQ);
            this.CalculoTf_IdfQueryD = CalculoTf_IdfQueryD;
            this.vectorQ = vectorQ;
            CalculoTf_IdfQuery(QueryWordsQ);
            CalScore();
            Colleccion(CadenasConOperadores);
            this.ResultFOperators = ResultFOperators;
            GuardarDiccionariosFinalesOperators();
        }



        public void GuardarDiccionariosFinalesOperators()
        {
            foreach (string term in Operators.Keys)
            {
                if (Operators[term] != 0)
                {
                    ResultFOperators.Add(term, Operators[term]);
                }
            }
        }

        public void CalScore()
        {
            double Csocre = 0;
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                Csocre = SimCos.Coseno(vectorQ, CalculoTf_IdfQueryD, Moogle.ldoc[i].CalTf_Idf);
                Operators.Add(Moogle.ldoc[i].Tittle, Csocre);
                if (Csocre != 0)
                {
                    Scores.Add(Moogle.ldoc[i].Tittle, Csocre);
                }
            }
        }
        public string[] QueryWords(string query)
        {
            string[] QueryWords = query.Split(' ').ToArray();
            string[] QueryArreglado = new string[0];
            for (int i = 0; i < QueryWords.Length; i++)
            {
                QueryArreglado = QueryArreglado.Append(DocumentP.EliminarSignos(QueryWords[i])).ToArray();
            }

            return QueryArreglado;
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

        #region Operadores

        public void Colleccion(string[] QueryWordsQ)
        {
            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                if (QueryWordsQ[i].Contains('!'))
                {
                    Exclucion(QueryWordsQ, QueryWordsQ[i]);
                }
                else if (QueryWordsQ[i].Contains('^'))
                {
                    TieneQueAparecer(QueryWordsQ, QueryWordsQ[i]);
                }
            }


        }

        public void Exclucion(string[] QueryWordsQ, string NoDebe)
        {
            string palabra = DocumentP.EliminarSignos((NoDebe));
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                if (Moogle.ldoc[i].tf.ContainsKey(palabra))
                {
                    string term = Moogle.ldoc[i].Tittle;
                    if (Operators[term] > 0)
                    {
                        Operators[term] = 0;
                    }
                }

            }
        }

        public void TieneQueAparecer(string[] QueryWordsQ, string DebeApar)
        {
            string palabra = DocumentP.EliminarSignos(DebeApar);
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                if (Moogle.ldoc[i].tf.ContainsKey(palabra))
                {
                    string term = Moogle.ldoc[i].Tittle;
                    if (Operators[term] > 0)
                    {
                        Operators[term]++;
                    }
                }
                else
                {
                    string term = Moogle.ldoc[i].Tittle;
                    if (Operators[term] > 0)
                    {
                        Operators[term] = 0;
                    }
                }
            }
        }
        #endregion


        #region  ArreglarOrtografia
        public static string Sugerncia(string query)
        {
            double calculo = 0;
            string[] palabras = new string[0];
            string[] palabrasMinimas = new string[0];
            double[] calculos = new double[0];
            double[] minimasDistancias = new double[0];
            double swap = 0;
            string swapC = "";

            foreach (string term in Moogle.idf.Keys)
            {
                if (term.Length - 2 == query.Length || term.Length + 2 == query.Length || term.Length + 3 == query.Length || term.Length - 3 == query.Length || term.Length == query.Length || term.Length - 1 == query.Length || term.Length + 1 == query.Length)
                {
                    calculo = LevenshteinDistance(query, term, 0);
                    palabras = palabras.Append(term).ToArray();
                    calculos = calculos.Append(calculo).ToArray();
                }
            }
            int contador = calculos.Length;
            do
            {
                for (int i = 0; i < calculos.Length - 1; i++)
                {
                    if (calculos[i] > calculos[i + 1])
                    {
                        swap = calculos[i];
                        calculos[i] = calculos[i + 1];
                        calculos[i + 1] = swap;

                        swapC = palabras[i];
                        palabras[i] = palabras[i + 1];
                        palabras[i + 1] = swapC;
                    }
                }
                contador--;
            } while (contador != 0);


            for (int j = 0; j < calculos.Length - 2; j++)
            {
                if (calculos[j] < calculos[j + 1])
                {
                    minimasDistancias = minimasDistancias.Append(calculos[j]).ToArray();
                    palabrasMinimas = palabrasMinimas.Append(palabras[j]).ToArray();
                }
                else if (calculos[j] == calculos[j + 1])
                {
                    minimasDistancias = minimasDistancias.Append(calculos[j]).ToArray();
                    palabrasMinimas = palabrasMinimas.Append(palabras[j]).ToArray();
                }
                break;
            }
            return Comparacion(palabrasMinimas);
        }

        public static int LevenshteinDistance(string s, string t, double porcentaje)
        {
            porcentaje = 0;

            // d es una tabla con m+1 renglones y n+1 columnas
            int costo = 0;
            int m = s.Length;
            int n = t.Length;
            int[,] d = new int[m + 1, n + 1];

            // Verifica que exista algo que comparar
            if (n == 0) return m;
            if (m == 0) return n;

            // Llena la primera columna y la primera fila.
            for (int i = 0; i <= m; d[i, 0] = i++) ;
            for (int j = 0; j <= n; d[0, j] = j++) ;


            /// recorre la matriz llenando cada unos de los pesos.
            /// i columnas, j renglones
            for (int i = 1; i <= m; i++)
            {
                // recorre para j
                for (int j = 1; j <= n; j++)
                {
                    /// si son iguales en posiciones equidistantes el peso es 0
                    /// de lo contrario el peso suma a uno.
                    costo = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1,  //Eliminacion
                                  d[i, j - 1] + 1),                             //Insercion 
                                  d[i - 1, j - 1] + costo);                     //Sustitucion
                }
            }

            /// Calculamos el porcentaje de cambios en la palabra.
            if (s.Length > t.Length)
                porcentaje = ((double)d[m, n] / (double)s.Length);
            else
                porcentaje = ((double)d[m, n] / (double)t.Length);
            return d[m, n];
        }

        public static string Comparacion(string[] palabras)
        {
            double[] idfs = new double[0];
            for (int i = 0; i < palabras.Length; i++)
            {
                idfs = idfs.Append(Moogle.idf[palabras[i]]).ToArray();
            }
            double swap = 0;
            string swapC = "";
            int contador = idfs.Length;
            do
            {
                for (int i = 0; i < idfs.Length - 2; i++)
                {
                    if (idfs[i] > idfs[i + 1])
                    {
                        swap = idfs[i];
                        idfs[i] = idfs[i + 1];
                        idfs[i + 1] = swap;

                        swapC = palabras[i];
                        palabras[i] = palabras[i + 1];
                        palabras[i + 1] = swapC;
                    }
                }
                contador--;
            } while (contador != 0);

            return palabras[0];
        }

        public void RevisarOrtogra(string[] QueryWordsQ)
        {
            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                QueryWordsQ[i] = Sugerncia(QueryWordsQ[i]);
            }
        }
        #endregion
    }
}