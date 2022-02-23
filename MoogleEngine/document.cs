using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace MoogleEngine
{
    public class DocumentP
    {
        List<DocumentP> Documentos = new List<DocumentP>();
        static public int cnt = 0;
        public int cntw = 0;
        public string Path;
        public string Tittle;
        public Dictionary<string, int> Words = new Dictionary<string, int>();
        public static Dictionary<string, double> Globalfreq = new Dictionary<string, double>();
        public Dictionary<string, double> tf = new Dictionary<string, double>();
        public Dictionary<string, double> CalTf_Idf = new Dictionary<string, double>();
        public double score;

        public DocumentP(string[] words, string Tittle)
        {
            this.Path = Tittle;
            this.Tittle = Tittle;
            Token(words);
            cnt++;
            cntw = Contador(words);
            CalculoTf(words);
            Tf_Idf();
            //Imprimir();
        }


        public void Token(string[] words)
        {
            string[] twords;

            for (int i = 0; i < words.Length; i++)
            {
                twords = words[i].Split(' ');

                for (int j = 0; j < twords.Length; j++)
                {
                    twords[j] = EliminarSignos(twords[j]);
                    string term = twords[j];
                    if (term != "")
                    {
                        if (!Words.ContainsKey(term))
                        {
                            Words.Add(twords[j], 1);
                        }
                        else
                        {
                            Words[term]++;
                        }
                    }
                }
            }
        }



        public static int ContadorPorDoc(string[] alltxt, string[] todas, string palabra)
        {
            int cnt = 0;

            for (int i = 0; i < alltxt.Length; i++)
            {
                for (int j = 0; j < todas.Length; j++)
                {
                    if (todas[j] == palabra)
                    {
                        cnt++;
                        break;
                    }
                }
            }
            return cnt;
        }
        public static int Contador(string[] palabras)
        {
            string[] twords = new string[palabras[0].Split(' ').Length];
            List<string> normallizacion = new List<string>();
            int cnt = 0;
            for (int i = 0; i < palabras.Length; i++)
            {
                twords = palabras[i].Split(' ');
            }
            for (int i = 0; i < twords.Length; i++)
            {
                normallizacion.Add(twords[i]);
            }
            normallizacion = QuitarEspacios(normallizacion);

            cnt = normallizacion.Count;
            return cnt;
        }

        public static List<string> QuitarEspacios(List<string> normallizacion)
        {
            List<string> normallizacion1 = new List<string>();
            for (int i = 0; i < normallizacion.Count; i++)
            {
                if (normallizacion[i] == "") continue;
                else normallizacion1.Add(normallizacion[i]);
            }
            return normallizacion1;
        }

        public static string EliminarSignos(string palabra)
        {
            var sb = new StringBuilder();
            int i = 0;

            foreach (char c in palabra)
            {
                if (c == '!' || c == '^')
                {
                    i++;
                    continue;
                }
                if (!char.IsPunctuation(c) && char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (i < 1)
                    {
                        sb.Append(" ");
                        i++;
                    }
                }
            }

            palabra = sb.ToString();
            string palabra1 = EliminarTildes(palabra).ToLower();
            string palabra2 = palabra1.TrimEnd(' ');
            string palabra3 = palabra2.TrimStart(' ');
            return palabra3;
        }


        public static string EliminarTildes(string palabra)
        {
            return Regex.Replace(palabra.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
        }



        private void CalculoTf(string[] palabra)
        {
            double count = 0;
            foreach (string term in Words.Keys)
            {
                count = (double)Words[term];
                double TF = (double)count / (double)Contador(palabra);
                tf.Add(term, TF);
            }
        }

        private void Imprimir()
        {
            foreach (var j in CalTf_Idf)
            {
                System.Console.WriteLine(j.Key + " " + j.Value);
            }
            System.Console.WriteLine("\n");
        }


        public void Tf_Idf()
        {
            double calculo = 0;
            double[] vector = new double[cntw];
            int i = 0;
            foreach (string term in tf.Keys)
            {
                if (!CalTf_Idf.ContainsKey(term) && Moogle.idf.ContainsKey(term))
                {
                    calculo = (double)tf[term] * (double)Moogle.idf[term];
                    CalTf_Idf.Add(term, calculo);
                    i++;
                }
            }
        }
    }
}
