using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
namespace MoogleEngine;


public static class Moogle
{
    public static Dictionary<string, double> Gneral = new Dictionary<string, double>();
    public static Dictionary<string, double> idf = new Dictionary<string, double>();
    public static List<DocumentP> ldoc = new List<DocumentP>();
    public static Dictionary<string, string> Snipet = new Dictionary<string, string>();


    public static void ComienzoDeIndex()
    {
        //Reviso la direccion en la cual se guardan los archivos y guardo solamente los que sean txts
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        //Voy iterando la variable var y guardando cada DocumentP en una lista de DocumentP 
        foreach (var i in list)
        {
            DocumentP d = new DocumentP(File.ReadAllLines(i), i);
            //Agrego cada DocumentP a la lista
            ldoc.Add(d);
        }
        //Realizo el calculo de la frecuencia global de cada palabra contenida en mis documentos
        CalidfOrg(Moogle.ldoc);
        //Realizo el calculo del Tf_Idf de cada palabra en su respectivo documento
        LeerSinonimos();
        CargarRaices_Y_Voc();
        Tf_Idf();
        System.Console.WriteLine("Tf_Idf Listo" + "\n");
    }
    public static SearchResult Query(string query)
    {
        if (query == "travesura realizada")
        {
            System.Environment.Exit(0);
        }
        //Limpio el Snipet ya que esta es static
        Snipet.Clear();
        //Inicializo un objeto QueryD_Object
        QueryD QueryD_Object = new QueryD(query);
        //Inicializa un SearchItem
        SearchItem[] Resultados = new SearchItem[0];
        //Inicializa 2 variables enterass
        int totalD = 0;
        int totalAD = 0;

        //Revisa si hay operadores
        if (RevisarOperadores(QueryD_Object.CadenasConOperadores))
        {
            //Le da valor a las variables entearas
            totalD = QueryD_Object.ResultFOperators.Count;
            totalAD = Snipet.Count;
            //Rellena el SearchItem
            Resultados = new SearchItem[Math.Min(totalD, totalAD)];
            {
                int i = 0;
                //Itera el diccionario de Snipet
                foreach (string term in Snipet.Keys)
                {
                    //Rellena el resultado final
                    Resultados[i] = new SearchItem(Path.GetFileNameWithoutExtension(term), Snipet[term], QueryD_Object.ResultFOperators[term]);
                    i++;
                }
            }
        }
        else
        {
            //Le da valor a las variables entearas
            totalD = QueryD_Object.Scores.Count;
            totalAD = Snipet.Count;
            //Rellena el SearchItem
            Resultados = new SearchItem[Math.Min(totalD, totalAD)];
            {
                int i = 0;
                //Itera el diccionario de Snipet
                foreach (string term in Snipet.Keys)
                {
                    //Rellena el resultado final
                    Resultados[i] = new SearchItem((Path.GetFileNameWithoutExtension(term)), Snipet[term], QueryD_Object.Scores[term]);
                    i++;
                }
            }
        }
        //revisa si contiene Operadores 
        if (ContieneOpp(query) == true)
        {
            return new SearchResult(Resultados, query);
        }
        else
        {

            if (EstaPresente(query) == false)
            {
                return new SearchResult(Resultados, SugernciaK(query));
            }
            else
            {
                return new SearchResult(Resultados, query);
            }
        }
    }

    //Metodo para la sugerencia a cada palabra
    public static string SugernciaK(string query)
    {
        string[] Convertir = query.Split(' ');
        string palabraDvol = "";
        //Itero el arreglo de string 
        for (int i = 0; i < Convertir.Length; i++)
        {
            //Voy llenado el string con la palabara sugerida
            palabraDvol += QueryD.Sugerncia(DocumentP.EliminarSignos(Convertir[i])) + " ";
        }

        return palabraDvol;
    }

    public static bool EstaPresente(string query)
    {
        foreach (string term in Gneral.Keys)
        {
            if (term == DocumentP.EliminarSignos(query))
                return true;
        }
        return false;
    }

    //Metodo para revisar si contiene operadores
    public static bool ContieneOpp(string query)
    {
        //Reviso si contiene operadores
        if (query.Contains('~') || query.Contains('!') || query.Contains('^') || query.Contains('*'))
        {
            return true;
        }
        return false;
    }


    //CalculoIDF
    public static void CalidfOrg(List<DocumentP> ldoc)
    {
        //Voy iterando cada documento
        for (int i = 0; i < ldoc.Count; i++)
        {
            //Despues comienzo a iterar los Diccionarios que guardadan las palabras  de Cada Documento
            foreach (string term in Moogle.ldoc[i].Palabras.Keys)
            {
                //Reviso si mi Diccionario Gneral contiene las palabras del diccionario
                if (!Moogle.Gneral.ContainsKey(term))
                {
                    //En caso de no tenerla la agrega al diccionario y le da valor 1
                    Gneral.Add(term, 1);
                }
                else
                {
                    //En caso de tenerla le aumento 1 a su frecuencia global
                    Gneral[term]++;
                }
            }
        }
        //Aqui guardo en el diccionario idf el calculo del Idf general de las palabras en la baes de datos
        idf = CalculoIDF(Moogle.ldoc);
    }


    public static void Tf_Idf()
    {
        //declaro la variable calculo y la igualo a 0
        double calculo = 0;
        //comienzo a iterar cada documento
        for (int i = 0; i < ldoc.Count; i++)
        {
            //comienzo a iterar cada termino  el Diccionario Gneral donde se guarda la frecuencia Global de cada Doc
            foreach (string term in Gneral.Keys)
            {
                //Reviso si el diccionario tf de cada documento contiene el termino
                if (Moogle.ldoc[i].tf.ContainsKey(term))
                {
                    //Se guarda y realiza el calcuo
                    calculo = (double)Moogle.ldoc[i].tf[term] * (double)idf[term];
                    //Guardamos en cada Documento en un diccionario el Tf_idf de cada palabra por documento
                    Moogle.ldoc[i].CalTf_Idf.Add(term, calculo);
                }
            }
        }
    }

    //Metodo para revisar operadores en un arreglo 
    public static bool RevisarOperadores(string[] QueryConOperadores)
    {
        //Itero el arreeglo de string
        for (int i = 0; i < QueryConOperadores.Length; i++)
        {
            //Reviso si las palabras
            if (QueryConOperadores[i].Contains('!') || QueryConOperadores[i].Contains('^') || QueryConOperadores[i].Contains('~') || QueryConOperadores[i].Contains('*'))
            {
                return true;
            }
        }
        return false;
    }

    //Metodo para calcular el Idf de cada palabra
    static public Dictionary<string, double> CalculoIDF(List<DocumentP> ldoc)
    {
        //Declaro el diccionario idf  donde voy a guardar los resultados del calculo
        Dictionary<string, double> idf = new Dictionary<string, double>();
        //Itero cada palabra en el Diccionario Gneral
        foreach (string term in Moogle.Gneral.Keys)
        {
            //Guardo en la variable calculo el resultado
            double Calculo = 1 + Math.Log10(1 + (double)Moogle.ldoc.Count) / (Moogle.Gneral[term] + 1);
            //Agrego el resultado y el termino al cual se le realizo la division el Diccionarios
            idf.Add(term, Calculo);
        }
        //Retorno el Diccionario ya lleno
        return idf;

    }

    #region Sinpet

    //Metodo para crear el snipet
    public static void CrearSnipet(string query, string[] CadenasConOperadores, Dictionary<string, double> ResultFOperators, Dictionary<string, double> Scores)
    {
        //declaro un string
        string palabra = "";
        //declaro un entero
        int indice = 0;
        //reviso si contiene operadores
        if (RevisarOperadores(CadenasConOperadores))
        {
            //itero los documentos
            for (int i = 0; i < ldoc.Count; i++)
            {
                //itero las lineas de cada docmento
                for (int j = 0; j < ldoc[i].twordsS.Length; j++)
                {
                    //Reviso si el diccionario no tiene el titulo y si esta presente en el titulo
                    if (!Snipet.ContainsKey(ldoc[i].Tittle) && ResultFOperators.ContainsKey(ldoc[i].Tittle))
                    {
                        //Reviso si la linea contiene la palabra
                        if (ldoc[i].twordsS[j].Contains(query))
                        {
                            //Reviso si la palabra con los signos eliminados es igual a la plabra buscada
                            if (query == DocumentP.EliminarSignos(ldoc[i].twordsS[j]))
                            {
                                //Declaro una variable entera
                                indice = j;
                                //Reviso si la linea es mayor al indice + longitud 20
                                if (ldoc[i].twordsS.Length > indice + 20)
                                {
                                    //itero las proximas 20 posciones
                                    for (int d = indice; d < indice + 20; d++)
                                    {
                                        //Agrego cada palabra al string
                                        palabra += ldoc[i].twordsS[d] + " ";
                                    }
                                    //La guardo en el diccionario
                                    Snipet.Add(ldoc[i].Tittle, palabra);
                                    //vacio el string
                                    palabra = "";
                                }

                                else
                                {
                                    //itero la linea
                                    for (int k = 0; k < ldoc[i].twordsS.Length; k++)
                                    {
                                        //relleno el snipet hasta q termine los indices
                                        palabra += ldoc[i].twordsS[k] + ' ';
                                    }
                                    //Guardo el Snipet
                                    Snipet.Add(ldoc[i].Tittle, palabra);
                                    //Vacio el string
                                    palabra = "";
                                }
                            }
                        }

                    }
                }
            }
        }
        else
        {
            //itero los documentos
            for (int i = 0; i < ldoc.Count; i++)
            {
                //itero las lineas de cada docmento
                for (int j = 0; j < ldoc[i].twordsS.Length; j++)
                {
                    //Reviso si el diccionario no tiene el titulo y si esta presente en el titulo
                    if (!Snipet.ContainsKey(ldoc[i].Tittle) && Scores.ContainsKey(ldoc[i].Tittle))
                    {
                        //Reviso si la linea contiene la palabra
                        if (ldoc[i].twordsS[j].Contains(query))
                        {
                            //Reviso si la palabra con los signos eliminados es igual a la plabra buscada
                            if (query == DocumentP.EliminarSignos(ldoc[i].twordsS[j]))
                            {
                                //Declaro una variable entera
                                indice = j;
                                //Reviso si la linea es mayor al indice + longitud 20
                                if (ldoc[i].twordsS.Length > indice + 20)
                                {
                                    //itero las proximas 20 posciones
                                    for (int d = indice; d < indice + 20; d++)
                                    {
                                        //Agrego cada palabra al string
                                        palabra += ldoc[i].twordsS[d] + " ";
                                    }
                                    //La guardo en el diccionario
                                    Snipet.Add(ldoc[i].Tittle, palabra);
                                    //vacio el string
                                    palabra = "";
                                }
                                else
                                {
                                    //itero la linea
                                    for (int k = 0; k < ldoc[i].twordsS.Length; k++)
                                    {
                                        //relleno el snipet hasta q termine los indices
                                        palabra += ldoc[i].twordsS[k] + ' ';
                                    }
                                    //Guardo el Snipet
                                    Snipet.Add(ldoc[i].Tittle, palabra);
                                    //Vacio el string
                                    palabra = "";
                                }
                            }
                        }

                    }
                }
            }
        }
    }



    #endregion

    #region Sinonimos
    public static void LeerSinonimos()
    {
        string jsonstring = File.ReadAllText("..//sinonimos.json");
        Sinonimo sin = JsonSerializer.Deserialize<Sinonimo>(jsonstring);
        Sinonimo.ListSinom = sin.sinonimos;
    }

    public static string[] RellenarListasDeSin(string query, string[] ListSinom)
    {
        string[] sinonimos = new string[0];
        for (int i = 0; i < ListSinom.Length; i++)
        {
            sinonimos = sinonimos.Append(ListSinom[i]).ToArray();
        }
        return sinonimos;
    }




    #endregion

    #region Raices

    public static void CargarRaices_Y_Voc()
    {
        string Voc = File.ReadAllText("..//voc.txt");
        Raiz.Vocabulario = Voc.Split();
        string Rais = File.ReadAllText("..//salida.txt");
        Raiz.salida = Rais.Split();
    }

    public static int GuardArIndiceVoca(string query, string[] Voc)
    {
        int indice = -1;
        bool comprobar = true;
        for (int i = 0; i < Voc.Length; i++)
        {
            if (Voc[i] == query)
            {
                indice = i;
                comprobar = false;
            }
            if (comprobar == false) break;

        }
        return indice;
    }

    public static int[] CargarIndicesRaices(int indices, string[] Salidas)
    {
        string RaizInicio = "";
        int[] indicesFinales = new int[0];


        RaizInicio = Salidas[indices];

        if (indices > 30)
        {
            for (int i = indices - 30; i < Salidas.Length; i++)
            {
                if (Salidas[i] == RaizInicio)
                {
                    indicesFinales = indicesFinales.Append(i).ToArray();

                }

            }
        }
        else
        {
            for (int j = 0; j < Salidas.Length; j++)
            {
                if (Salidas[j] == RaizInicio)
                {
                    indicesFinales = indicesFinales.Append(j).ToArray();

                }

            }
        }

        return indicesFinales;
    }

    #endregion

}

public class Raiz
{
    public static string[] Vocabulario { get; set; }
    public static string[] salida { get; set; }
}












