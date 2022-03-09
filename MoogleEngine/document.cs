using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace MoogleEngine
{
    public class DocumentP
    {
        //Propiedades***

        //Propiedad donde voy realizando el conteo de documentoss
        static public int cnt = 0;
        //Propiedad donde voy realizando el conteo de palabras por cada documento
        public int cntw = 0;
        //Propiedad done guardo el path o direccion de los documentos
        public string Path;
        //Propiedad donde guardo el Titulo de cada documento
        public string Tittle;
        //Propiedad donde guardo cada palabra de cada documento y su frecuencia en este
        public Dictionary<string, int> Palabras = new Dictionary<string, int>();
        //Propiedad donde guardo el tf de cada palabra en el DocumentoC
        public Dictionary<string, double> tf = new Dictionary<string, double>();
        //Propiedad donde guardo el Tf_idf de cada palabra en el Documento
        public Dictionary<string, double> CalTf_Idf = new Dictionary<string, double>();
        //Propiedad donde guardo las lineas de los Documentoss
        public string[] twordsS;
        //Propiedad donde guardo el snipet a devolver al usuario de cada documento


        public DocumentP(string[] words, string Tittle)
        {
            this.Path = Tittle;
            this.Tittle = Tittle;
            this.twordsS = twordsS;
            this.Palabras = Palabras;
            this.cntw = cntw;
            this.CalTf_Idf = CalTf_Idf;
            Token(words);
            CalculoTf(words);
            cnt++;

        }

        //Metodo donde se realiza la Tokenizacion
        public void Token(string[] words)
        {
            //Declaro un arreglo de strings
            string[] twordsSS;
            //Declaro un string vacio
            string palabra = " ";
            //Comienzo a iterar el arreglo de string que contiene las lineas del codigo
            for (int i = 0; i < words.Length; i++)
            {
                //Separo cada palabra por espacios
                twordsSS = words[i].Split(' ');
                //Comienzo a iterar el arreglo de strings que va conteniendo las palabras del documento
                for (int j = 0; j < twordsSS.Length; j++)
                {
                    //Condicional para aseguar no guardar espacios en blanco
                    if (twordsSS[j] == "")
                    {
                        //Si lo es la ignora y brinca esa iteración
                        continue;
                    }

                    //A cada palabra le elimino los Signos de Puntuación
                    twordsSS[j] = EliminarSignos(twordsSS[j]);
                    //Agrego a el string vacio cada palabra separandolo por un espacio
                    palabra += twordsSS[j] + " ";
                    //Guardo la palabra en un string terms
                    string term = twordsSS[j];
                    //Vuevlo a revisar si no es un espacio en blanco ya que por respectivos cambios puede haber llegado a ese estado
                    if (term != "")
                    {
                        //Si no es un espacio en blanco reviso si ya el Diccionario Palabras
                        if (!Palabras.ContainsKey(term))
                        {
                            //Si no la contiene la agrego y le doy valor 1
                            Palabras.Add(twordsSS[j], 1);
                            //Le aumento 1 a la propiedad que va realizando el conteo de plaabras
                            cntw++;
                        }
                        else
                        {
                            //Si ya lo contiene le aumento uno a la frecuencia de la palabra en el documentoss
                            Palabras[term]++;
                            //Le aumento 1 a la propiedad que va realizando el conteo de plaabras
                            cntw++;
                        }
                    }
                }
            }
            //Guardo en el arreglo de string todo el string que iba guardando de todas las palabras separado por espacios
            twordsS = palabra.Split();
        }

        //Metodo para eliminar los signos de puntuacion dentro y alrededor de la palabra
        public static string EliminarSignos(string palabra)
        {
            //Creo un objeto StringBulider
            var sb = new StringBuilder();
            //Inicializo una variable entera en 0
            int i = 0;
            //Itero cada caracter de la palabra
            foreach (char c in palabra)
            {
                //Reviso si el caracter es uno de los Operadores de Busqueda
                if (c == '!' || c == '^')
                {
                    //Si lo es lo ignoro y paso al siguiente caracter
                    i++;
                    continue;
                }
                //Reviso que no sea un Signo de Puntuacion y que sea letra o digito
                if (!char.IsPunctuation(c) && char.IsLetterOrDigit(c))
                {
                    //Si es una letra o digito la agrego al objeto
                    sb.Append(c);
                }
                else
                {
                    {
                        //Sino la ignoro y paso al siguiente caracter
                        i++;
                        continue;
                    }
                }
            }
            //Convierto el ojbeto en un string y lo guardo
            palabra = sb.ToString();
            //Elimino las tildes de la palabra
            string palabra1 = EliminarTildes(palabra).ToLower();
            //Reviso si tiene un caracter en blanco al final y lo elimino
            string palabra2 = palabra1.TrimEnd(' ');
            //Reviso si tiene un caracter en blanco al principio y lo elimino
            string palabra3 = palabra2.TrimStart(' ');
            //Retorno la palabra
            return palabra3;
        }

        //Metodo para eliminar las tildes de las palabras
        public static string EliminarTildes(string palabra)
        {
            //Elimina las tildes de la palabra usando (expresiones regulares)System.Text.RegularExpressions;
            return Regex.Replace(palabra.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
        }


        //Metodo para calcular el Tf de cada palabra en su documento
        private void CalculoTf(string[] palabra)
        {
            //Declaro variable para la frec
            double frec = 0;
            //Itero el Diccionario Palabras por cada palabra que este contiene
            foreach (string term in Palabras.Keys)
            {
                //Igualo la variable a la frecuencia de la palabra en el documento
                frec = (double)Palabras[term];
                //Realizo el calculo del Tf
                double TF = (double)frec / (double)cntw;
                // Y agrego al diccionario tf la palabra y el resultado del calculo
                tf.Add(term, TF);
            }
        }


    }
}

