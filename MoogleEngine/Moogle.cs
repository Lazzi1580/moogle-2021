using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
namespace MoogleEngine;


public static class Moogle
{
    public static Dictionary<string, double> Gneral = new Dictionary<string, double>();
    public static Dictionary<string, double> idf = new Dictionary<string, double>();
    public static List<DocumentP> ldoc = new List<DocumentP>();

    public static void StarIndex()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        BuscarEnDirectorio();
        foreach (var i in list)
        {
            DocumentP d = new DocumentP(File.ReadAllLines(i), i);
            ldoc.Add(d);
        }

    }
    public static SearchResult Query(string query)
    {
        QueryD QueryD_Object = new QueryD(query);
        SearchItem[] Resultados = new SearchItem[0];
        int totalD = 0;

        if (RevisarOperadores(QueryD_Object.CadenasConOperadores))
        {
            totalD = QueryD_Object.ResultFOperators.Count;
            Resultados = new SearchItem[totalD];
            {
                int i = 0;
                foreach (string term in QueryD_Object.ResultFOperators.Keys)
                {
                    Resultados[i] = new SearchItem(term, QueryD_Object.ResultFOperators[term] + "", QueryD_Object.ResultFOperators[term]);
                    i++;
                }
            }
        }
        else
        {
            totalD = QueryD_Object.Scores.Count;
            Resultados = new SearchItem[totalD];
            {
                int i = 0;
                foreach (string term in QueryD_Object.Scores.Keys)
                {
                    Resultados[i] = new SearchItem(term, QueryD_Object.Scores[term] + "", QueryD_Object.Scores[term]);
                    i++;
                }
            }
        }
        return new SearchResult(Resultados, QueryD.Sugerncia(query));
    }

    // Metodo para Buscar dentro de un  las respectivas busquedas del usuario
    public static void BuscarEnDirectorio()
    {
        string[] txts = CargarTxts("..//Content");

        Dictionary<string, double> GlobalFrec = new Dictionary<string, double>();
        for (int i = 0; i < txts.Length; i++)
        {
            GlobalFrec = GlobalFrecMet(txts, CargarPalabras(txts[i]));
            foreach (string term in GlobalFrec.Keys)
            {
                if (!Gneral.ContainsKey(term))
                {
                    Gneral.Add(term, (double)GlobalFrec[term]);
                }
            }
        }
        idf = CalculoIDF(txts);
    }

    public static bool RevisarOperadores(string[] QueryConOperadores)
    {
        for (int i = 0; i < QueryConOperadores.Length; i++)
        {
            if (QueryConOperadores[i].Contains('!') || QueryConOperadores[i].Contains('^'))
            {
                return true;
            }
        }
        return false;
    }




    static public Dictionary<string, double> CalculoIDF(string[] txts)
    {
        Dictionary<string, double> idf = new Dictionary<string, double>();
        foreach (string term in Moogle.Gneral.Keys)
        {
            double Calculo = 1 + Math.Log10(1 + (double)txts.Length) / (Moogle.Gneral[term] + 1);
            idf.Add(term, Calculo);
        }
        return idf;
    }


    public static Dictionary<string, double> GlobalFrecMet(string[] txts, string[] palabras)
    {
        Dictionary<string, double> GlobalFrec = new Dictionary<string, double>();
        string[] palabrasSplit = new string[0];
        string modificar = "";
        for (int i = 0; i < palabras.Length; i++)
        {
            modificar += palabras[i] + " ";
        }

        palabrasSplit = modificar.Split(' ');

        for (int i = 0; i < palabrasSplit.Length; i++)
        {

            if (!GlobalFrec.ContainsKey(palabrasSplit[i]))
            {
                if (palabrasSplit[i] != "")
                    GlobalFrec.Add(DocumentP.EliminarSignos(palabrasSplit[i]), ContadorPorDoc(txts, palabrasSplit[i]));
            }
        }
        return GlobalFrec;
    }

    public static string[] CargarTxts(string path)
    {
        string[] documentos = Directory.GetFiles(path);
        string[] txt = new string[0];

        for (int i = 0; i < documentos.Length; i++)
        {
            if (documentos[i].ToString().Contains(".txt"))
            {
                txt = txt.Append(documentos[i]).ToArray();
            }
        }

        return txt;
    }
    public static int ContadorPorDoc(string[] alltxt, string palabra)
    {
        int cnt = 0;

        for (int i = 0; i < alltxt.Length; i++)
        {
            string[] Normal = CargarPalabras(alltxt[i]);
            for (int j = 0; j < Normal.Length; j++)
            {
                if (Normal[j] == palabra)
                {
                    cnt++;
                    break;
                }
            }
        }
        return cnt;
    }

    public static string[] CargarPalabras(string txt)
    {
        string palabras = File.ReadAllText(txt);
        return QuitarEspaciosGG(palabras.Split(' '));
    }

    public static string[] QuitarEspaciosGG(string[] normallizacion)
    {
        List<string> normallizacion1 = new List<string>();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < normallizacion.Length; i++)
        {

            if (normallizacion[i] == "") continue;
            if (normallizacion[i][normallizacion[i].Length - 1] == '\n')
            {
                sb.Append(normallizacion[i]);
                sb.Replace("\n", "");
                if (sb.ToString() == "") continue;
                normallizacion1.Add(sb.ToString());
            }

            else normallizacion1.Add(normallizacion[i].ToLower());
        }
        string[] palabrasNormalizdas = new string[normallizacion1.Count];
        for (int i = 0; i < normallizacion1.Count; i++)
        {
            palabrasNormalizdas[i] = DocumentP.EliminarSignos(normallizacion1[i]);
        }
        return palabrasNormalizdas;
    }
}












