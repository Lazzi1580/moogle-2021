using System.Xml.Schema;
using System.Numerics;
namespace MoogleEngine
{
    public class QueryD
    {
        //Propiedad para guardar el Tf_idf de las query
        public Dictionary<string, double> CalculoTf_IdfQueryD = new Dictionary<string, double>();
        //Propiedad para vectorizar el peso de las query
        public double[] vectorQ = new double[0];
        //Propiedad para separar la query en palabras
        public string[] QueryWordsQ;
        //Propiedad para guardar la Importancia de los Documentos y sus Titulos respectivoss
        public Dictionary<string, double> Scores = new Dictionary<string, double>();
        //Propiedad para guardar de forma auxiliar los reslutados que voy obteniendo de los Operadores de busqueda
        public Dictionary<string, double> Operators = new Dictionary<string, double>();
        //Propiedad para entregar el resultado final de los scores cuadno la query contiene Operadores de busqueda
        public Dictionary<string, double> ResultFOperators = new Dictionary<string, double>();
        //Propiedad que guarda cada una de las palabras de la query sin eliminar los operadores de busqueda
        public string[] CadenasConOperadores;

        public string[] SinonimosList;

        public string[] NuevasQuerys;



        public QueryD(string query)
        {
            this.QueryWordsQ = QueryWordsQ;
            this.NuevasQuerys = NuevasQuerys;
            this.CadenasConOperadores = query.Split(' ');
            this.SinonimosList = SinonimosList;
            this.CalculoTf_IdfQueryD = CalculoTf_IdfQueryD;
            this.vectorQ = vectorQ;
            this.ResultFOperators = ResultFOperators;

            if (ComprobarOperadores(CadenasConOperadores) == false)
            {
                QueryWordsQ = QueryWords(query);
                RevisarOrtogra(QueryWordsQ);
                NuevasQuerys = LlenarNuevasQuerys(QueryWordsQ, Raiz.Vocabulario, Raiz.salida);
                SinonimosList = LlenarConSinonimos(NuevasQuerys, Sinonimo.ListSinom);
                CalculoTf_IdfQuery(SinonimosList);
                CalScore();
                ArreglarValores(QueryWordsQ, NuevasQuerys, SinonimosList, Scores, Moogle.ldoc);
                OrdenarCreacionSnipet(SinonimosList);
            }
            else
            {
                QueryWordsQ = QueryWords(query);
                RevisarOrtogra(QueryWordsQ);
                CalculoTf_IdfQuery(QueryWordsQ);
                CalScore();
                Colleccion(CadenasConOperadores);
                GuardarDiccionariosFinalesOperators();
                OrdenarCreacionSnipet(QueryWordsQ);
            }

        }

        public static void ArreglarValores(string[] QueryWordsQ, string[] NuevasQuerys, string[] SinonimosList, Dictionary<string, double> Scores, List<DocumentP> ldoc)
        {
            for (int i = 0; i < SinonimosList.Length; i++)
            {
                for (int j = 0; j < Moogle.ldoc.Count; j++)
                {
                    if (ldoc[j].CalTf_Idf.ContainsKey(SinonimosList[i]))
                    {
                        string titulo = Moogle.ldoc[j].Tittle;
                        Scores[titulo] = 0.1;
                    }
                }
            }

            for (int i = 0; i < NuevasQuerys.Length; i++)
            {
                for (int j = 0; j < Moogle.ldoc.Count; j++)
                {
                    if (ldoc[j].CalTf_Idf.ContainsKey(NuevasQuerys[i]))
                    {
                        string titulo = Moogle.ldoc[j].Tittle;
                        Scores[titulo] = 0.2;
                    }
                }
            }

            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                for (int j = 0; j < Moogle.ldoc.Count; j++)
                {
                    if (ldoc[j].CalTf_Idf.ContainsKey(QueryWordsQ[i]))
                    {
                        string titulo = Moogle.ldoc[j].Tittle;
                        System.Console.WriteLine(titulo);
                        Scores[titulo]++;
                        Scores[titulo]++;
                    }
                }
            }
        }

        static void Imprimir(string[] NuevasQuerys)
        {
            foreach (string term in NuevasQuerys)
            {
                System.Console.WriteLine(term);
            }
        }
        //Raices
        public string[] LlenarNuevasQuerys(string[] QueryWordsQ, string[] ListVoc, string[] ListSalidas)
        {
            string[] palabras = new string[0];
            int a = 0;

            for (int p = 0; p < QueryWordsQ.Length; p++)
            {
                a = Moogle.GuardArIndiceVoca(QueryWordsQ[p], ListVoc);
                if (a != -1)
                {
                    int[] indices = Moogle.CargarIndicesRaices(a, ListSalidas);

                    for (int i = 0; i < indices.Length; i++)
                    {
                        palabras = palabras.Append(ListVoc[indices[i]]).ToArray();
                    }
                }
                else
                {
                    palabras = palabras.Append((QueryWordsQ[p])).ToArray();
                }
            }

            return palabras;
        }

        //Sinonimos
        public string[] LlenarConSinonimos(string[] QueryWordsQ, List<string[]> ListSinom)
        {
            string[] sinonimos = new string[0];
            string[] ListaFinal = new string[0];
            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                for (int j = 0; j < ListSinom.Count; j++)
                {
                    if (ListSinom[j].Contains(QueryWordsQ[i]))
                    {
                        sinonimos = Moogle.RellenarListasDeSin(QueryWordsQ[i], ListSinom[j]);
                    }
                }
            }

            for (int i = 0; i < sinonimos.Length; i++)
            {
                ListaFinal = ListaFinal.Append(sinonimos[i]).ToArray();
            }
            for (int j = 0; j < QueryWordsQ.Length; j++)
            {
                if (!ListaFinal.Contains(QueryWordsQ[j]))
                {
                    ListaFinal = ListaFinal.Append(QueryWordsQ[j]).ToArray();
                }
            }
            return ListaFinal;
        }


        // Metodo para crear el snipet de cada documento
        public void OrdenarCreacionSnipet(string[] QueryWordsQ)
        {
            //Itero la query y voy creando el snipet de cada palabra de la query
            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                Moogle.CrearSnipet(QueryWordsQ[i], CadenasConOperadores, ResultFOperators, Scores);
            }
        }

        //Metodo para guardar los scores finales cuando la query contiene operadores de busqueda
        public void GuardarDiccionariosFinalesOperators()
        {
            //itero cada termino en el diccionario de Operadores
            foreach (string term in Operators.Keys)
            {
                //Si el score es mayor que 0 lo guardo
                if (Operators[term] > 0)
                {
                    //Lo agrego al Diccionario final
                    ResultFOperators.Add(term, Operators[term]);
                }
            }
        }


        //Metodo para calcular el score final
        public void CalScore()
        {
            //Inicializo la varibale Cscore
            double Csocre = 0;
            //Itero cada documento
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                //Realizo el calculo de la sim del coseno llamando al metodo de la clase SimCos
                Csocre = SimCos.Coseno(vectorQ, CalculoTf_IdfQueryD, Moogle.ldoc[i].CalTf_Idf);
                //Lo guardo en el diccionario Operadores el cual voy a utilizar solo en caso de que la query tenga Operadores de busqueda
                Operators.Add(Moogle.ldoc[i].Tittle, Csocre);
                //Reviso si el calculo es myor q 0
                if (Csocre > 0)
                {
                    //Si lo es lo agrego al diccionario finals
                    Scores.Add(Moogle.ldoc[i].Tittle, Csocre);
                }
            }
        }

        //Metodo para separar cada palabra de la query
        public string[] QueryWords(string query)
        {
            //Le paso el metodo ToSplit a la query para guardar cada palabra separada por espacios
            string[] QueryWords = query.Split(' ').ToArray();
            //Inicializo un arreglo de string
            string[] QueryArreglado = new string[0];
            //Itero el arreglo que contiene las palabras seapradas por espacios
            for (int i = 0; i < QueryWords.Length; i++)
            {
                //Las agrego al arreglo de string vacio despues de pasarle el metodo de EliminarSignos
                QueryArreglado = QueryArreglado.Append(DocumentP.EliminarSignos(QueryWords[i])).ToArray();
            }
            //Inicializo un contador en 0
            int contador = 0;
            //Itero el arreglo de string anterior
            for (int i = 0; i < QueryArreglado.Length; i++)
            {
                //Reviso si contiene espacios vacios
                if (QueryArreglado[i] == " " || QueryArreglado[i] == "")
                {
                    //Si lo contiene sumo 1 al contador
                    contador++;
                }
            }

            //Declaro un arreglo de string de longitud (la longitud del query arreglado menos el valor del contador)
            string[] QueryFinal = new string[QueryArreglado.Length - contador];
            //inicializo la variable k
            int k = 0;
            //Itero el arreglo de string arreglado
            for (int i = 0; i < QueryArreglado.Length; i++)
            {
                //Reviso si contiene espacios vacios en caso de hacerlo lo ignora y comienza la proxima iteracion
                if (QueryArreglado[i] == " " || QueryArreglado[i] == "") continue;
                //Sino pues lo agrega
                QueryFinal[k] = QueryArreglado[i];
                //Y le suma 1 a la variable k
                k++;
            }

            return QueryFinal;
        }


        //Metodo para calcular el Tf_idf de la Query
        public void CalculoTf_IdfQuery(string[] QueryWords)
        {
            //Declaro un diccionario de string,double
            Dictionary<string, double> NewDictionary = new Dictionary<string, double>();
            //Inicializo una varaible en 0
            double Calculo = 0;
            //itero el arreglo de string que contiene las palabra de la query
            for (int i = 0; i < QueryWords.Length; i++)
            {
                //Reviso si la query no esta contenida en el Diccionario creado anteriormente
                if (!CalculoTf_IdfQueryD.ContainsKey(QueryWords[i]))
                {
                    //Reviso si la query esta contenida en el diccionario del Idf genearl de la base de datos
                    if (Moogle.idf.ContainsKey(QueryWords[i]))
                    {
                        //Realizo el calculo del tf_idf
                        Calculo = (ContadorDePalabras(QueryWords, QueryWords[i]) / QueryWords.Length) * (double)Moogle.idf[QueryWords[i]];
                        //Lo almaceno en el diccionario
                        CalculoTf_IdfQueryD.Add(QueryWords[i], Calculo);
                        //Lo almaceno en el vector
                        vectorQ = vectorQ.Append(Calculo).ToArray();
                    }
                }
            }
        }

        // Metodo para contar cuantas veces contiene la query una palabra
        public static double ContadorDePalabras(string[] QueryWords, string palabra)
        {
            //declaro una variable entera en 0
            double cnt = 0;
            //Itero el arreglo de string que contiene las palabras de la query
            for (int i = 0; i < QueryWords.Length; i++)
            {
                //Reivsa si la palabra en la posicion i es la palabra buscada 
                if (palabra == QueryWords[i])
                {
                    //Si es suma 1 al contador
                    cnt++;
                }
            }
            return cnt;
        }




        #region Operadores


        //Metodo dedicado a la busqueda de los operadores en la query
        public void Colleccion(string[] CadenasConOperadores)
        {
            //itera la cadena que contiene a las palabras de la query con sus operadores
            for (int i = 0; i < CadenasConOperadores.Length; i++)
            {
                //Revisa si la palabra contiene el operador de No Debe estar
                if (CadenasConOperadores[i].Contains('!'))
                {
                    //Si la tiene llama al metodo
                    Exclucion(CadenasConOperadores[i]);
                }
                //Revisa si la palabra contiene el operador Debe estar
                if (CadenasConOperadores[i].Contains('^'))
                {
                    //Si la tiene llama al metodo
                    TieneQueAparecer(CadenasConOperadores, CadenasConOperadores[i]);
                }
                //Revisa si la palabra contiene el operador Cercania
                if (CadenasConOperadores[i].Contains('~'))
                {
                    //Si la tiene llama al metodo
                    Cercania(CadenasConOperadores, Moogle.ldoc);
                }
                //Revisa si la palabra contiene el operador Importancia
                if (CadenasConOperadores[i].Contains('*'))
                {
                    //Si la tiene llama al metodo
                    Importancia(CadenasConOperadores, CadenasConOperadores[i]);
                }
            }
        }

        public bool ComprobarOperadores(string[] CadenasConOperadores)
        {
            for (int i = 0; i < CadenasConOperadores.Length; i++)
            {
                //Revisa si la palabra contiene el operador de No Debe estar
                if (CadenasConOperadores[i].Contains('!'))
                {
                    return true;
                }
                //Revisa si la palabra contiene el operador Debe estar
                if (CadenasConOperadores[i].Contains('^'))
                {
                    return true;
                }
                //Revisa si la palabra contiene el operador Cercania
                if (CadenasConOperadores[i].Contains('~'))
                {
                    return true;
                }
                //Revisa si la palabra contiene el operador Importancia
                if (CadenasConOperadores[i].Contains('*'))
                {
                    return true;
                }
            }
            return false;

        }


        //Metodo para el Operador de NoDebe Estar
        public void Exclucion(string NoDebe)
        {
            //Declaro un string donde guardo la palabra eliminando los signos
            string palabra = DocumentP.EliminarSignos((NoDebe));


            //Itero los documentos
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                //Reviso si en los documentos esta la palabra buscada
                if (Moogle.ldoc[i].tf.ContainsKey(palabra))
                {
                    //Si esta guardo el titulo
                    string term = Moogle.ldoc[i].Tittle;
                    // Reviso Si es mayor que 0 en el Diccionario Auxilar para metodos
                    if (Operators[term] > 0)
                    {
                        //Si es mayor que 0 la hago 0
                        Operators[term] = 0;
                    }
                }
            }

        }

        //Metodo de debe aparecer
        public void TieneQueAparecer(string[] QueryWordsQ, string DebeApar)
        {
            //Declaro un string donde guardo la palabra eliminando los signos
            string palabra = DocumentP.EliminarSignos(DebeApar);
            //Itero los documentos
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                //Reviso si en los documentos esta la palabra buscada
                if (Moogle.ldoc[i].tf.ContainsKey(palabra))
                {
                    //Si esta guardo el titulo
                    string term = Moogle.ldoc[i].Tittle;
                    //Reviso si el Score del titulo de es > 0
                    if (Operators[term] > 0)
                    {
                        //Si lo es sumo 1
                        Operators[term]++;
                    }
                    //Reviso si el Score del titulo es 0
                    if (Operators[term] == 0)
                    {
                        //Si lo es le sumo 1
                        Operators[term]++;
                    }
                }
                else
                {
                    //Si no esta guardo el titulo
                    string term = Moogle.ldoc[i].Tittle;
                    //Reviso si es mayor que 0
                    if (Operators[term] > 0)
                    {
                        //Si lo es lo igualo a 0
                        Operators[term] = 0;
                    }
                }
            }
        }

        //Metodo para econtrar la cercania 
        public void Cercania(string[] query, List<DocumentP> ldoc)
        {
            //Inicializo un string vacio
            string palabra = "";
            //Inicializo un entero en 0
            int indice = 0;
            //Inicializo una variable boleana en falso
            bool control = false;
            //Itero la query
            for (int i = 0; i < query.Length; i++)
            {
                //Reviso si en la posicion marcada contien el Operador
                if (query[i].Contains('~'))
                {
                    //Reviso si no he guardado la palabra de la posicion anterior
                    if (!palabra.Contains(query[i - 1]))
                    {
                        //Reviso si la longitud de la query es mayor que el indice +2
                        if (query.Length > indice + 2)
                        {
                            //Reviso si la query en el indice+2 contiene el operador
                            if (!query[indice + 2].Contains('~'))
                            {
                                //Si la condicion esperada sucede agrega la palabra de la posicion anterior y la que le sigue
                                palabra += query[i - 1] + " " + query[i + 1];
                                //Guardo el indice
                                indice = i;
                                //Hago la varible boleana true
                                control = true;
                            }
                        }
                    }
                    else
                    {
                        //Sino pasa agrego solo la palabra de la  posicion siguiente
                        palabra += " " + query[i + 1];
                        //Guardo el indices
                        indice = i;
                    }
                }
            }
            //Guardo en una arreglo de string las palabras a las que hay que buscarle la cercanis
            string[] queryAb = palabra.Split(' ');
            //Inicializo una variable entera de valor igal a la longitud del arreglo de string anterior
            int entero = queryAb.Length;
            //Declaro un un arreglo de enteros de longitud igual al valor de la variable anterior
            int[] indices = new int[entero];
            //Declaro un arreglo de entero de longitud igual a la cantidad de documentos
            int[] restas = new int[ldoc.Count];
            //Declaro un arreglo de string de longitud igual a la cantidad de documentos
            string[] textos = new string[ldoc.Count];

            //Itero por cada uno de los documentos
            for (int d = 0; d < ldoc.Count; d++)
            {
                //Vacio la variable indice
                indices.DefaultIfEmpty();
                //Itero el arreglo de strings que contienen las palabras
                for (int i = 0; i < queryAb.Length; i++)
                {
                    //itera el arreglo de string que contiene las lineas de cada documento
                    for (int j = 0; j < ldoc[d].twordsS.Length; j++)
                    {
                        //Reviso si estas lineas contiene la palabra
                        if (ldoc[d].twordsS[j].Contains(queryAb[i]))
                        {
                            //Reviso si la palabra que contiene la palabra asignada son iguales despues de eliminarle los signos de puntuacion
                            if (DocumentP.EliminarSignos(ldoc[d].twordsS[j]) == queryAb[i])
                            {
                                //Guardo el indice y rompo el ciclo
                                indices[i] = j;
                                break;
                            }
                        }
                    }
                }
                //Guardo el titulo del documento
                textos[d] = ldoc[d].Tittle;
                //Inicializo un entero en 0
                int resta = 0;
                //Inicializo un string vacio
                string auto = "";
                //Reviso si si ambas palabras estan contenidas en el docuemtno
                if (ldoc[d].tf.ContainsKey(queryAb[0]) && ldoc[d].tf.ContainsKey(queryAb[1]))
                {
                    //Realizo la resta
                    resta = indices[1] - indices[0];
                    //Reviso si la resta es = a 1
                    if (resta == 1)
                    {
                        //si lo es guardo el titulo
                        auto = ldoc[d].Tittle;
                        //Reviso si el diccionario de Operadores contiene el titulo
                        if (Operators.ContainsKey(auto))
                        {
                            //Si lo contiene le sumo 5
                            Operators[auto]++;
                            Operators[auto]++;
                            Operators[auto]++;
                            Operators[auto]++;
                            Operators[auto]++;

                        }
                    }
                    else
                    {
                        //Sino le asigno resta en d a un arreglo de double
                        restas[d] = resta;
                    }
                }
            }
            //a la resta final la igualo al valor minimo del arreglo anterior
            int final = IndiceMin(restas);
            //guardo el titulo del documento
            string titulo = textos[final];
            //Revisa SI el diccionario de operadores contiene el titulo
            if (Operators.ContainsKey(titulo))
            {
                //Le suma 5
                Operators[titulo]++;
                Operators[titulo]++;
                Operators[titulo]++;
                Operators[titulo]++;

            }
        }

        //Metodo para sacar el indice del menor valor del arreglo
        static public int IndiceMin(int[] a)
        {
            //declaro una varibale entera en 0
            int MinInd = 0;
            //itero el arreglo de enteros
            for (int b = 0; b < a.Length; b++)
            {
                //Reviso si es 0
                if (a[b] == 0)
                {
                    //Si lo es le sumo 10
                    a[b] += 10;
                }
            }
            //itero los arreglos de enteros
            for (int i = 0; i < a.Length - 1; i++)
            {
                //reviso si la Cual es el minmo entre las 2 posiciones o si El indice del menor numero es igual al siguiente
                if (Math.Min(a[MinInd], a[i + 1]) == a[i + 1] || MinInd == a[i + 1])
                {
                    //Guardo en la varibale la posicion siguiente
                    MinInd = i + 1;
                }
            }
            return MinInd;
        }


        //Metodo para calcular la importancia Con el Operador
        public void Importancia(string[] palabras, string queryImportante)
        {
            //Declaro una variable entera
            int contador = 0;
            //itero  la palabra para contar la cantdiad de opeadores de improtancia
            for (int i = 0; i < queryImportante.Length; i++)
            {
                //Reviso si lo contiene
                if (queryImportante[i] == '*')
                {
                    //Le sumo 1 al conador
                    contador++;
                }
            }
            //Declaro una varibale entera
            int resta = 0;
            //Guardo en un termino la palabra con los signos eliminados
            string term = DocumentP.EliminarSignos(queryImportante);
            //itero los documetos
            for (int i = 0; i < Moogle.ldoc.Count; i++)
            {
                //Reviso si la palabra esta contenida en los documentos
                if (Moogle.ldoc[i].tf.ContainsKey(term))
                {
                    //Igualo la resta al contador
                    resta = contador;
                    do
                    {
                        //Mientras resta sea difernete de 0 va a sumarle 1 al score del titulo
                        Operators[Moogle.ldoc[i].Tittle]++;
                        resta--;
                    } while (resta != 0);
                }
            }
        }

        #endregion





        #region  ArreglarOrtografia
        //Metodo para dar la sugerencia
        public static string Sugerncia(string query)
        {
            //Declaro una variable double en 0
            double calculo = 0;
            //Declaro un arreglo de string
            string[] palabras = new string[0];
            //Declaro un arreglo de string
            string[] palabrasMinimas = new string[0];
            //Declaro un arreglo de double
            double[] calculos = new double[0];
            //Declaro un arreglo de doubles
            double[] minimasDistancias = new double[0];
            //Declaro una variable double
            double swap = 0;
            //Declaro una cadena vacia
            string swapC = "";

            //Itero el Diccionario que contiene la frecuencia global de los documentos
            foreach (string term in Moogle.idf.Keys)
            {
                //Reviso si la palabra esta en el rango 3+ 3- 2+ 2- 1+ 1- 0
                if (term.Length - 2 == query.Length || term.Length + 2 == query.Length || term.Length + 3 == query.Length || term.Length - 3 == query.Length || term.Length == query.Length || term.Length - 1 == query.Length || term.Length + 1 == query.Length)
                {
                    //Reviso si la palabra es vacia
                    if (query != "" || query != " ")
                    {
                        //Si no lo es llamo al metodo de la Distancia del Levenshtein
                        calculo = LevenshteinDistance(query, term, 0);
                        //La agrego al arreglo
                        palabras = palabras.Append(term).ToArray();
                        //La agrego al arreglo
                        calculos = calculos.Append(calculo).ToArray();
                    }
                }
            }
            //Igualo el contador a la longitud del arreglo anterior
            int contador = calculos.Length;
            //Inicializo un ciclo do while
            do
            {
                //itero el arreglo de documentos
                for (int i = 0; i < calculos.Length - 1; i++)
                {
                    //Reviso cual de los 2 es mayor
                    if (calculos[i] > calculos[i + 1])
                    {
                        //Si la condicion es true
                        //Guardo el indice del mayor, sustituyo al mayor por el menor y coloco al mayor en la posicion antigua del menor
                        swap = calculos[i];
                        calculos[i] = calculos[i + 1];
                        calculos[i + 1] = swap;

                        //Realizo el mismo proceso con los string guiandome por los indices de sus valores
                        swapC = palabras[i];
                        palabras[i] = palabras[i + 1];
                        palabras[i + 1] = swapC;
                    }
                }
                //Le resto 1 al contador
                contador--;
            } while (contador != 0);

            //Itero el arreglo de calculos
            for (int j = 0; j < calculos.Length - 2; j++)
            {
                //Reviso cual es mayor
                if (calculos[j] < calculos[j + 1])
                {
                    //agrego el valor y el string a los arreglos
                    minimasDistancias = minimasDistancias.Append(calculos[j]).ToArray();
                    palabrasMinimas = palabrasMinimas.Append(palabras[j]).ToArray();
                }
                else if (calculos[j] == calculos[j + 1])
                {
                    //agrego el valor y el string a los arreglos
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

        //Metodos para Comparar
        public static string Comparacion(string[] palabras)
        {
            //Declaro un arreglo de double 
            double[] idfs = new double[0];
            //itero las palabras de la query
            for (int i = 0; i < palabras.Length; i++)
            {
                //Agrego el valor
                idfs = idfs.Append(Moogle.idf[palabras[i]]).ToArray();
            }
            //Declaro una varible double, un string vacio, y un entero igualado a la longitud del arreglo 
            double swap = 0;
            string swapC = "";
            int contador = idfs.Length;
            //Inicializo el ciclo do while
            do
            {
                //Itero el arreglo double
                for (int i = 0; i < idfs.Length - 2; i++)
                {
                    //Reviso cual es el menor numero
                    if (idfs[i] > idfs[i + 1])
                    {
                        //Si la condicion es true
                        //Guardo el indice del mayor, sustituyo al mayor por el menor y coloco al mayor en la posicion antigua del menors
                        swap = idfs[i];
                        idfs[i] = idfs[i + 1];
                        idfs[i + 1] = swap;
                        //Realizo el mismo proceso con los string guiandome por los indices de sus valores
                        swapC = palabras[i];
                        palabras[i] = palabras[i + 1];
                        palabras[i + 1] = swapC;
                    }
                }
                //resto 1 al contador
                contador--;
            } while (contador != 0);

            return palabras[0];
        }


        //Metodo para Revisar la Ortografia
        public void RevisarOrtogra(string[] QueryWordsQ)
        {
            int contador = 0;
            //Itero el arreglo de las palabras de la query
            for (int i = 0; i < QueryWordsQ.Length; i++)
            {
                for (int k = 0; k < Moogle.ldoc.Count; k++)
                {

                    if (!Moogle.ldoc[i].tf.ContainsKey(QueryWordsQ[i]))
                    {
                        contador++;
                    }

                }
                if (contador == Moogle.ldoc.Count)
                {
                    //Llama al metodo Sugerencia a cada palabra
                    QueryWordsQ[i] = Sugerncia(QueryWordsQ[i]);
                }
            }
        }
        #endregion

        #region  Auxilares

        //Metodo para sacar el menor indice
        static public int IndiceDeMin(int[] sumas)
        {
            //declaro una varible entera
            int minimo = 0;
            //itero el arreglo de enteros
            for (int i = 0; i < sumas.Length - 1; i++)
            {
                //Reviso cual es el minmo entre los numeros
                if (Math.Min(sumas[i], sumas[i + 1]) == sumas[i])
                {
                    //Guardo el indice
                    minimo = i;
                }
                else
                {
                    //Guardo el indice
                    minimo = i + 1;
                }
            }
            return minimo;
        }

        #endregion


    }
}