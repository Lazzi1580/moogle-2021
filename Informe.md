# M ⚯⚡gle! ⏃

![](MoogleHP.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

Buenas, durante el desarrollo de este informe sera explicado paso a paso el funcionamiento de mi Proyecto.

---

# Arquitectura básica y flujo de datos.

En primera instancia cuando se ejecuta el servidor comienza el método `StarIndex()`, el que da inicio el procesamiento de todos los documentos. Aquí doy uso de mi `clase DocumentP` que es en el cual básicamente ocurre toda la `“magia”`, se separan los documentos por título, se extraen las palabras, las líneas, se realiza un conteo de la frecuencia de las palabras tanto por documento individual como frecuencia global y se realizan los cálculos necesarios para obtener la importancia de las palabras en la base de datos.

Una vez finalizado este proceso ya se termina de ejecutar el servidor y la página está disponible para el uso del usuario.

Cuando el usuario hace una búsqueda se inicializa otro proceso, esta vez usamos la `clase Query`. Esta recoge la `query`, la desmantela en términos individuales y revisa si esta contiene operadores o no, en caso de no contenerlos toma los términos obtenidos anteriormente, los arregla y les hace una `“corrección ortográfica”` siempre y cuando alguno de los terminos de la `query` no este contenido en el texto. Después pasamos estos términos por un método que se encarga de buscar a partir de una base de datos las `raíces` de estos términos y agregar a la `query` algunas de las palabras que puedan ser `raíces` de los términos individuales anteriores, otra vez estos términos pasan por otro proceso pero esta vez es para buscar `sinónimos` de todos los nuevos términos obteniendo así una lista de palabras con su palabras parecidas o de igual significado. A cada término individual se le hace el mismo procesamiento que a todas las palabras de la base de datos considerando la query como un documento adicional.

Ahora que el programa ya ha calculado el `“peso”` de cada término es hora de encontrar la secuencia de documentos que se le debe mostrar al usuario. Para empezar a calcular los valores indicativos de cada documento usamos la clase SimCos Esta clase está basada en el algoritmo de `“Similitud del Coseno”`, en el cual doy uso de los valores obtenidos de los pesos de los términos de la query y los términos de la base datos donde a través de ciertos cálculos, cómo la norma de los vectores y multiplicación entre vectores obtenemos unos resultados los cuales estiman un valor que nos indican qué documentos tienen más relación con la `query`.

Una vez se tiene la secuencia de documentos que se le mostrará al usuario se empezará a construir la referencia de la `query` en cada documento usando el método `CrearSnipet()`.

Después se rellena cada objeto `SearchItem` con el titulo de cada documento, la referencia de la query en el documento y su valor de relación con la `query` y se ordena la forma en la que se van a reproducir frente al usuario con una `función landa`.

`Si la query contiene operadores de búsqueda ocurre el siguiente proceso:`
Todo el proceso de `raíces y sinónimos` es ignorado y este trabajará con la `query` original todo el proceso del cácalculo del peso de los términos y la `Similitud del Coseno` con los documentos. Después un método revisa qué operadores contiene y va desarrollando según el operador las funciones de estos variando así los valores que ya tenían asignados anteriormente los otros documentos y ahora si son rellanados los SearchItem con los nuevos valores y se reproducirán ante el usuario en la interfaz.

---

# Operadores

Con intenciones de ayudar al usuario a realizar consultas más específicas desarrollé el sistema de los operadores de búsqueda hasta ahora con 4 operadores funcionales.

`- Operador de Exclusión o “no debe estar” :` Explicado de forma sencilla, se le eliminan las tildes y los signos de puntuación al término que no debe estar, después reviso que documentos contienen el término que no debe aparecer, guardo el título de estos documentos y reviso si aparecen en los resultados previos que le voy a devolver al usuario en caso de que suceda les doy valor 0 evitando que estos documentos les desean devueltos al usuario.

```cs
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
```

---

`-Operador de “Tiene que aparecer” :` De nuevo se eliminan las tildes y los signos de puntuación al término que debe aparecer, se revisa en que documentos aparece, se guarda el título de estos documentos y se revisa si su score es 0 le aumentamos 1, después se revisa si en los documentos donde no aparece el score es mayor a 0 en caso de suceder esto se hace 0 su score.

```cs
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
```

---

`-Operador de “Importancia”` : Comienzo recorriendo el término que contiene el operador y cuento la cantidad de veces que se repite, cuando termina el conteo se elimina del termino los signos de puntuación y las tildes, se revisa que documentos contienen el término, se guarda el titulo, y revisamos los `scores` y le sumamos 1 por cada vez que se repetía el operador al score de los documentos que contienen el término.

```cs
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
```

---

# Estructura de la representación de los documentos y consultas.

## Class DocumentP y la Tokenizacion

En el inicio del informe se hace referencia a la `clase DocumentP` que según mis propias palabras es donde ocurre la `magia`. Cuando se ejecuta el servidor se crea un objeto `DocumentP` por cada documento .txt que se encuentre.

```cs
//Reviso la direccion en la cual se guardan los archivos y guardo solamente los que sean txts
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        //Voy iterando la variable var y guardando cada DocumentP en una lista de DocumentP
        foreach (var i in list)
        {
            DocumentP d = new DocumentP(File.ReadAllLines(i), i);
            //Agrego cada DocumentP a la lista
            ldoc.Add(d);
        }
```

---

Este objeto `DocumentP` contiene propiedades donde se guardan datos como la dirección donde esta contenida el archivo, el título del archivo, las líneas de cada documento, un Diccionario que contiene cada palabra individual del documento y cuantas veces esta repetida en este, lleva un conteo de la cantidad total de palabras del documento y por último un diccionario que contiene cada palabra con su `“importancia”` en el documento.

```cs
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

```

---

Como se aprecia en la imagen dentro del constructor se consta de 2 métodos no mencionados anteriormente `Token()` y `CalculoTf()`, ambos muy importantes en el proceso del calculado de la importancia de los términos del documento.

### Token()

En este método comenzamos iterando cada una de las líneas del documento y extraemos cada uno de los términos que estén separados por espacios, después volvemos a iterar pero esta vez sobre el array que contiene a los términos ya separados, vamos seleccionando cada término, le eliminamos los signos de puntuación y las tildes(en caso del término sea un espacio en blanco simplemente lo ignoramos), verificamos si está en nuestro diccionario general de palabras del documento, si no está la agregamos al diccionario y le damos valor 1, en caso de estar ya presente le sumamos 1 a su valor en el diccionario y de forma paralela cada termino la vamos agregando a una cadena separando cada término por espacios y le sumo uno a la propiedad que va contando la cantidad de palabras total del documento. Una vez terminado todo el proceso la cadena que contiene los términos la guardo en un arreglo de strings separando cada término de forma individual.

```cs
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

```

### CalculoTf()

Este es un método más sencillo en el cual solamente iteramos cada palabra del Diccionario de palabras de documento y dividimos su frecuencia en el documento entre la cantidad de palabras total del documento y guardamos el término y el valor en otro Diccionario destinado para almacenar estos valores.

```cs
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
```

---

## Class Query

En esta clase es básicamente donde ocurre todo el procesamiento de la `query` comenzando desde su descomposición en términos independites hasta encontrar sus `raíces y sinónimos`.

Comenzamos por el princpio, cuando el usario escribe la consulta un método trivial se encarnga de descomponerlo en términos independientes separados por espacios, a cada uno de estos términos se le eliminan los signos de puntuación y las tildes y estos quedan almacenados en un array de strings. Después le intentamos aplicar una `corrección ortográfica` siempre y cuando alguna de las consultas del usuario no se encuentre en la base de datos.

> ⚠️ Correción ortográfica!

### Sugerencia()

Esta frase ya ha sido usada más de una vez y aún no ha sido explicada. Bueno este es un método el cual se apoya en el `Algortimo de Levenshtein` para conseguir la palabra de mi base de datos más cercana o parecida posible a la consulta del usuario. Primero se somete al array de strings a un método trivial para ir seleccionando los términos de forma individual y se les pasa el metodo `Sugerncia()` el cual funciona de la siguiente manera:

Se le pasa un término individual y de la base de datos general se toman todos los términos que tengan 3+,3-,2+,2-,+1,1- y la misma cantidad de caracteres y se comparan utilizando el `Algortimo de Levenshtein` para conseguir de cuanto es la menor cantidad de cambios posibles para igualar ambos términos y los vamos guardando junto con las palabras en mismo índice respectivamente. Una vez culminado este proceso organizo los valores de la cantidad de cmbios posibles de menor a mayor y según vamos ordenando los valores los términos asignados a los mismos índices de estos también van cambiando.Despúes vuelvo a guardar los valores ya ordenados en otros arreglos junto con sus términos en busca de mejor organización. Una vez todos ordenados a cada uno de los términos le saco su `frecuencia global` y la guardo en un arreglo double en los indices correspondientes a los términos entonces en caso de que alguna de las palabras encontradas requiera 0 cambios esa es la que devolemos, si requieren 1 o más cambio devolemos la que mayor frecuencia global tenga de las que requieran un solo cambio y así susecivamente.

```cs
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
```

---

Ya finalizado todo el proceso de `correción ortográfica` se dispone a buscar las raíces de los términos individuales. Para esto me apoyé en 2 documentos .txt los cuales contenían la raíz de cada término y las palabras asociadas a estas raíces. Esto era posible gracias a un método bastante sencillo el cual consiste en simplemente llenar un arreglo de strings con las las cuales eran consideradas las raíces de estos términos pero para lograr esto primero era necesario conocer la raíz del término y extraer las palabras asociadas a estas raíces, para ellos se apoya en 2 métodos, el primero `GuardarIndices_de_Vocabulario` el cual trabaja de la siguiente forma:

### GuardarIndices_de_Vocabulario()

A este método de forma inicial le pasamos el término con el que va a trabajar y la lista de todas las palabras del vocabulario, después buscamos si la palabra esta contenida en el vocabulario en caso de estar guardamos el indice de donde se encuentra y rompemos el ciclo en ese punto, el método devuelve un entero el cual representa el índice de donde va estar contenida `la raíz` de este término en el otro txt.

```cs
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
```

---

El otro método en el cual se apoya en `CargarIndicesRaíces()`, este es simplemente para almacenar los índices en los que van a estar contenidos todas las palabras asociadas por `la raíz` del término inicial. Este funciona del siguiente modo:

### CargarIndicesRaíces()

A este método se le pasa el índice en cuestión a buscar y la lista donde están contenidas todas `las raíces` de palabras, se guarda en un string `la raíz` contenida en la lista en el índice en cuestión. Ahora sabemos que raíz buscamos solo queda encontrarla, para eso iteramos en la lista donde están contenidas las raíces, para acortar la búsqueda en caso de que el índice sea mayor que 30, inciamos la búsqueda apartir de 30 indíces anteriores, sino apartir de 0. Se va revisando cada índice y si uno de ellos correponde a `la raíz` que buscamos lo guardamos en un arreglo de enteros. Este es el que al final del método devolvemos al usuario.

```cs
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

```

---

Hasta este punto el programa contiene una query con los términos indiviudales de la query básica del usuario más `las raíces` de estos términos pero... Por qué dejarlo aquí? Mi programa además de extraer las palabras asociadas por las raíces también es capaz de extraer los `sinónimos` de dichos términos.

Esto lo hace de forma sencilla, carga con un método llamado `LeerSinonimos()` los archivos .json guardando el contenido de este en Listas de arreglos de string.

```cs
public static void LeerSinonimos()
    {
        string jsonstring = File.ReadAllText("..//sinonimos.json");
        Sinonimo sin = JsonSerializer.Deserialize<Sinonimo>(jsonstring);
        Sinonimo.ListSinom = sin.sinonimos;
    }
```

---

### LlenarConSinonimos()

Una vez ya tenemos la información cargada iteramos las listas de arreglos revisando si contienen el término a buscar, en caso de hacerlo rellenamos un arreglo de strings con sus `sinónimos` y eso es lo que devolvemos al usaurio, en caso de no tener `sinónimos` devolvemos lo términos que ya teníamos.

```cs
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
```

---

# Modelo de Funcionalidad del Rankinkg

Para la `Función del Rankinkg` mi proyecto se basa en el `Modelo Vectorial` para esto era necesario conseguir el `Tf_Idf` de cada término de la base de datos en el documento que le correspondía, así mismo con los términos de la query considerando esta como un documento más.

Al princpio cuando hicmos referncica a la `clase DocumentP` al final explicamos que calculamos el `tf` de cada palabra en sus respectivos documentos. Ahora para el calculo del `Tf_Idf` debemos de calcular el `Idf` de cada palabra. Para esto nos apoyamos en los documentos ya recopilados y vamos iterando sobre los diccionarios que contienes los términos de estos, si el diccionario en el que guardaremos la frecuencia global del documento no contiene un término lo agregamos y le damos valor 1,en caso de que ya la tenga agregada le sumamos 1 a su value, esto va asegurar que el conteo sea exacto ya que los diccionarios pueden contener un término no repetido evitando así que agregue más valor a una palabra que se encuentre 2 veces en el mismo documento.

### CalidfOrg()

```cs
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
```

---

### CalculoIDF()

Una vez terminamos el conteo, iteramos el nuevo diccionario y calculamos su `idf` hallando el logartimo en base 10 de la división de la cantidad total de documentos entre la frecuencia global del término en cuestión, para evitar obtener resultados infintos le sumamos 1 cada valor y así tenemos tanto el tf como el `idf` de cada término.

```cs
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
```

---

### Tf_Idf()

Una vez ya tenemos los valores del `Tf` y el `idf` de cada término solo queda multiplicarlo, el método `Tf_Idf()`se encarga de realizarlo, este selecciona cada término toma su valor del `tf` en el documento correspondiente y su valor de idf el cual es global y los multiplica y así obtenemos el peso de ese término en el documento

```cs
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

```

Una vez ya tenemos toda la base de datos `Indexada` y con el cálculo del peso de sus términos ya guardado podemos disponer como ocurre el mismo proceso para los términos de la `query`.

### CalculoTf_IdfQuery()

Para realizar el cálculo del `Tf_Idf` de la `query` selecionamos cada término de la `query` y buscamos si se encuentra en mi diccionario global en caso de no encontrarse esta query se ignora, en caso de que si con un metodo auxlizar contamos cuantas veces se repite el término en la `query` y después hallamos su `tf` y para culminar este resultado lo multiplicamos por el `idf`. y así obtenemos el `Tf_Idf` de cada término de la `query`, esto lo guardamos en un diccionario para que sea de fácil acceso.

```cs
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

```

---

## Class SimCos

Una vez ya obtenido tanto el peso de todos los términos de mis documentos y los pesos de los términos de la `query` le damos uso a la class SimCos, en esta están contenidos todos los métodos para a través de la fórmula de `similitud del coseno`.

Dicha clase se inicializa cuando es llamada por el método `CalScore()` contenido en la `class Query`, la cual llama directamente al método `Cos()` pasandole los valores de(un vector con el peso de todos los términos de la query,los valores de los pesos de cada término de la query y los valores del peso de cada término en sus respectivos documentos).

### Cos()

Este se apoya en 2 métodos auxiliares para la creación de los vectores y otros 2 para los cálculos de dicha fórmula.

```cs
//Metodo para encontarr la Similitud del Coseno
        public static double Coseno(double[] vectorQ, Dictionary<string, double> Query, Dictionary<string, double> tfidfDoc)
        {
            //Realizo la multiplicacion de los Vectores, la norma de los vectores y los divido para encontrar la similitud del coseno
            return MultiVectors(vectorQ, RellenarVectorDelCorpus(Query, tfidfDoc)) / (Norma(vectorQ) * Norma(RellenarCorpus(tfidfDoc)));
        }

```

---

### MultiVectors()

A este método se le pasa el `vectorQ` que contiene los pesos de cada `query` y un método auxiliar que rellena en un vector los pesos de los términos de la `query` contenidos en los documentos. Para así realizar la `multiplicación de vectores`

```cs
//Metodo para realizar la Multiplicacion de los vectores
static double MultiVectors(double[] Query, double[] Corpus)
{
    //Declaro una varibale double
    double Multi = 0;
           //Voy iternando la query
           for (int i = 0; i < Query.Length; i++)
           {
              //Multiplico cada peso de la query por cada peso del Corpus
               Multi += Query[i] \* Corpus[i];
           }

    return Multi;
}
```

---

### Norma

También para los cálculos necesarios para la fórmula es necesario encontrar la `norma` de los vectores que contienen el peso de los términos de la `query` y el peso de los términos de nuestra base de datos. Este método sencillo el cual consiste en ir hallando el cuadrado de cada valor e ir sumándolo y al final hallar la raíz cuadrada del resultado final y eso sería la `norma` del vector.

```cs
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
```

---

Ahora ya tenemos todo lo necesario, se efectuan los cálculos que se muestan e la fórmula del primer método y guardamos en un diccionario los Títulos de los documentos junto con su `score` pero solamente los que sean mayor a 0.

---

### Sinpet()

Para la representación de los documentos de forma que el usario pueda leer la representacion de la `query` en estos le doy uso a un método el cual nombré `CrearSnipett()`, este funciona de la siguiente manera, vamos iterando cada uno de los documentos, entonces comprobamos que el título del que vamos a crear la representación no fuese usado ya y comprobamos que este título de documento se encuentra etntre los documentos cuyo score es mayor a 0, si atraviesa esta barrera se revisa si la `query` esta contenida en alguna de las líneas del documento de ser así revisamos si es exacatemente igual o no a la `query` que buscamos, en caso de ser así guardamos esa posición y agragamos a un string la `query` más las próximas 19 palabras, esta lo guardamos en un diccionario junto al título del documento en que se encuentra y continuamos con la siguiente `query` de forma sucesiva.

```cs
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

```

---

Ya tenemos entonces los títulos de los documentos que le vamos a mostrar al usuario, la representación en texto del documento y el `socre` , ahora rellenamos los `SearchItem` con estos datos y con una `funcion landa` en el `class SearchResult` ordenamos la representación de los documentos de mayor a menor por su score para representarselo al usuario.

```cs
 public SearchResult(SearchItem[] items, string suggestion = "")
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }
        Array.Sort(items, (o1, o2) => o2.Score.CompareTo(o1.Score));

        this.items = items;
        this.Suggestion = suggestion;
    }

```

---

## Métodos para los operadores

Para el trabajo con opeadores primeramente se creó un metodo bool para encontar si hay presente un operador o no, en caso de exisitir todos los procesos de la query como sinónimos y raíces son ignorados y se realizan los cálculos con las `querys` originales

```cs
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

```

Para los cambios que realizan los distintos operadores se utiliza un diccionario auxiliar el cual contiene los títulos de todos los documentos así sus scores sean 0 o no. Este diccionario se llena a la misma vez que el del score de las `query` sin las modificaciones.

```cs
//Realizo el calculo de la sim del coseno llamando al metodo de la clase SimCos
                Csocre = SimCos.Coseno(vectorQ, CalculoTf_IdfQueryD, Moogle.ldoc[i].CalTf_Idf);
                //Lo guardo en el diccionario Operadores el cual voy a utilizar solo en caso de que la query tenga Operadores de busqueda
                Operators.Add(Moogle.ldoc[i].Tittle, Csocre);
```

El método Colleccion() es el que se encarga de que se ejecuten todos los operadores ya explicados anteriormente en el proyecto.

```cs
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
```

A la hora de crear el `Snipett` para cuando hay operadores presentes funciona de igual forma que sin ue existiesen solo cambia el hecho de que ya no se trabaja con el Diccionario Scores sino con otro que ya tiene todos los valores modificados difrentes de 0 que se le mostraran al usuario.

```cs
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
```

---

Con esto finaliza el informe de mi proyecto, espero recibir críticas constructivas que me ayuden a mejorar como desarrollador de Software y como Científico de la Computación. Muchas gracias por la atención.
